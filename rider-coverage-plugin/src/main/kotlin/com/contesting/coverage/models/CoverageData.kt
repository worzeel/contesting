package com.contesting.coverage.models

data class CoverageResult(
    val timestamp: String,
    val testRunId: String,
    val summary: CoverageSummary,
    val files: List<FileCoverageData>
)

data class CoverageSummary(
    val lineCoverage: Double,
    val branchCoverage: Double,
    val coveredLines: Int,
    val totalLines: Int
)

data class FileCoverageData(
    val path: String,
    val lineCoverage: Double,
    val coveredLines: Int,
    val totalLines: Int,
    val uncoveredLines: List<Int>,
    val lines: List<LineCoverageData>,
    val methods: List<MethodCoverageData>
)

data class LineCoverageData(
    val number: Int,
    val hits: Int,
    val isBranch: Boolean,
    val branchCoverage: BranchCoverageData?
)

data class BranchCoverageData(
    val covered: Int,
    val total: Int,
    val percentage: Double
)

data class MethodCoverageData(
    val name: String,
    val signature: String,
    val lineCoverage: Double,
    val startLine: Int,
    val endLine: Int,
    val complexity: Int
)
