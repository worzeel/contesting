# Contesting - Standalone Background Test Runner

## Project Overview
Contesting is a standalone background test runner that monitors ANY C# solution/project directory for file changes and automatically runs tests. It's designed to run as a separate application that watches your main development projects in real-time.

## ✨ Key Features
- 🔍 **File Monitoring**: Watches `.cs` files for changes using FileSystemWatcher
- ⚡ **Instant Testing**: Automatically runs `dotnet test` when files change
- 🎯 **External Project Support**: Monitor any C# project directory from anywhere
- 📊 **MiniCover Integration**: Ready for coverage-based test selection (future enhancement)
- 💬 **Real-time Feedback**: Clear ✅/❌ indicators for test results
- 🚀 **Background Operation**: Runs continuously while you develop

## Recent Updates (Session Progress)
- ✅ **Standalone Mode**: Now accepts target directory as command line argument
- ✅ **External Project Support**: Can monitor any C# solution outside of Contesting itself  
- ✅ **Enhanced File Watching**: Improved FileSystemWatcher with better event handling
- ✅ **Real-time Test Execution**: Automatic test runs on file changes with visual feedback
- ✅ **Comprehensive Logging**: Better visibility into file changes and test results

## Usage

### Basic Usage
```bash
# Run Contesting to monitor a specific C# project directory
dotnet run --project /path/to/Contesting /path/to/your/csharp/project

# Example: Monitor your main app while Contesting runs in background
dotnet run --project ~/tools/contesting ~/dev/MyAwesomeApp

# Quick help
dotnet run --project Contesting
# Shows: Usage: Contesting <target-directory>
```

### Build Contesting
```bash
dotnet restore
dotnet build
```

### Publish for Easy Use
```bash
# Create a self-contained executable
dotnet publish -c Release --self-contained -r osx-arm64 -o ./publish

# Then you can run it from anywhere
./publish/Contesting /path/to/your/project
```

### MiniCover Commands (used internally)
```bash
# These are executed automatically by Contesting, but you can run them manually:
dotnet minicover instrument --sources "**/*.cs" --tests "**/*Tests.cs" --exclude-sources "**/obj/**/*.cs" --exclude-tests "**/obj/**/*.cs"
dotnet minicover reset
dotnet test --no-build
dotnet minicover uninstrument
dotnet minicover report --threshold 90
```

## How It Works
1. **Monitors External Projects**: Contesting runs separately from your main development project
2. **File Watching**: Uses FileSystemWatcher to detect changes to `.cs` files in the target directory
3. **Automatic Testing**: When files change, runs `dotnet test` automatically
4. **Coverage Analysis**: Uses MiniCover to analyze which tests cover which code (future enhancement)
5. **Background Operation**: Runs continuously in the background while you develop

## Typical Workflow
```bash
# Terminal 1: Start Contesting to monitor your project
cd /path/to/contesting
dotnet run /path/to/your/main/project

# Terminal 2: Work on your main project as usual
cd /path/to/your/main/project
# Edit files, and Contesting will automatically run tests when you save!
```

### What You'll See
When you save a `.cs` file, Contesting will show:
```
info: File changed: /path/to/YourProject/SomeFile.cs (Created)
info: Processing file change for: /path/to/YourProject/SomeFile.cs  
info: File changed, rebuilding and running tests...
info: Removing instrumentation
info: Building solution with changes...
info: Instrumenting assemblies for coverage analysis
info: Resetting coverage hits
info: Running tests
info: Tests passed
info: ✅ Tests passed after file change
```

Or if tests fail (with detailed failure information):
```
warn: Tests failed with exit code 1
warn: ❌ Failing tests:
warn:    • Contesting.Tests.CalculatorTests.TestWithSpecificFailureMessage: Assert.Equal() Failure: Values differ Expected: 9 Actual: 8
warn: ❌ Tests failed after file change
```

Or if the build fails:
```
info: Building solution with changes...
warn: Build failed with exit code 1
warn: ❌ Build failed after file change
```

## Project Structure
```
/
├── Contesting/              # Main console application (takes target directory as argument)
├── Contesting.Core/         # Core library with file watching and coverage logic
└── Contesting.Tests/        # Unit tests for Contesting itself
```

## Dependencies
- MiniCover (for coverage analysis)
- Microsoft.Extensions.Hosting (for background service)
- Microsoft.Extensions.Logging (for logging)

## Implementation Details

### Core Components
- **`Program.cs`**: Command line argument parsing and service setup
- **`FileWatcherService.cs`**: FileSystemWatcher implementation with event handling
- **`TestRunner.cs`**: Executes `dotnet test` commands in target directory
- **`MiniCoverService.cs`**: Wraps MiniCover tool for coverage analysis
- **`Calculator.cs`**: Sample implementation for testing Contesting itself

