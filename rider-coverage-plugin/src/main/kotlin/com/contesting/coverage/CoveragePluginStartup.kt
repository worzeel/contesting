package com.contesting.coverage

import com.intellij.openapi.project.Project
import com.intellij.openapi.startup.ProjectActivity
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.openapi.vfs.VirtualFileManager

class CoveragePluginStartup : ProjectActivity {
    override suspend fun execute(project: Project) {
        // Find TestResults/latest-coverage.json in project
        val coverageFile = findCoverageFile(project)

        // Initialize data manager and load coverage data
        val dataManager = CoverageDataManager.getInstance(project)
        dataManager.loadCoverageData(coverageFile)

        // Start watching for file changes
        CoverageFileWatcher.start(project, coverageFile)
    }

    private fun findCoverageFile(project: Project): VirtualFile? {
        val basePath = project.basePath ?: return null
        val baseDir = VirtualFileManager.getInstance().findFileByUrl("file://$basePath") ?: return null

        // Look for TestResults/latest-coverage.json
        return findFileRecursive(baseDir, "TestResults/latest-coverage.json")
    }

    private fun findFileRecursive(directory: VirtualFile, targetPath: String): VirtualFile? {
        // Check current directory
        val testResultsDir = directory.findChild("TestResults")
        if (testResultsDir != null && testResultsDir.isDirectory) {
            val coverageFile = testResultsDir.findChild("latest-coverage.json")
            if (coverageFile != null && coverageFile.exists()) {
                return coverageFile
            }
        }

        // Search subdirectories (but skip common build directories)
        directory.children.forEach { child ->
            if (child.isDirectory && !shouldSkipDirectory(child.name)) {
                val found = findFileRecursive(child, targetPath)
                if (found != null) return found
            }
        }

        return null
    }

    private fun shouldSkipDirectory(name: String): Boolean {
        return name in setOf("bin", "obj", "node_modules", ".git", ".vs", ".idea", "packages")
    }
}
