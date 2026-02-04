package com.contesting.coverage

import com.contesting.coverage.models.CoverageResult
import com.contesting.coverage.models.FileCoverageData
import com.google.gson.Gson
import com.intellij.codeInsight.daemon.DaemonCodeAnalyzer
import com.intellij.openapi.components.Service
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile

@Service(Service.Level.PROJECT)
class CoverageDataManager(private val project: Project) {
    private var coverageData: Map<String, FileCoverageData> = emptyMap()

    companion object {
        fun getInstance(project: Project): CoverageDataManager {
            return project.getService(CoverageDataManager::class.java)
        }
    }

    fun loadCoverageData(jsonFile: VirtualFile?) {
        if (jsonFile == null || !jsonFile.exists()) {
            coverageData = emptyMap()
            return
        }

        try {
            // Parse JSON using Gson
            val json = jsonFile.inputStream.bufferedReader().use { it.readText() }
            val result = Gson().fromJson(json, CoverageResult::class.java)

            // Convert to Map<filePath, FileCoverageData> for fast lookup
            coverageData = result.files.associateBy { it.path }

            // Trigger editor refresh
            refreshEditors()
        } catch (e: Exception) {
            // Silently fail if JSON is malformed - just clear coverage data
            coverageData = emptyMap()
        }
    }

    fun getCoverageForFile(filePath: String): FileCoverageData? {
        // Try exact match first
        coverageData[filePath]?.let { return it }

        // Try matching by filename only (for relative paths in JSON)
        val fileName = filePath.substringAfterLast('/')
        return coverageData[fileName] ?:
               // Try finding any entry that ends with the path
               coverageData.entries.find {
                   filePath.endsWith(it.key) || it.key.endsWith(fileName)
               }?.value
    }

    fun getLineStatus(filePath: String, lineNumber: Int): LineStatus {
        val fileData = getCoverageForFile(filePath) ?: return LineStatus.NONE

        // Check if line is in lines array
        val lineData = fileData.lines.find { it.number == lineNumber }
        return when {
            lineData == null -> LineStatus.NONE          // Not a coverable line
            lineData.hits > 0 -> LineStatus.COVERED      // Green
            lineData.hits == 0 -> LineStatus.UNCOVERED   // Red
            else -> LineStatus.NONE                      // Grey/blank
        }
    }

    private fun refreshEditors() {
        // Trigger DaemonCodeAnalyzer to refresh all open editors
        DaemonCodeAnalyzer.getInstance(project).restart()
    }
}

enum class LineStatus {
    COVERED,    // Green dot
    UNCOVERED,  // Red dot
    NONE        // No dot (grey or blank)
}
