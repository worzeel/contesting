package com.contesting.coverage

import com.intellij.openapi.application.ApplicationManager
import com.intellij.openapi.project.Project
import com.intellij.openapi.vfs.VirtualFile
import java.util.concurrent.Executors
import java.util.concurrent.TimeUnit

object CoverageFileWatcher {
    private val executor = Executors.newSingleThreadScheduledExecutor()
    private var lastModified: Long = 0

    fun start(project: Project, coverageFile: VirtualFile?) {
        if (coverageFile == null) return

        // Store initial modification time
        lastModified = coverageFile.timeStamp

        // Poll the file every second for changes
        executor.scheduleAtFixedRate({
            try {
                // Force VFS refresh to get latest file state
                coverageFile.refresh(false, false)

                val currentModified = coverageFile.timeStamp
                if (currentModified > lastModified) {
                    lastModified = currentModified

                    // File changed! Reload coverage data on UI thread
                    ApplicationManager.getApplication().invokeLater {
                        val dataManager = CoverageDataManager.getInstance(project)
                        dataManager.loadCoverageData(coverageFile)
                    }
                }
            } catch (e: Exception) {
                // Silently ignore errors (file might be temporarily unavailable)
            }
        }, 0, 1, TimeUnit.SECONDS)  // Check every 1 second
    }
}
