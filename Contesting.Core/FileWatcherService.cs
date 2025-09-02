using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Contesting.Core;

public class FileWatcherService : BackgroundService
{
    private readonly ILogger<FileWatcherService> _logger;
    private readonly TestRunner _testRunner;
    private readonly MiniCoverService _miniCoverService;
    private readonly string _watchPath;
    private FileSystemWatcher? _watcher;

    public FileWatcherService(ILogger<FileWatcherService> logger, TestRunner testRunner, MiniCoverService miniCoverService, string watchPath = ".")
    {
        _logger = logger;
        _testRunner = testRunner;
        _miniCoverService = miniCoverService;
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

        _logger.LogInformation("File watcher configured. Filter: {Filter}, IncludeSubdirectories: {IncludeSubdirectories}", _watcher.Filter, _watcher.IncludeSubdirectories);

        _watcher.EnableRaisingEvents = true;
        _logger.LogInformation("File watcher events enabled");

        return Task.CompletedTask;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogInformation("File changed: {FilePath} ({ChangeType})", e.FullPath, e.ChangeType);
        
        // TODO: Analyze file changes and run relevant tests
        _ = Task.Run(() => ProcessFileChange(e.FullPath));
    }

    private async Task ProcessFileChange(string filePath)
    {
        // Add debouncing to avoid processing rapid file changes
        await Task.Delay(500);
        
        _logger.LogInformation("Processing file change for: {FilePath}", filePath);
        
        try
        {
            _logger.LogInformation("File changed, rebuilding and running tests...");
            
            // Step 1: Uninstrument previous instrumentation
            await _miniCoverService.UninstrumentAsync();
            
            // Step 2: Build the solution to include changes
            _logger.LogInformation("Building solution with changes...");
            var buildResult = await RunBuildCommand();
            if (!buildResult)
            {
                _logger.LogWarning("❌ Build failed after file change");
                return;
            }
            
            // Step 3: Re-instrument with new assemblies
            await _miniCoverService.InstrumentAsync();
            await _miniCoverService.ResetAsync();
            
            // Step 4: Run tests
            var testResult = await _testRunner.RunTestsAsync();
            
            if (testResult)
            {
                _logger.LogInformation("✅ Tests passed after file change");
            }
            else
            {
                _logger.LogWarning("❌ Tests failed after file change");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file change for {FilePath}", filePath);
        }
    }

    private async Task<bool> RunBuildCommand()
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build",
            WorkingDirectory = _watchPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = System.Diagnostics.Process.Start(startInfo);
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