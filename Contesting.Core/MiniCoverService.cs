using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Contesting.Core;

public class MiniCoverService
{
    private readonly ILogger<MiniCoverService> _logger;
    private readonly string _workingDirectory;
    private readonly string _miniCoverPath;

    public MiniCoverService(ILogger<MiniCoverService> logger, string workingDirectory = ".")
    {
        _logger = logger;
        _workingDirectory = Path.GetFullPath(workingDirectory);
        _miniCoverPath = FindMiniCoverPath();
    }

    private static string FindMiniCoverPath()
    {
        // Try to find minicover in PATH first
        var pathVar = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathVar))
        {
            foreach (var path in pathVar.Split(Path.PathSeparator))
            {
                var fullPath = Path.Combine(path, OperatingSystem.IsWindows() ? "minicover.exe" : "minicover");
                if (File.Exists(fullPath))
                    return fullPath;
            }
        }

        // Fallback to common dotnet tools location
        var toolsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools", OperatingSystem.IsWindows() ? "minicover.exe" : "minicover");
        return File.Exists(toolsPath) ? toolsPath : "minicover";
    }

    public async Task<bool> InstrumentAsync()
    {
        _logger.LogInformation("Instrumenting assemblies for coverage analysis");
        return await RunMiniCoverCommand("instrument", "--sources \"**/*.cs\" --tests \"**/*Tests.cs\" --exclude-sources \"**/obj/**/*.cs\" --exclude-tests \"**/obj/**/*.cs\"");
    }

    public async Task<bool> ResetAsync()
    {
        _logger.LogInformation("Resetting coverage hits");
        return await RunMiniCoverCommand("reset", "");
    }

    public async Task<bool> UninstrumentAsync()
    {
        _logger.LogInformation("Removing instrumentation");
        return await RunMiniCoverCommand("uninstrument", "");
    }

    public async Task<bool> GenerateReportAsync()
    {
        _logger.LogInformation("Generating coverage report");
        return await RunMiniCoverCommand("report", "--threshold 0");
    }

    private async Task<bool> RunMiniCoverCommand(string command, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _miniCoverPath,
            Arguments = $"{command} {arguments}",
            WorkingDirectory = _workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start minicover process");
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                _logger.LogDebug("MiniCover {Command} succeeded", command);
                if (!string.IsNullOrWhiteSpace(output))
                    _logger.LogDebug("Output: {Output}", output);
                return true;
            }
            else
            {
                _logger.LogWarning("MiniCover {Command} failed with exit code {ExitCode}", command, process.ExitCode);
                if (!string.IsNullOrWhiteSpace(output))
                    _logger.LogDebug("Output: {Output}", output);
                if (!string.IsNullOrWhiteSpace(error))
                    _logger.LogDebug("Error: {Error}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running MiniCover {Command}", command);
            return false;
        }
    }
}