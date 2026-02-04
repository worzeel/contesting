using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Contesting.Core;

/// <summary>
/// Writes coverage results to JSON for IDE plugin consumption
/// </summary>
public class CoverageOutputService
{
    private readonly ILogger<CoverageOutputService> _logger;
    private readonly string _workingDirectory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public CoverageOutputService(ILogger<CoverageOutputService> logger, string workingDirectory = ".")
    {
        _logger = logger;
        _workingDirectory = Path.GetFullPath(workingDirectory);
    }

    /// <summary>
    /// Writes detailed coverage result to TestResults/latest-coverage.json
    /// </summary>
    public void WriteJsonOutput(CoverageResultDetail? result)
    {
        if (result == null)
        {
            _logger.LogDebug("No coverage result to write to JSON");
            return;
        }

        try
        {
            // Find TestResults directory
            var testResultsDirs = Directory
                .GetDirectories(_workingDirectory, "TestResults", SearchOption.AllDirectories)
                .Where(d => !d.Contains("/bin/") && !d.Contains("/obj/"))
                .ToList();

            if (!testResultsDirs.Any())
            {
                _logger.LogWarning("Cannot write JSON output - no TestResults directory found");
                return;
            }

            // Use the first TestResults directory (typically at solution root)
            var testResultsPath = testResultsDirs.First();
            var outputPath = Path.Combine(testResultsPath, "latest-coverage.json");

            var json = JsonSerializer.Serialize(result, JsonOptions);
            File.WriteAllText(outputPath, json);

            _logger.LogDebug("Coverage JSON written to {Path}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write coverage JSON output");
        }
    }
}
