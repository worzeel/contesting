.PHONY: help build clean run install

help:
	@echo "Rider Coverage Plugin - Available commands:"
	@echo ""
	@echo "  make build     - Build the plugin (creates .zip in build/distributions/)"
	@echo "  make clean     - Clean build artifacts"
	@echo "  make run       - Run Rider with plugin in test mode"
	@echo "  make install   - Show instructions for installing the plugin"
	@echo ""

build:
	@echo "Building plugin..."
	export JAVA_HOME=/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home && ./gradlew buildPlugin --no-daemon

clean:
	@echo "Cleaning build artifacts..."
	export JAVA_HOME=/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home && ./gradlew clean --no-daemon

run:
	@echo "Running Rider with plugin in test mode..."
	@echo "This will open a Rider sandbox with the plugin installed."
	export JAVA_HOME=/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home && ./gradlew runIde --no-daemon

install:
	@echo ""
	@echo "To install the plugin in Rider:"
	@echo ""
	@echo "1. Build the plugin: make build"
	@echo "2. Open Rider"
	@echo "3. Go to Settings → Plugins"
	@echo "4. Click the ⚙️  icon → Install Plugin from Disk..."
	@echo "5. Select: build/distributions/rider-coverage-plugin-1.0.0.zip"
	@echo "6. Restart Rider"
	@echo ""
	@echo "Plugin location: $(shell pwd)/build/distributions/rider-coverage-plugin-1.0.0.zip"
	@echo ""
