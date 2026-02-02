# Contesting - Standalone Background Test Runner
# Makefile for common development tasks

.PHONY: help build test clean run install publish deps lint restore coverage report

# Default target
help:
	@echo "Contesting - Available Make Targets:"
	@echo ""
	@echo "Development:"
	@echo "  build      - Build the solution"
	@echo "  test       - Run unit tests"
	@echo "  clean      - Clean build artifacts"
	@echo "  restore    - Restore NuGet packages"
	@echo ""
	@echo "Running:"
	@echo "  run DIR=<path>  - Run Contesting with target directory"
	@echo "  run-self        - Run Contesting monitoring itself (for testing)"
	@echo ""
	@echo "Distribution:"
	@echo "  install         - Install as global tool (requires publish)"
	@echo "  publish         - Create self-contained executable"
	@echo "  publish-all     - Publish for multiple platforms"
	@echo ""
	@echo "Code Quality:"
	@echo "  lint           - Run code analysis and formatting"
	@echo "  coverage       - Run tests with coverage (uses Coverlet)"
	@echo "  report         - Generate HTML coverage report (requires reportgenerator)"
	@echo ""
	@echo "Examples:"
	@echo "  make run DIR=~/dev/MyProject"
	@echo "  make run DIR=/path/to/external/project"

# Build targets
restore:
	@echo "Restoring NuGet packages..."
	dotnet restore

build: restore
	@echo "Building solution..."
	dotnet build

clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf */bin */obj publish TestResults

test: build
	@echo "Running tests..."
	dotnet test

# Running targets
run:
	@if [ -z "$(DIR)" ]; then \
		echo "Error: DIR parameter required"; \
		echo "Usage: make run DIR=<target-directory>"; \
		echo "Example: make run DIR=~/dev/MyProject"; \
		exit 1; \
	fi
	@echo "Starting Contesting to monitor: $(DIR)"
	dotnet run --project Contesting "$(DIR)"

run-self: build
	@echo "Running Contesting to monitor itself (for testing)..."
	dotnet run --project Contesting .

# Publishing targets
publish:
	@echo "Publishing self-contained executable..."
	dotnet publish Contesting -c Release --self-contained -r osx-arm64 -o publish/osx-arm64
	@echo "Published to: publish/osx-arm64/Contesting"
	@echo "Run with: ./publish/osx-arm64/Contesting <target-directory>"

publish-all:
	@echo "Publishing for multiple platforms..."
	dotnet publish Contesting -c Release --self-contained -r osx-arm64 -o publish/osx-arm64
	dotnet publish Contesting -c Release --self-contained -r osx-x64 -o publish/osx-x64
	dotnet publish Contesting -c Release --self-contained -r linux-x64 -o publish/linux-x64
	dotnet publish Contesting -c Release --self-contained -r win-x64 -o publish/win-x64
	@echo "Published to multiple platforms in publish/ directory"

install: publish
	@echo "Installing Contesting globally..."
	sudo cp publish/osx-arm64/Contesting /usr/local/bin/contesting
	@echo "Installed! Run with: contesting <target-directory>"

# Code quality
lint: build
	@echo "Running code analysis..."
	dotnet format --verify-no-changes || echo "Code formatting issues found"

# Coverage using Coverlet (built into dotnet test)
coverage: build
	@echo "Running tests with Coverlet coverage..."
	dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
	@echo "Coverage report generated in TestResults/"
	@echo "Look for coverage.cobertura.xml in TestResults/*/"
	@find TestResults -name "coverage.cobertura.xml" -exec echo "Found: {}" \;

# Generate HTML report using ReportGenerator (optional tool)
report: coverage
	@echo "Generating HTML coverage report..."
	@if command -v reportgenerator > /dev/null 2>&1; then \
		reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/html" -reporttypes:Html; \
		echo "HTML report generated at TestResults/html/index.html"; \
	else \
		echo "ReportGenerator not installed. Install with:"; \
		echo "  dotnet tool install -g dotnet-reportgenerator-globaltool"; \
	fi

# Quick development cycle
dev: clean build test
	@echo "Development cycle complete (clean, build, test)"

# Show current status
status:
	@echo "Contesting Status:"
	@echo "Solution files: $$(find . -name '*.sln' | wc -l | tr -d ' ')"
	@echo "Project files: $$(find . -name '*.csproj' | wc -l | tr -d ' ')"
	@echo "C# files: $$(find . -name '*.cs' | grep -v obj | wc -l | tr -d ' ')"
	@echo "Test files: $$(find . -name '*Test*.cs' | grep -v obj | wc -l | tr -d ' ')"
	@echo ""
	@echo "Tools:"
	@echo "  .NET version: $$(dotnet --version)"
	@echo "  Coverlet: Built-in via coverlet.collector NuGet package"
	@echo "  ReportGenerator: $$(dotnet tool list -g | grep reportgenerator || echo 'Not installed (optional)')"
