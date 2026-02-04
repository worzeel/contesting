namespace Contesting.Core;

/// <summary>
/// Complete coverage result for JSON output consumption by IDE plugins
/// </summary>
public class CoverageResultDetail
{
    public DateTime Timestamp { get; set; }
    public string TestRunId { get; set; } = "";
    public CoverageSummary Summary { get; set; } = new();
    public List<FileCoverageDetail> Files { get; set; } = new();
}

public class CoverageSummary
{
    public double LineCoverage { get; set; }
    public double BranchCoverage { get; set; }
    public int CoveredLines { get; set; }
    public int TotalLines { get; set; }
    public int CoveredBranches { get; set; }
    public int TotalBranches { get; set; }
}

public class FileCoverageDetail
{
    public string Path { get; set; } = "";
    public double LineCoverage { get; set; }
    public double BranchCoverage { get; set; }
    public int CoveredLines { get; set; }
    public int TotalLines { get; set; }
    public int CoveredBranches { get; set; }
    public int TotalBranches { get; set; }
    public List<int> UncoveredLines { get; set; } = new();
    public List<LineCoverageDetail> Lines { get; set; } = new();
    public List<MethodCoverageDetail> Methods { get; set; } = new();
}

public class LineCoverageDetail
{
    public int Number { get; set; }
    public int Hits { get; set; }
    public bool IsBranch { get; set; }
    public BranchCoverageDetail? BranchCoverage { get; set; }
}

public class BranchCoverageDetail
{
    public int Covered { get; set; }
    public int Total { get; set; }
    public double Percentage { get; set; }
}

public class MethodCoverageDetail
{
    public string Name { get; set; } = "";
    public string Signature { get; set; } = "";
    public double LineCoverage { get; set; }
    public int StartLine { get; set; }
    public int EndLine { get; set; }
    public int Complexity { get; set; }
}