### File Monitoring
- Watches all `.cs` files recursively in target directory
- Responds to `Created`, `Changed`, and `Renamed` events
- Uses debouncing (500ms delay) to avoid rapid-fire executions
- Handles editor temp file patterns (e.g., `.tmp.12345.timestamp`)

### Test Execution
- Runs `dotnet test --no-build` for speed
- Captures stdout/stderr for proper logging
- Returns pass/fail status with exit code analysis
- Future: Will support filtered test execution based on coverage

## Quick Commands (Makefile)

```bash
# Show all available commands
make help

# Development workflow
make build          # Build the solution
make test           # Run unit tests  
make dev            # Clean, build, and test in one command

# Run Contesting
make run DIR=~/dev/MyProject    # Monitor external project
make run-self                   # Monitor Contesting itself (testing)

# Install dependencies
make deps           # Install MiniCover global tool

# Distribution
make publish        # Create executable in publish/
make install        # Install globally as 'contesting' command

# Code quality
make lint           # Code formatting and analysis
make coverage       # Generate coverage reports
```

## Session History & Changes
This section tracks all changes made during the current development session:

### Initial Setup (Completed)
- ✅ Created basic C# console application structure
- ✅ Added MiniCover global tool integration  
- ✅ Implemented basic file watching and test execution
- ✅ Created sample Calculator class and tests for validation

### Standalone Mode Implementation (Completed)
- ✅ Modified `Program.cs` to accept target directory argument
- ✅ Updated `FileWatcherService` to work with external directories
- ✅ Enhanced `TestRunner` to execute tests in target working directory
- ✅ Added project/solution discovery validation
- ✅ Improved error handling and logging throughout

### File Monitoring Enhancements (Completed)  
- ✅ Enhanced FileSystemWatcher with comprehensive event handling
- ✅ Added support for Created, Changed, Renamed events
- ✅ Implemented error event handling for watcher failures
- ✅ Added detailed logging for troubleshooting file detection issues
- ✅ Verified real-time test execution with ✅/❌ feedback

### Testing & Validation (Completed)
- ✅ Successfully tested standalone mode monitoring external directories
- ✅ Verified file change detection works correctly
- ✅ Confirmed automatic test execution on file modifications
- ✅ Validated real-time feedback with pass/fail indicators

### Development Tooling (Completed)
- ✅ Created comprehensive Makefile with common development tasks
- ✅ Added `make help` with detailed usage instructions
- ✅ Included build, test, run, publish, and maintenance targets
- ✅ Added multi-platform publishing support (macOS, Linux, Windows)
- ✅ Integrated MiniCover dependency management
- ✅ Added code quality targets (lint, coverage)

### Critical Bug Fix - Test Refresh Issue (Completed)
- ✅ **Fixed test execution on file changes**: Previously, tests would run against old instrumented code
- ✅ **Implemented full rebuild cycle**: Now uninstruments → builds → re-instruments → tests
- ✅ **Added build command integration**: FileWatcherService now rebuilds on changes
- ✅ **Verified fix with failing/passing test cycle**: Confirmed tests now reflect actual code changes
- ✅ **Enhanced error handling**: Better build failure detection and reporting

### Enhanced Failure Reporting (Completed)
- ✅ **Detailed failure information**: Shows specific failing test names and reasons
- ✅ **Parse dotnet test output**: Extracts failure details from test runner output
- ✅ **Smart failure detection**: Handles multiple test failure formats (MSTest, xUnit, NUnit)
- ✅ **Cleaner output for passing tests**: Only shows failure details when needed
- ✅ **Real assertion messages**: Displays "Expected: X, Actual: Y" type messages

### Project Maintenance (Completed)
- ✅ **Comprehensive .gitignore**: Added complete .NET project .gitignore file
- ✅ **Build artifacts exclusion**: Excludes bin/, obj/, publish/, coverage files
- ✅ **IDE support**: Covers Visual Studio, VS Code, Rider, and other common IDEs
- ✅ **MiniCover artifacts**: Specifically excludes *.uninstrumented.dll/pdb files
- ✅ **Coverage files exclusion**: Excludes coverage-hits/, *.hits, coverage.json/xml
- ✅ **Contesting-specific ignores**: Excludes test files and temporary artifacts
- ✅ **Verified .gitignore functionality**: Tested that MiniCover files are properly ignored

## Future Enhancements (Planned)
- 🔄 **Smart Test Selection**: Use MiniCover to run only tests that cover changed code
- 🔄 **Performance Optimization**: Cache coverage analysis results  
- 🔄 **Configuration File**: Support for `.contesting.json` config files
- 🔄 **Multiple Project Support**: Handle solutions with multiple test projects
- 🔄 **IDE Integration**: Plugins for VS Code, Visual Studio, Rider
- 🔄 **Notification System**: Desktop notifications for test results