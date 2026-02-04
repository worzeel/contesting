# Rider Coverage Plugin - Development Guide

## Overview

This is an IntelliJ/Rider plugin that displays code coverage from Contesting in the editor gutter as green/red dots.

## Architecture

### Core Components

1. **CoveragePluginStartup** - Runs on plugin load
   - Finds `TestResults/latest-coverage.json` in project
   - Initializes CoverageDataManager
   - Starts file watcher

2. **CoverageDataManager** - Singleton service
   - Parses JSON using Gson
   - Caches coverage data in memory (Map<filePath, FileCoverageData>)
   - Provides O(1) lookup for line coverage status
   - Triggers editor refresh when data changes

3. **CoverageFileWatcher** - File system watcher
   - Watches JSON file for changes via VirtualFileManager
   - Triggers reload when Contesting updates coverage
   - Runs on UI thread

4. **TestFileDetector** - Test file detection
   - Fast path: filename check (`*Test.cs`, `*Tests.cs`)
   - Slow path: PSI traversal for test attributes
   - Supports NUnit, xUnit, MSTest

5. **CoverageLineMarkerProvider** - Gutter icon provider
   - Called for each PSI element in visible editor
   - Performance optimized (only leaf elements)
   - Shows green (covered) or red (uncovered) dots
   - Skips test files

### Data Flow

```
1. Plugin Startup
   └─> Find TestResults/latest-coverage.json
       └─> Load into CoverageDataManager
           └─> Start CoverageFileWatcher

2. User Opens File
   └─> CoverageLineMarkerProvider called per line
       └─> Check if test file → skip if yes
           └─> Query CoverageDataManager.getLineStatus()
               └─> Return green/red/none marker

3. Coverage JSON Changes
   └─> CoverageFileWatcher detects change
       └─> CoverageDataManager reloads
           └─> DaemonCodeAnalyzer.restart() triggers refresh
               └─> Gutter icons update
```

## Building

### Prerequisites

- Java 17 (installed via `brew install openjdk@17`)
- Gradle 8.5 (via wrapper)

### Build Commands

```bash
# View all commands
make help

# Build plugin
make build
# Output: build/distributions/rider-coverage-plugin-1.0.0.zip

# Clean
make clean

# Run in test Rider instance
make run
```

### Manual Gradle Commands

All require `JAVA_HOME` to be set:

```bash
export JAVA_HOME=/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home

# Build
./gradlew buildPlugin --no-daemon

# Run test instance
./gradlew runIde --no-daemon

# Clean
./gradlew clean --no-daemon
```

## Installation

1. Build the plugin (`make build`)
2. Open Rider
3. Settings → Plugins → ⚙️ → Install Plugin from Disk...
4. Select `build/distributions/rider-coverage-plugin-1.0.0.zip`
5. Restart Rider

## Testing Locally

### With Contesting Running

1. Open Contesting project in terminal:
   ```bash
   cd ~/sandbox/contesting
   dotnet run --project Contesting ~/path/to/your/csharp/project
   ```

2. Run plugin in test Rider:
   ```bash
   cd rider-coverage-plugin
   make run
   ```

3. In test Rider instance:
   - Open your C# project
   - Verify `TestResults/latest-coverage.json` exists
   - Open a C# file (not a test file)
   - Check for green/red dots in gutter

### Verification Checklist

- [ ] Green dots on covered lines
- [ ] Red dots on uncovered lines
- [ ] No dots on test files (with [Test] or [Fact] attributes)
- [ ] No dots on blank lines/comments
- [ ] Dots update when saving file (Contesting reruns tests)
- [ ] Tooltips show "Covered" or "Not covered"

## Plugin.xml Configuration

The plugin descriptor uses the modern format:

```xml
<extensions defaultExtensionNs="com.intellij">
  <!-- Service registration -->
  <projectService
    serviceInterface="com.contesting.coverage.CoverageDataManager"
    serviceImplementation="com.contesting.coverage.CoverageDataManager"/>

  <!-- Startup activity -->
  <postStartupActivity
    implementation="com.contesting.coverage.CoveragePluginStartup"/>

  <!-- Line marker provider -->
  <codeInsight.lineMarkerProvider
    language="C#"
    implementationClass="com.contesting.coverage.CoverageLineMarkerProvider"/>
</extensions>
```

**Note**: Old format `<projectService serviceImplementation="..."/>` causes errors in newer IntelliJ Platform versions.

## Common Issues

### Build fails with "No matching toolchains found"

**Cause**: Gradle can't find Java 17

**Fix**: Set JAVA_HOME before running Gradle:
```bash
export JAVA_HOME=/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home
./gradlew build --no-daemon
```

Or use the Makefile which handles this automatically:
```bash
make build
```

### Plugin fails to load: "Unknown element: projectService"

**Cause**: Using old plugin.xml format

**Fix**: Use modern format with service inside `<extensions>`:
```xml
<extensions defaultExtensionNs="com.intellij">
  <projectService
    serviceInterface="..."
    serviceImplementation="..."/>
</extensions>
```

### No coverage dots appear

**Checklist**:
1. Check `TestResults/latest-coverage.json` exists in project
2. Verify JSON is valid and contains coverage data
3. Check file path in JSON matches opened file (absolute paths)
4. Ensure opened file is not a test file
5. Check Rider logs: Help → Show Log in Finder

## Code Structure

```
src/main/kotlin/com/contesting/coverage/
├── CoveragePluginStartup.kt       # Plugin initialization
├── CoverageDataManager.kt         # Coverage data cache
├── CoverageFileWatcher.kt         # JSON file watcher
├── CoverageLineMarkerProvider.kt  # Gutter icon provider
├── TestFileDetector.kt            # Test file detection
├── CoverageIcons.kt               # Icon loader
└── models/
    └── CoverageData.kt            # JSON data models

src/main/resources/
├── META-INF/
│   └── plugin.xml                 # Plugin descriptor
└── icons/
    ├── coverageGreen.svg          # Covered line icon
    ├── coverageRed.svg            # Uncovered line icon
    └── coverageGrey.svg           # (unused) Not applicable icon
```

## Future Improvements

- [ ] Settings UI for custom JSON path
- [ ] Click actions on gutter icons (jump to test)
- [ ] Coverage percentage in status bar
- [ ] Branch coverage indicators (yellow for partial)
- [ ] Method coverage in structure view
- [ ] Delta coverage (lines that lost coverage)
- [ ] Support for remote coverage files
- [ ] Publish to JetBrains Marketplace

## Resources

- [IntelliJ Platform SDK Docs](https://plugins.jetbrains.com/docs/intellij/)
- [LineMarkerProvider Reference](https://plugins.jetbrains.com/docs/intellij/line-marker-provider.html)
- [Rider Plugin Development](https://www.jetbrains.com/help/rider/Plugins.html)
- [Gradle IntelliJ Plugin](https://github.com/JetBrains/gradle-intellij-plugin)
