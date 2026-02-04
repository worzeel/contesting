using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Contesting.Core;

// Parse command line arguments
if (args.Length == 0)
{
    Console.WriteLine("Usage: Contesting <target-directory>");
    Console.WriteLine("  target-directory: Path to the C# solution/project directory to monitor");
    Console.WriteLine();
    Console.WriteLine("Example: Contesting /path/to/MyApp");
    return;
}

var targetDirectory = Path.GetFullPath(args[0]);
if (!Directory.Exists(targetDirectory))
{
    Console.WriteLine($"Error: Directory '{targetDirectory}' does not exist.");
    return;
}

var builder = Host.CreateApplicationBuilder();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Register services with the target directory
builder.Services.AddSingleton<TestRunner>(provider =>
    new TestRunner(provider.GetRequiredService<ILogger<TestRunner>>(), targetDirectory));
builder.Services.AddSingleton<CoverletService>(provider =>
    new CoverletService(provider.GetRequiredService<ILogger<CoverletService>>(), targetDirectory));
builder.Services.AddSingleton<CoverageOutputService>(provider =>
    new CoverageOutputService(provider.GetRequiredService<ILogger<CoverageOutputService>>(), targetDirectory));
builder.Services.AddHostedService<FileWatcherService>(provider =>
    new FileWatcherService(
        provider.GetRequiredService<ILogger<FileWatcherService>>(),
        provider.GetRequiredService<TestRunner>(),
        provider.GetRequiredService<CoverletService>(),
        provider.GetRequiredService<CoverageOutputService>(),
        targetDirectory));

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting Contesting background test runner for directory: {Directory}", targetDirectory);

// Check if target directory has a solution or projects
var solutionFiles = Directory.GetFiles(targetDirectory, "*.sln");
var projectFiles = Directory.GetFiles(targetDirectory, "*.csproj", SearchOption.AllDirectories);

if (solutionFiles.Length == 0 && projectFiles.Length == 0)
{
    logger.LogError("No .sln or .csproj files found in target directory. This doesn't appear to be a C# project.");
    return;
}

logger.LogInformation("Found {SolutionCount} solution(s) and {ProjectCount} project(s)", solutionFiles.Length, projectFiles.Length);

// Initial setup - build and run tests
var testRunner = host.Services.GetRequiredService<TestRunner>();
var coverletService = host.Services.GetRequiredService<CoverletService>();
var coverageOutputService = host.Services.GetRequiredService<CoverageOutputService>();

logger.LogInformation("Building solution...");
var buildResult = await RunCommand("dotnet", "build", targetDirectory);
if (!buildResult)
{
    logger.LogError("Build failed. Exiting.");
    return;
}

logger.LogInformation("Running initial tests with coverage...");
var testsPassed = await testRunner.RunTestsAsync(collectCoverage: true);

// Show initial coverage summary
var coverage = coverletService.GetLatestCoverageResult();
coverletService.LogCoverageSummary(coverage);

// Write detailed coverage JSON for IDE plugins
var detailedCoverage = coverletService.GetLatestCoverageResultDetail();
coverageOutputService.WriteJsonOutput(detailedCoverage);

logger.LogInformation("Starting file monitoring...");
await host.RunAsync();

static async Task<bool> RunCommand(string fileName, string arguments, string workingDirectory = ".")
{
    var startInfo = new System.Diagnostics.ProcessStartInfo
    {
        FileName = fileName,
        Arguments = arguments,
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using var process = System.Diagnostics.Process.Start(startInfo);
    if (process == null) return false;

    await process.WaitForExitAsync();
    return process.ExitCode == 0;
}
