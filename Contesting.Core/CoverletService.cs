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
        var (file, testRunId) = FindLatestCoverageFile();
        if (file == null) return null;

        _logger.LogDebug("Parsing coverage from {File}", file.FullName);
        return ParseCoberturaFile(file.FullName);
    }

    /// <summary>
    /// Finds and parses the most recent coverage file with detailed information for JSON output
    /// </summary>
    public CoverageResultDetail? GetLatestCoverageResultDetail()
    {
        var (file, testRunId) = FindLatestCoverageFile();
        if (file == null) return null;

        _logger.LogDebug("Parsing detailed coverage from {File}", file.FullName);
        return ParseCoberturaFileDetailed(file.FullName, testRunId);
    }

    private (FileInfo? file, string testRunId) FindLatestCoverageFile()
    {
        // Search for TestResults directories recursively (they're often in test project dirs)
        var testResultsDirs = Directory
            .GetDirectories(_workingDirectory, "TestResults", SearchOption.AllDirectories)
            .Where(d => !d.Contains("/bin/") && !d.Contains("/obj/"))
            .ToList();

        if (!testResultsDirs.Any())
        {
            _logger.LogWarning("📊 Coverage: No TestResults directories found under {Path}", _workingDirectory);
            return (null, "");
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
            return (null, "");
        }

        var coverageFile = coverageFiles.First();

        // Extract test run ID from directory structure (GUID folder name)
        var testRunId = "";
        var parentDir = coverageFile.Directory?.Name;
        if (parentDir != null && Guid.TryParse(parentDir, out _))
        {
            testRunId = parentDir;
        }

        return (coverageFile, testRunId);
    }

    /// <summary>
    /// Parses a Cobertura XML coverage file with detailed information
    /// </summary>
    private CoverageResultDetail? ParseCoberturaFileDetailed(string filePath, string testRunId)
    {
        try
        {
            var doc = XDocument.Load(filePath);
            var coverage = doc.Root;

            if (coverage == null)
                return null;

            var result = new CoverageResultDetail
            {
                Timestamp = DateTime.UtcNow,
                TestRunId = testRunId
            };

            // Parse overall coverage from root attributes
            result.Summary.LineCoverage = ParseRate(coverage.Attribute("line-rate")) * 100;
            result.Summary.BranchCoverage = ParseRate(coverage.Attribute("branch-rate")) * 100;
            result.Summary.TotalLines = ParseInt(coverage.Attribute("lines-valid"));
            result.Summary.CoveredLines = ParseInt(coverage.Attribute("lines-covered"));
            result.Summary.TotalBranches = ParseInt(coverage.Attribute("branches-valid"));
            result.Summary.CoveredBranches = ParseInt(coverage.Attribute("branches-covered"));

            // Parse per-file coverage
            var classes = coverage.Descendants("class");
            foreach (var classElement in classes)
            {
                var filename = classElement.Attribute("filename")?.Value;
                if (string.IsNullOrEmpty(filename))
                    continue;

                var fileCoverage = new FileCoverageDetail
                {
                    Path = filename,
                    LineCoverage = ParseRate(classElement.Attribute("line-rate")) * 100,
                    BranchCoverage = ParseRate(classElement.Attribute("branch-rate")) * 100
                };

                // Parse methods
                var methods = classElement.Element("methods")?.Elements("method");
                if (methods != null)
                {
                    foreach (var method in methods)
                    {
                        var methodDetail = new MethodCoverageDetail
                        {
                            Name = method.Attribute("name")?.Value ?? "",
                            Signature = method.Attribute("signature")?.Value ?? "",
                            LineCoverage = ParseRate(method.Attribute("line-rate")) * 100,
                            Complexity = ParseInt(method.Attribute("complexity"))
                        };

                        // Get method line range
                        var methodLines = method.Descendants("line");
                        if (methodLines.Any())
                        {
                            var lineNumbers = methodLines
                                .Select(l => ParseInt(l.Attribute("number")))
                                .Where(n => n > 0)
                                .ToList();

                            if (lineNumbers.Any())
                            {
                                methodDetail.StartLine = lineNumbers.Min();
                                methodDetail.EndLine = lineNumbers.Max();
                            }
                        }

                        fileCoverage.Methods.Add(methodDetail);
                    }
                }

                // Parse individual lines
                var lines = classElement.Element("lines")?.Elements("line");
                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        var lineNumber = ParseInt(line.Attribute("number"));
                        if (lineNumber == 0) continue;

                        var hits = ParseInt(line.Attribute("hits"));
                        fileCoverage.TotalLines++;

                        if (hits > 0)
                        {
                            fileCoverage.CoveredLines++;
                        }
                        else
                        {
                            fileCoverage.UncoveredLines.Add(lineNumber);
                        }

                        var lineDetail = new LineCoverageDetail
                        {
                            Number = lineNumber,
                            Hits = hits
                        };

                        // Check for branch coverage
                        var conditions = line.Attribute("condition-coverage")?.Value;
                        if (!string.IsNullOrEmpty(conditions))
                        {
                            lineDetail.IsBranch = true;
                            lineDetail.BranchCoverage = ParseBranchCoverage(conditions);

                            if (lineDetail.BranchCoverage != null)
                            {
                                fileCoverage.TotalBranches += lineDetail.BranchCoverage.Total;
                                fileCoverage.CoveredBranches += lineDetail.BranchCoverage.Covered;
                            }
                        }

                        fileCoverage.Lines.Add(lineDetail);
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
    /// Helper to parse rate attributes (0.0-1.0)
    /// </summary>
    private static double ParseRate(XAttribute? attr)
    {
        if (attr == null) return 0;
        return double.TryParse(attr.Value, out var rate) ? rate : 0;
    }

    /// <summary>
    /// Helper to parse integer attributes
    /// </summary>
    private static int ParseInt(XAttribute? attr)
    {
        if (attr == null) return 0;
        return int.TryParse(attr.Value, out var value) ? value : 0;
    }

    /// <summary>
    /// Helper to parse branch coverage from condition-coverage attribute
    /// Format: "50% (1/2)" or "100% (2/2)"
    /// </summary>
    private static BranchCoverageDetail? ParseBranchCoverage(string conditions)
    {
        try
        {
            // Format is typically: "50% (1/2)"
            var parts = conditions.Split(' ');
            if (parts.Length < 2) return null;

            var fractionPart = parts[1].Trim('(', ')');
            var fractionParts = fractionPart.Split('/');
            if (fractionParts.Length != 2) return null;

            if (!int.TryParse(fractionParts[0], out var covered)) return null;
            if (!int.TryParse(fractionParts[1], out var total)) return null;

            return new BranchCoverageDetail
            {
                Covered = covered,
                Total = total,
                Percentage = total > 0 ? (covered * 100.0 / total) : 0
            };
        }
        catch
        {
            return null;
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