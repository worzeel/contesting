using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Contesting.Core;

public class TestFailure
{
    public string TestName { get; set; } = "";
    public string FailureReason { get; set; } = "";
}

public class TestRunner
{
    private readonly ILogger<TestRunner> _logger;
    private readonly string _workingDirectory;

    public TestRunner(ILogger<TestRunner> logger, string workingDirectory = ".")
    {
        _logger = logger;
        _workingDirectory = Path.GetFullPath(workingDirectory);
    }

    public async Task<bool> RunTestsAsync(string? filter = null)
    {
        _logger.LogInformation("Running tests{Filter}", string.IsNullOrEmpty(filter) ? "" : $" with filter: {filter}");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = string.IsNullOrEmpty(filter) ? "test --no-build" : $"test --no-build --filter \"{filter}\"",
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
                _logger.LogError("Failed to start test process");
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Tests passed");
                _logger.LogDebug("Test output: {Output}", output);
                return true;
            }
            else
            {
                _logger.LogWarning("Tests failed with exit code {ExitCode}", process.ExitCode);
                
                // Extract and log failing tests
                var failingTests = ExtractFailingTests(output);
                if (failingTests.Any())
                {
                    _logger.LogWarning("❌ Failing tests:");
                    foreach (var failingTest in failingTests)
                    {
                        _logger.LogWarning("   • {TestName}: {FailureReason}", failingTest.TestName, failingTest.FailureReason);
                    }
                }
                
                _logger.LogDebug("Full test output: {Output}", output);
                if (!string.IsNullOrWhiteSpace(error))
                    _logger.LogDebug("Test error: {Error}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running tests");
            return false;
        }
    }

    private List<TestFailure> ExtractFailingTests(string testOutput)
    {
        var failures = new List<TestFailure>();
        
        if (string.IsNullOrEmpty(testOutput))
            return failures;

        var lines = testOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Look for "Failed TestName [< time]" pattern from dotnet test output
            var failedMatch = Regex.Match(line, @"^\s*Failed\s+(.+?)\s*\[");
            if (failedMatch.Success)
            {
                var testName = failedMatch.Groups[1].Value.Trim();
                var failureDetails = new List<string>();
                
                // Look for "Error Message:" section and capture details
                for (int j = i + 1; j < Math.Min(i + 20, lines.Length); j++)
                {
                    var nextLine = lines[j].Trim();
                    
                    if (nextLine == "Error Message:")
                    {
                        // Capture the actual error message on next lines
                        for (int k = j + 1; k < Math.Min(j + 10, lines.Length); k++)
                        {
                            var errorLine = lines[k].Trim();
                            if (string.IsNullOrEmpty(errorLine) || errorLine == "Stack Trace:")
                                break;
                            if (!errorLine.StartsWith("at ")) // Don't include stack trace lines
                                failureDetails.Add(errorLine);
                        }
                        break;
                    }
                    
                    // Stop if we hit another test result or section
                    if (nextLine.StartsWith("Failed ") || nextLine.StartsWith("Passed ") || 
                        nextLine.Contains("Test run") || nextLine.Contains("Total tests"))
                    {
                        break;
                    }
                }
                
                var failureReason = failureDetails.Any() 
                    ? string.Join(" ", failureDetails)
                    : "Test failed";
                
                failures.Add(new TestFailure 
                { 
                    TestName = testName, 
                    FailureReason = failureReason 
                });
            }
            
            // Alternative pattern for xUnit output: "TestName [FAIL]"
            var xunitFailMatch = Regex.Match(line, @"^\s*(.+?)\s*\[FAIL\]");
            if (xunitFailMatch.Success && !line.Contains("xUnit.net"))
            {
                var testName = xunitFailMatch.Groups[1].Value.Trim();
                // Don't duplicate if we already found this test
                if (!failures.Any(f => f.TestName == testName))
                {
                    failures.Add(new TestFailure 
                    { 
                        TestName = testName, 
                        FailureReason = "Test failed" 
                    });
                }
            }
        }
        
        return failures;
    }
}