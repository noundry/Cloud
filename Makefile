# NDC (Noundry Deploy CLI) Build and Release

APP_NAME = ndc
VERSION ?= 1.0.0
BUILD_DIR = dist
BINARY_DIR = $(BUILD_DIR)/bin

# Go build settings
GOARCH ?= $(shell go env GOARCH)
GOOS ?= $(shell go env GOOS)

# Build targets
.PHONY: build clean test install release build-all

# Default target
all: build

# Clean build artifacts
clean:
	rm -rf $(BUILD_DIR)
	go clean

# Run tests
test:
	go test ./...

# Build for current platform
build:
	mkdir -p $(BINARY_DIR)
	go build -o $(BINARY_DIR)/$(APP_NAME) .

# Build for all platforms
build-all: clean
	mkdir -p $(BINARY_DIR)
	
	# Linux AMD64
	GOOS=linux GOARCH=amd64 go build -o $(BINARY_DIR)/$(APP_NAME)-linux-amd64 .
	
	# Linux ARM64
	GOOS=linux GOARCH=arm64 go build -o $(BINARY_DIR)/$(APP_NAME)-linux-arm64 .
	
	# macOS AMD64
	GOOS=darwin GOARCH=amd64 go build -o $(BINARY_DIR)/$(APP_NAME)-darwin-amd64 .
	
	# macOS ARM64 (Apple Silicon)
	GOOS=darwin GOARCH=arm64 go build -o $(BINARY_DIR)/$(APP_NAME)-darwin-arm64 .
	
	# Windows AMD64
	GOOS=windows GOARCH=amd64 go build -o $(BINARY_DIR)/$(APP_NAME)-windows-amd64.exe .

# Create release packages
release: build-all
	mkdir -p $(BUILD_DIR)/releases
	
	# Create tar.gz for Unix systems
	tar -czf $(BUILD_DIR)/releases/$(APP_NAME)-$(VERSION)-linux-amd64.tar.gz -C $(BINARY_DIR) $(APP_NAME)-linux-amd64
	tar -czf $(BUILD_DIR)/releases/$(APP_NAME)-$(VERSION)-linux-arm64.tar.gz -C $(BINARY_DIR) $(APP_NAME)-linux-arm64
	tar -czf $(BUILD_DIR)/releases/$(APP_NAME)-$(VERSION)-darwin-amd64.tar.gz -C $(BINARY_DIR) $(APP_NAME)-darwin-amd64
	tar -czf $(BUILD_DIR)/releases/$(APP_NAME)-$(VERSION)-darwin-arm64.tar.gz -C $(BINARY_DIR) $(APP_NAME)-darwin-arm64
	
	# Create zip for Windows
	cd $(BINARY_DIR) && zip ../releases/$(APP_NAME)-$(VERSION)-windows-amd64.zip $(APP_NAME)-windows-amd64.exe

# Install locally (requires sudo on Unix)
install: build
ifeq ($(GOOS),windows)
	@echo "Manual installation required on Windows. Copy $(BINARY_DIR)/$(APP_NAME).exe to a directory in your PATH."
else
	sudo cp $(BINARY_DIR)/$(APP_NAME) /usr/local/bin/$(APP_NAME)
	@echo "$(APP_NAME) installed to /usr/local/bin/$(APP_NAME)"
endif

# Development helpers
dev-test:
	go run . create dotnet-webapp-aws --name test-project --output ./test-output
	go run . create dotnet-webapp-gcp --name test-gcp-project --output ./test-output
	go run . create dotnet-webapp-azure --name test-azure-project --output ./test-output

dev-clean:
	rm -rf ./test-output

# Generate module dependencies
mod:
	go mod tidy
	go mod download

# Format code
fmt:
	go fmt ./...

# Lint code (requires golangci-lint)
lint:
	golangci-lint run

.PHONY: dev-test dev-clean mod fmt lint