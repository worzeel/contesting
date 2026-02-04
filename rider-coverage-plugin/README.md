# Contesting Coverage - Rider Plugin

IntelliJ/Rider plugin that displays code coverage from Contesting test runner in the editor gutter.

## Features

- **Green/Red Gutter Dots**: Shows covered (green) and uncovered (red) lines
- **Auto-Detection**: Finds `TestResults/latest-coverage.json` automatically
- **Test File Exclusion**: Skips test files (detected via [Test], [Fact], etc. attributes)
- **Live Updates**: Updates coverage when JSON file changes
- **Zero Configuration**: Works out of the box

## Installation

### Prerequisites

You need Java 17 installed. On macOS with Homebrew:

```bash
brew install openjdk@17
```

### From Source

```bash
# Build plugin
make build

# Or manually:
export JAVA_HOME=/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home
./gradlew buildPlugin --no-daemon

# Output: build/distributions/rider-coverage-plugin-1.0.0.zip
```

### Install in Rider

1. Open Rider
2. Go to **Settings → Plugins**
3. Click the **⚙️  icon** → **Install Plugin from Disk...**
4. Select `build/distributions/rider-coverage-plugin-1.0.0.zip`
5. Restart Rider

```bash
# View installation instructions
make install
```

## Usage

1. Run Contesting to monitor your C# project (generates `TestResults/latest-coverage.json`)
2. Open your project in Rider with this plugin installed
3. Coverage dots will appear in the gutter automatically
4. Edit code and save - dots update when Contesting re-runs tests

## Development

```bash
# View available commands
make help

# Build plugin
make build

# Run plugin in test Rider instance
make run

# Clean build artifacts
make clean
```

**Note**: All Gradle commands require `JAVA_HOME` to be set to Java 17. The Makefile handles this automatically.

## How It Works

1. Plugin starts and searches for `TestResults/latest-coverage.json`
2. Parses JSON coverage data using Gson
3. Watches JSON file for changes (via VirtualFileManager)
4. Provides `LineMarkerProvider` that shows gutter icons
5. Skips test files (detected by filename or test attributes)

## Coverage Indicators

- 🟢 **Green dot**: Line is covered by tests
- 🔴 **Red dot**: Line is not covered by tests
- ⚫ **No dot**: Line is not coverable (blank line, comment, etc.) or is in a test file

## Requirements

- Rider 2024.2 or later
- Contesting test runner generating `TestResults/latest-coverage.json`

## Architecture

```
CoveragePluginStartup         # Finds JSON file on startup
    ↓
CoverageDataManager           # Parses JSON, caches coverage data
    ↓
CoverageFileWatcher           # Watches for JSON file changes
    ↓
CoverageLineMarkerProvider    # Shows gutter icons in editor
    ↓
TestFileDetector              # Identifies test files to skip
```

## Future Enhancements

- Settings UI for custom JSON path
- Click actions on gutter icons (jump to test, show details)
- Coverage percentage in status bar
- Branch coverage indicators (yellow for partial)
- Historical coverage tracking
