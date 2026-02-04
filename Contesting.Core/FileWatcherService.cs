using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Contesting.Core;

public class FileWatcherService : BackgroundService
{
    private readonly ILogger<FileWatcherService> _logger;
    private readonly TestRunner _testRunner;
    private readonly CoverletService _coverletService;
    private readonly CoverageOutputService _coverageOutputService;
    private readonly string _watchPath;
    private FileSystemWatcher? _watcher;
    private DateTime _lastProcessedTime = DateTime.MinValue;
    private readonly object _lockObject = new();

    public FileWatcherService(
        ILogger<FileWatcherService> logger,
        TestRunner testRunner,
        CoverletService coverletService,
        CoverageOutputService coverageOutputService,
        string watchPath = ".")
    {
        _logger = logger;
        _testRunner = testRunner;
        _coverletService = coverletService;
        _coverageOutputService = coverageOutputService;
        _watchPath = Path.GetFullPath(watchPath);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting file watcher on path: {Path}", _watchPath);

        _watcher = new FileSystemWatcher(_watchPath);
        _watcher.Filter = "*.cs";
        _watcher.IncludeSubdirectories = true;
        _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Renamed += OnFileRenamed;
        _watcher.Error += OnError;

        _logger.LogInformation("File watcher configured. Filter: {Filter}, IncludeSubdirectories: {IncludeSubdirectories}",
            _watcher.Filter, _watcher.IncludeSubdirectories);

        _watcher.EnableRaisingEvents = true;
        _logger.LogInformation("File watcher events enabled - watching for .cs file changes");

        return Task.CompletedTask;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // Skip obj and bin directories
        if (e.FullPath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
            e.FullPath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
        {
            return;
        }

        _logger.LogInformation("File changed: {FilePath} ({ChangeType})", e.FullPath, e.ChangeType);
        _ = Task.Run(() => ProcessFileChange(e.FullPath));
    }

    private async Task ProcessFileChange(string filePath)
    {
        // Debounce - avoid processing rapid file changes
        lock (_lockObject)
        {
            var now = DateTime.UtcNow;
            if ((now - _lastProcessedTime).TotalMilliseconds < 500)
            {
                return;
            }
            _lastProcessedTime = now;
        }

        await Task.Delay(500); // Additional debounce delay

        _logger.LogInformation("Processing file change for: {FilePath}", filePath);

        try
        {
            _logger.LogInformation("File changed, rebuilding and running tests...");

            // Step 1: Build the solution
            _logger.LogInformation("Building solution...");
            var buildResult = await RunBuildCommand();
            if (!buildResult)
            {
                _logger.LogWarning("❌ Build failed after file change");
                return;
            }

            // Step 2: Run tests with coverage collection
            var testResult = await _testRunner.RunTestsAsync(collectCoverage: true);

            if (testResult)
            {
                _logger.LogInformation("✅ Tests passed after file change");

                // Show coverage summary
                var coverage = _coverletService.GetLatestCoverageResult();
                _coverletService.LogCoverageSummary(coverage);

                // Write detailed coverage JSON for IDE plugins
                var detailedCoverage = _coverletService.GetLatestCoverageResultDetail();
                _coverageOutputService.WriteJsonOutput(detailedCoverage);
            }
            else
            {
                _logger.LogWarning("❌ Tests failed after file change");
            }

            // Cleanup old test results occasionally
            _coverletService.CleanupOldResults();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file change for {FilePath}", filePath);
        }
    }

    private async Task<bool> RunBuildCommand()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build --nologo -v q",
            WorkingDirectory = _watchPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null) return false;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                _logger.LogDebug("Build succeeded");
                return true;
            }
            else
            {
                _logger.LogWarning("Build failed with exit code {ExitCode}", process.ExitCode);
                if (!string.IsNullOrWhiteSpace(error))
                    _logger.LogWarning("Build error: {Error}", error);
                if (!string.IsNullOrWhiteSpace(output))
                    _logger.LogWarning("Build output: {Output}", output);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running build command");
            return false;
        }
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        // Skip obj and bin directories
        if (e.FullPath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
            e.FullPath.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
        {
            return;
        }

        _logger.LogInformation("File renamed: {OldPath} -> {NewPath}", e.OldFullPath, e.FullPath);
        _ = Task.Run(() => ProcessFileChange(e.FullPath));
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "File watcher error occurred");
    }

    public override void Dispose()
    {
        _watcher?.Dispose();
        base.Dispose();
    }
}
