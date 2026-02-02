# Contesting - Standalone Background Test Runner

## Project Overview
Contesting is a standalone background test runner that monitors ANY C# solution/project directory for file changes and automatically runs tests. It's designed to run as a separate application that watches your main development projects in real-time.

## Key Features
- **File Monitoring**: Watches `.cs` files for changes using FileSystemWatcher
- **Instant Testing**: Automatically runs `dotnet test` when files change
- **External Project Support**: Monitor any C# project directory from anywhere
- **Coverlet Integration**: Built-in code coverage using Coverlet collector
- **Real-time Feedback**: Clear pass/fail indicators for test results
- **Coverage Reporting**: Shows line coverage percentage after each test run
- **Background Operation**: Runs continuously while you develop

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

### Coverage Commands
```bash
# Run tests with coverage collection (using Coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Coverage output is in TestResults/*/coverage.cobertura.xml

# Generate HTML report (requires reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/html" -reporttypes:Html
```

## How It Works
1. **Monitors External Projects**: Contesting runs separately from your main development project
2. **File Watching**: Uses FileSystemWatcher to detect changes to `.cs` files in the target directory
3. **Automatic Testing**: When files change, builds and runs `dotnet test` automatically
4. **Coverage Collection**: Uses Coverlet (via `--collect:"XPlat Code Coverage"`) to collect coverage data
5. **Coverage Reporting**: Parses Cobertura XML and displays coverage summary
6. **Background Operation**: Runs continuously in the background while you develop

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
info: File changed: /path/to/YourProject/SomeFile.cs (Changed)
info: Processing file change for: /path/to/YourProject/SomeFile.cs
info: File changed, rebuilding and running tests...
info: Building solution...
info: Running tests with coverage
info: Tests passed
info: Coverage: 85.5% lines (42/49)
info: Tests passed after file change
```

Or if tests fail (with detailed failure information):
```
warn: Tests failed with exit code 1
warn: Failing tests:
warn:    - Contesting.Tests.CalculatorTests.TestAdd: Assert.Equal() Failure: Expected 9, Actual 8
warn: Tests failed after file change
```

Or if the build fails:
```
info: Building solution...
warn: Build failed with exit code 1
warn: Build failed after file change
```

## Project Structure
```
/
├── Contesting/              # Main console application (takes target directory as argument)
├── Contesting.Core/         # Core library with file watching and coverage logic
└── Contesting.Tests/        # Unit tests for Contesting itself
```

## Dependencies
- **Coverlet** (via coverlet.collector NuGet package - for coverage analysis)
- **Microsoft.Extensions.Hosting** (for background service)
- **Microsoft.Extensions.Logging** (for logging)

## Implementation Details

### Core Components
- **`Program.cs`**: Command line argument parsing and service setup
- **`FileWatcherService.cs`**: FileSystemWatcher implementation with event handling
- **`TestRunner.cs`**: Executes `dotnet test` commands with coverage collection
- **`CoverletService.cs`**: Parses Cobertura XML coverage results
- **`Calculator.cs`**: Sample implementation for testing Contesting itself

### File Monitoring
- Watches all `.cs` files recursively in target directory
- Responds to `Created`, `Changed`, and `Renamed` events
- Uses debouncing (500ms delay) to avoid rapid-fire executions
- Skips bin/ and obj/ directories

### Test Execution
- Runs `dotnet test --no-build --collect:"XPlat Code Coverage"`
- Captures stdout/stderr for proper logging
- Returns pass/fail status with exit code analysis
- Parses test output for failure details

### Coverage Analysis
- Uses Coverlet collector (built into `dotnet test`)
- Parses Cobertura XML format output
- Reports line and branch coverage percentages
- Highlights files with low coverage (<80%)

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

# Distribution
make publish        # Create executable in publish/
make install        # Install globally as 'contesting' command

# Code quality
make lint           # Code formatting and analysis
make coverage       # Run tests with coverage (Coverlet)
make report         # Generate HTML coverage report
```

## Session History & Changes

### Initial Setup (Completed)
- Created basic C# console application structure
- Implemented basic file watching and test execution
- Created sample Calculator class and tests for validation

### Standalone Mode Implementation (Completed)
- Modified `Program.cs` to accept target directory argument
- Updated `FileWatcherService` to work with external directories
- Enhanced `TestRunner` to execute tests in target working directory
- Added project/solution discovery validation

### File Monitoring Enhancements (Completed)
- Enhanced FileSystemWatcher with comprehensive event handling
- Added support for Created, Changed, Renamed events
- Implemented error event handling for watcher failures
- Added detailed logging for troubleshooting

### Development Tooling (Completed)
- Created comprehensive Makefile with common development tasks
- Added multi-platform publishing support (macOS, Linux, Windows)
- Added code quality targets (lint, coverage)

### Enhanced Failure Reporting (Completed)
- Shows specific failing test names and reasons
- Parses dotnet test output for failure details
- Handles multiple test failure formats (MSTest, xUnit, NUnit)
- Displays "Expected: X, Actual: Y" type messages

### Coverlet Migration (Completed)
- **Replaced MiniCover with Coverlet**: Simpler, more modern coverage tooling
- **Removed instrumentation steps**: No more instrument/uninstrument cycle
- **Simplified workflow**: Just build → test with `--collect:"XPlat Code Coverage"`
- **Added CoverletService**: Parses Cobertura XML coverage results
- **Coverage summaries**: Shows coverage percentage after each test run
- **Cleaned up Makefile**: Updated coverage targets for Coverlet
- **Added HTML report generation**: Optional ReportGenerator support

## Future Enhancements (Planned)
- **Smart Test Selection**: Use coverage data to run only affected tests
- **Performance Optimization**: Cache coverage analysis results
- **Configuration File**: Support for `.contesting.json` config files
- **Multiple Project Support**: Handle solutions with multiple test projects
- **IDE Integration**: Plugins for VS Code, Visual Studio, Rider
- **Notification System**: Desktop notifications for test results
