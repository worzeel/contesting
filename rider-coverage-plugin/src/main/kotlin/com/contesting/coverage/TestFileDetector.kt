package com.contesting.coverage

import com.intellij.psi.PsiElement
import com.intellij.psi.PsiFile
import com.intellij.psi.PsiRecursiveElementVisitor

object TestFileDetector {
    private val TEST_ATTRIBUTES = setOf(
        // NUnit
        "Test", "TestFixture", "TestCase", "TestCaseSource", "SetUp", "TearDown",
        "OneTimeSetUp", "OneTimeTearDown", "Theory", "TestFixtureSetUp", "TestFixtureTearDown",

        // xUnit
        "Fact", "Theory", "InlineData", "MemberData", "ClassData",

        // MSTest
        "TestMethod", "TestClass", "TestInitialize", "TestCleanup"
    )

    fun isTestFile(psiFile: PsiFile): Boolean {
        // Quick check: file name ends with Test/Tests
        if (psiFile.name.matches(Regex(".*Tests?\\.cs$"))) {
            return true
        }

        // Deep check: scan for test attributes
        return containsTestAttributes(psiFile)
    }

    private fun containsTestAttributes(psiFile: PsiFile): Boolean {
        // Use PSI tree to search for attributes
        var hasTestAttribute = false

        psiFile.accept(object : PsiRecursiveElementVisitor() {
            override fun visitElement(element: PsiElement) {
                super.visitElement(element)

                // Check if element text contains test attribute patterns
                // This is a simplified check - in a production plugin we'd use proper PSI navigation
                val elementText = element.text
                TEST_ATTRIBUTES.forEach { attr ->
                    if (elementText.contains("[$attr]") || elementText.contains("[$attr(")) {
                        hasTestAttribute = true
                    }
                }
            }
        })

        return hasTestAttribute
    }
}
