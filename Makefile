# Contesting - Standalone Background Test Runner
# Makefile for common development tasks

.PHONY: help build test clean run install publish deps lint restore

# Default target
help:
	@echo "Contesting - Available Make Targets:"
	@echo ""
	@echo "Development:"
	@echo "  build      - Build the solution"
	@echo "  test       - Run unit tests"
	@echo "  clean      - Clean build artifacts"
	@echo "  restore    - Restore NuGet packages"
	@echo "  deps       - Install dependencies (MiniCover tool)"
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
	@echo "Maintenance:"
	@echo "  lint           - Run code analysis and formatting"
	@echo "  coverage       - Generate coverage report"
	@echo ""
	@echo "Examples:"
	@echo "  make run DIR=~/dev/MyProject"
	@echo "  make run DIR=/path/to/external/project"

# Build targets
restore:
	@echo "🔄 Restoring NuGet packages..."
	dotnet restore

build: restore
	@echo "🔨 Building solution..."
	dotnet build

clean:
	@echo "🧹 Cleaning build artifacts..."
	dotnet clean
	rm -rf */bin */obj publish

test: build
	@echo "🧪 Running tests..."
	dotnet test

# Dependencies
deps:
	@echo "📦 Installing MiniCover global tool..."
	dotnet tool install --global MiniCover || echo "MiniCover already installed"
	@echo "✅ Dependencies installed"

# Running targets
run:
	@if [ -z "$(DIR)" ]; then \
		echo "❌ Error: DIR parameter required"; \
		echo "Usage: make run DIR=<target-directory>"; \
		echo "Example: make run DIR=~/dev/MyProject"; \
		exit 1; \
	fi
	@echo "🚀 Starting Contesting to monitor: $(DIR)"
	dotnet run --project Contesting "$(DIR)"

run-self: build
	@echo "🔍 Running Contesting to monitor itself (for testing)..."
	dotnet run --project Contesting .

# Publishing targets
publish:
	@echo "📦 Publishing self-contained executable..."
	dotnet publish Contesting -c Release --self-contained -r osx-arm64 -o publish/osx-arm64
	@echo "✅ Published to: publish/osx-arm64/Contesting"
	@echo "💡 Run with: ./publish/osx-arm64/Contesting <target-directory>"

publish-all:
	@echo "📦 Publishing for multiple platforms..."
	dotnet publish Contesting -c Release --self-contained -r osx-arm64 -o publish/osx-arm64
	dotnet publish Contesting -c Release --self-contained -r osx-x64 -o publish/osx-x64
	dotnet publish Contesting -c Release --self-contained -r linux-x64 -o publish/linux-x64
	dotnet publish Contesting -c Release --self-contained -r win-x64 -o publish/win-x64
	@echo "✅ Published to multiple platforms in publish/ directory"

install: publish
	@echo "🔧 Installing Contesting globally..."
	sudo cp publish/osx-arm64/Contesting /usr/local/bin/contesting
	@echo "✅ Installed! Run with: contesting <target-directory>"

# Code quality
lint: build
	@echo "🔍 Running code analysis..."
	dotnet format --verify-no-changes || echo "⚠️  Code formatting issues found"
	@echo "✅ Lint complete"

coverage: build deps
	@echo "📊 Generating coverage report..."
	@echo "Setting up MiniCover instrumentation..."
	dotnet minicover instrument --sources "**/*.cs" --tests "**/*Tests.cs" --exclude-sources "**/obj/**/*.cs" --exclude-tests "**/obj/**/*.cs"
	dotnet minicover reset
	dotnet test --no-build
	dotnet minicover uninstrument
	dotnet minicover report --threshold 0
	@echo "✅ Coverage report generated"

# Quick development cycle
dev: clean build test
	@echo "✅ Development cycle complete (clean, build, test)"

# Show current status
status:
	@echo "📋 Contesting Status:"
	@echo "Solution files: $$(find . -name '*.sln' | wc -l | tr -d ' ')"
	@echo "Project files: $$(find . -name '*.csproj' | wc -l | tr -d ' ')"
	@echo "C# files: $$(find . -name '*.cs' | grep -v obj | wc -l | tr -d ' ')"
	@echo "Test files: $$(find . -name '*Test*.cs' | grep -v obj | wc -l | tr -d ' ')"
	@echo ""
	@echo "Tools:"
	@echo "  .NET version: $$(dotnet --version)"
	@echo "  MiniCover: $$(dotnet tool list -g | grep minicover || echo 'Not installed')"