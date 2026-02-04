package com.contesting.coverage

import com.intellij.codeInsight.daemon.LineMarkerInfo
import com.intellij.codeInsight.daemon.LineMarkerProvider
import com.intellij.openapi.editor.markup.GutterIconRenderer
import com.intellij.psi.PsiDocumentManager
import com.intellij.psi.PsiElement
import javax.swing.Icon

class CoverageLineMarkerProvider : LineMarkerProvider {
    override fun getLineMarkerInfo(element: PsiElement): LineMarkerInfo<*>? {
        // Only process leaf elements (performance optimization)
        if (element.firstChild != null) return null

        val psiFile = element.containingFile ?: return null
        val project = element.project

        // Skip test files
        if (TestFileDetector.isTestFile(psiFile)) return null

        // Get file path and line number
        val virtualFile = psiFile.virtualFile ?: return null
        val filePath = virtualFile.path
        val document = PsiDocumentManager.getInstance(project).getDocument(psiFile) ?: return null
        val lineNumber = document.getLineNumber(element.textRange.startOffset) + 1  // 1-indexed

        // Get coverage status for this line
        val dataManager = CoverageDataManager.getInstance(project)
        val status = dataManager.getLineStatus(filePath, lineNumber)

        // Return appropriate marker
        return when (status) {
            LineStatus.COVERED -> createMarker(element, CoverageIcons.GREEN, "Covered by tests")
            LineStatus.UNCOVERED -> createMarker(element, CoverageIcons.RED, "Not covered by tests")
            LineStatus.NONE -> null  // No marker
        }
    }

    private fun createMarker(element: PsiElement, icon: Icon, tooltip: String): LineMarkerInfo<PsiElement> {
        return LineMarkerInfo(
            element,
            element.textRange,
            icon,
            { tooltip },
            null,  // No navigation handler
            GutterIconRenderer.Alignment.LEFT
        )
    }
}
