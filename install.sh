#!/bin/bash

# NDC (Noundry Deploy CLI) Installation Script
# Usage: curl -fsSL https://raw.githubusercontent.com/plsft/noundry-cloud-cli/main/install.sh | sh

set -e

# Configuration
BINARY_NAME="ndc"
GITHUB_REPO="plsft/noundry-cloud-cli"
INSTALL_DIR="/usr/local/bin"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Print with color
print_color() {
    printf "${1}${2}${NC}\n"
}

print_info() {
    print_color $BLUE "$1"
}

print_success() {
    print_color $GREEN "$1"
}

print_warning() {
    print_color $YELLOW "$1"
}

print_error() {
    print_color $RED "$1"
}

# Detect OS and architecture
detect_platform() {
    OS=$(uname -s | tr '[:upper:]' '[:lower:]')
    ARCH=$(uname -m)
    
    case $OS in
        linux)
            OS="linux"
            ;;
        darwin)
            OS="darwin"
            ;;
        *)
            print_error "Unsupported operating system: $OS"
            exit 1
            ;;
    esac
    
    case $ARCH in
        x86_64|amd64)
            ARCH="amd64"
            ;;
        aarch64|arm64)
            ARCH="arm64"
            ;;
        *)
            print_error "Unsupported architecture: $ARCH"
            exit 1
            ;;
    esac
    
    PLATFORM="${OS}-${ARCH}"
    print_info "Detected platform: $PLATFORM"
}

# Get the latest release version
get_latest_version() {
    print_info "Fetching latest release information..."
    
    LATEST_VERSION=$(curl -s https://api.github.com/repos/$GITHUB_REPO/releases/latest | grep '"tag_name":' | sed -E 's/.*"([^"]+)".*/\1/' | sed 's/^v//')
    
    if [ -z "$LATEST_VERSION" ]; then
        print_error "Failed to fetch latest version"
        exit 1
    fi
    
    print_info "Latest version: $LATEST_VERSION"
}

# Download and install binary
install_binary() {
    DOWNLOAD_URL="https://github.com/$GITHUB_REPO/releases/download/v$LATEST_VERSION/${BINARY_NAME}-${LATEST_VERSION}-${PLATFORM}.tar.gz"
    TEMP_DIR=$(mktemp -d)
    
    print_info "Downloading $BINARY_NAME v$LATEST_VERSION..."
    print_info "URL: $DOWNLOAD_URL"
    
    if ! curl -fsSL "$DOWNLOAD_URL" -o "$TEMP_DIR/${BINARY_NAME}.tar.gz"; then
        print_error "Failed to download $BINARY_NAME"
        print_error "URL: $DOWNLOAD_URL"
        exit 1
    fi
    
    print_info "Extracting archive..."
    cd "$TEMP_DIR"
    tar -xzf "${BINARY_NAME}.tar.gz"
    
    # Find the binary (it might have a platform suffix)
    BINARY_FILE=$(find . -name "${BINARY_NAME}*" -type f -executable | head -n1)
    
    if [ -z "$BINARY_FILE" ]; then
        print_error "Binary not found in archive"
        exit 1
    fi
    
    print_info "Installing to $INSTALL_DIR..."
    
    # Check if we can write to install directory
    if [ -w "$INSTALL_DIR" ]; then
        cp "$BINARY_FILE" "$INSTALL_DIR/$BINARY_NAME"
    else
        print_warning "Need sudo permissions to install to $INSTALL_DIR"
        sudo cp "$BINARY_FILE" "$INSTALL_DIR/$BINARY_NAME"
    fi
    
    chmod +x "$INSTALL_DIR/$BINARY_NAME"
    
    # Cleanup
    rm -rf "$TEMP_DIR"
    
    print_success "✅ $BINARY_NAME v$LATEST_VERSION installed successfully!"
}

# Verify installation
verify_installation() {
    print_info "Verifying installation..."
    
    if command -v $BINARY_NAME >/dev/null 2>&1; then
        INSTALLED_VERSION=$($BINARY_NAME --version | grep -o '[0-9]\+\.[0-9]\+\.[0-9]\+' | head -n1)
        print_success "✅ $BINARY_NAME v$INSTALLED_VERSION is ready to use!"
        
        print_info "Try it out:"
        print_info "  $BINARY_NAME list"
        print_info "  $BINARY_NAME create dotnet-webapp-aws --name my-api"
    else
        print_error "Installation verification failed. $BINARY_NAME not found in PATH."
        print_warning "You may need to reload your shell or add $INSTALL_DIR to your PATH."
        exit 1
    fi
}

# Main installation process
main() {
    print_info "🚀 Installing NDC (Noundry Deploy CLI)..."
    
    detect_platform
    get_latest_version
    install_binary
    verify_installation
    
    print_success "🎉 Installation complete!"
}

# Run main function
main "$@"