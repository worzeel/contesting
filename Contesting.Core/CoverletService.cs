using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Contesting.Core;

public class CoverageResult
{
    public double LineCoverage { get; set; }
    public double BranchCoverage { get; set; }
    public int CoveredLines { get; set; }
    public int TotalLines { get; set; }
    public int CoveredBranches { get; set; }
    public int TotalBranches { get; set; }
    public List<FileCoverage> Files { get; set; } = new();
}

public class FileCoverage
{
    public string FilePath { get; set; } = "";
    public double LineCoverage { get; set; }
    public int CoveredLines { get; set; }
    public int TotalLines { get; set; }
    public List<int> UncoveredLines { get; set; } = new();
}

public class CoverletService
{
    private readonly ILogger<CoverletService> _logger;
    private readonly string _workingDirectory;

    public CoverletService(ILogger<CoverletService> logger, string workingDirectory = ".")
    {
        _logger = logger;
        _workingDirectory = Path.GetFullPath(workingDirectory);
    }

    /// <summary>
    /// Finds and parses the most recent coverage.cobertura.xml file from TestResults
    /// </summary>
    public CoverageResult? GetLatestCoverageResult()
    {
        // Search for TestResults directories recursively (they're often in test project dirs)
        var testResultsDirs = Directory
            .GetDirectories(_workingDirectory, "TestResults", SearchOption.AllDirectories)
            .Where(d => !d.Contains("/bin/") && !d.Contains("/obj/"))
            .ToList();

        if (!testResultsDirs.Any())
        {
            _logger.LogWarning("📊 Coverage: No TestResults directories found under {Path}", _workingDirectory);
            return null;
        }

        // Find the most recent coverage file across all TestResults directories
        var coverageFiles = testResultsDirs
            .SelectMany(dir => Directory.GetFiles(dir, "coverage.cobertura.xml", SearchOption.AllDirectories))
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .ToList();

        if (!coverageFiles.Any())
        {
            _logger.LogWarning("📊 Coverage: No coverage.cobertura.xml files found in TestResults directories");
            _logger.LogWarning("📊 Coverage: Make sure test project has coverlet.collector package reference");
            return null;
        }

        var coverageFile = coverageFiles.First();
        _logger.LogDebug("Parsing coverage from {File}", coverageFile.FullName);
        return ParseCoberturaFile(coverageFile.FullName);
    }

    /// <summary>
    /// Parses a Cobertura XML coverage file
    /// </summary>
    private CoverageResult? ParseCoberturaFile(string filePath)
    {
        try
        {
            var doc = XDocument.Load(filePath);
            var coverage = doc.Root;

            if (coverage == null)
                return null;

            var result = new CoverageResult();

            // Parse overall coverage from root attributes
            var lineRateAttr = coverage.Attribute("line-rate");
            var branchRateAttr = coverage.Attribute("branch-rate");
            var linesValidAttr = coverage.Attribute("lines-valid");
            var linesCoveredAttr = coverage.Attribute("lines-covered");
            var branchesValidAttr = coverage.Attribute("branches-valid");
            var branchesCoveredAttr = coverage.Attribute("branches-covered");

            if (lineRateAttr != null && double.TryParse(lineRateAttr.Value, out var lineRate))
                result.LineCoverage = lineRate * 100;

            if (branchRateAttr != null && double.TryParse(branchRateAttr.Value, out var branchRate))
                result.BranchCoverage = branchRate * 100;

            if (linesValidAttr != null && int.TryParse(linesValidAttr.Value, out var totalLines))
                result.TotalLines = totalLines;

            if (linesCoveredAttr != null && int.TryParse(linesCoveredAttr.Value, out var coveredLines))
                result.CoveredLines = coveredLines;

            if (branchesValidAttr != null && int.TryParse(branchesValidAttr.Value, out var totalBranches))
                result.TotalBranches = totalBranches;

            if (branchesCoveredAttr != null && int.TryParse(branchesCoveredAttr.Value, out var coveredBranches))
                result.CoveredBranches = coveredBranches;

            // Parse per-file coverage
            var classes = coverage.Descendants("class");
            foreach (var classElement in classes)
            {
                var filename = classElement.Attribute("filename")?.Value;
                if (string.IsNullOrEmpty(filename))
                    continue;

                var fileLineRate = classElement.Attribute("line-rate");
                var fileCoverage = new FileCoverage
                {
                    FilePath = filename
                };

                if (fileLineRate != null && double.TryParse(fileLineRate.Value, out var fileRate))
                    fileCoverage.LineCoverage = fileRate * 100;

                // Parse individual lines
                var lines = classElement.Descendants("line");
                foreach (var line in lines)
                {
                    fileCoverage.TotalLines++;
                    var hits = line.Attribute("hits")?.Value;
                    if (hits == "0")
                    {
                        var lineNum = line.Attribute("number")?.Value;
                        if (int.TryParse(lineNum, out var num))
                            fileCoverage.UncoveredLines.Add(num);
                    }
                    else
                    {
                        fileCoverage.CoveredLines++;
                    }
                }

                // Only add files with lines (skip empty/interface files)
                if (fileCoverage.TotalLines > 0)
                    result.Files.Add(fileCoverage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing coverage file {File}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Logs a summary of the coverage results
    /// </summary>
    public void LogCoverageSummary(CoverageResult? result)
    {
        if (result == null)
        {
            // Don't log anything here - GetLatestCoverageResult already logged the issue
            return;
        }

        _logger.LogInformation("📊 Coverage: {LineCoverage:F1}% lines ({Covered}/{Total})",
            result.LineCoverage, result.CoveredLines, result.TotalLines);

        if (result.TotalBranches > 0)
        {
            _logger.LogInformation("📊 Branch coverage: {BranchCoverage:F1}% ({Covered}/{Total})",
                result.BranchCoverage, result.CoveredBranches, result.TotalBranches);
        }

        // Show coverage breakdown by file
        var filesWithCoverage = result.Files
            .Where(f => f.TotalLines > 0)
            .OrderByDescending(f => f.LineCoverage)
            .ToList();

        if (filesWithCoverage.Any())
        {
            _logger.LogInformation("📊 Coverage by file:");
            foreach (var file in filesWithCoverage)
            {
                var shortPath = Path.GetFileName(file.FilePath);
                var emoji = file.LineCoverage >= 80 ? "✅" : file.LineCoverage >= 50 ? "⚠️ " : "❌";
                var logLevel = file.LineCoverage >= 80 ? LogLevel.Information : LogLevel.Warning;

                _logger.Log(logLevel, "  {Emoji} {File}: {Coverage:F1}% ({Covered}/{Total} lines)",
                    emoji, shortPath, file.LineCoverage, file.CoveredLines, file.TotalLines);
            }
        }
    }

    /// <summary>
    /// Cleans up old TestResults directories
    /// </summary>
    public void CleanupOldResults(int keepCount = 5)
    {
        var testResultsPath = Path.Combine(_workingDirectory, "TestResults");

        if (!Directory.Exists(testResultsPath))
            return;

        try
        {
            var directories = Directory.GetDirectories(testResultsPath)
                .Select(d => new DirectoryInfo(d))
                .OrderByDescending(d => d.LastWriteTime)
                .Skip(keepCount)
                .ToList();

            foreach (var dir in directories)
            {
                _logger.LogDebug("Cleaning up old test results: {Dir}", dir.Name);
                dir.Delete(recursive: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error cleaning up test results");
        }
    }
}