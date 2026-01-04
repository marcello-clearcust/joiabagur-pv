#!/bin/bash

# Setup Development HTTPS Certificates for Joiabagur PV Backend
# This script configures development certificates for HTTPS development

set -e

echo "Setting up development HTTPS certificates for Joiabagur PV Backend..."

# Check if dotnet dev-certs is available
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK not found. Please install .NET 10 SDK first."
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "Found .NET SDK version: $DOTNET_VERSION"

# Check if certificates already exist
echo "Checking for existing development certificates..."
dotnet dev-certs https --check

if [ $? -eq 0 ]; then
    echo "Development certificates already exist."
else
    echo "Creating new development certificates..."
    dotnet dev-certs https

    if [ $? -ne 0 ]; then
        echo "Error: Failed to create development certificates."
        exit 1
    fi
fi

# Trust the certificates (requires sudo on Linux/Mac)
echo "Trusting development certificates (may require sudo)..."

if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    echo "On Linux, you may need to import the certificate manually."
    echo "Certificate location: ~/.dotnet/corefx/cryptography/x509stores/my/"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    echo "On macOS, you may need to import the certificate manually."
    echo "Certificate location: ~/.dotnet/corefx/cryptography/x509stores/my/"
else
    echo "Unknown OS. Please check the .NET documentation for certificate trust on your platform."
fi

echo ""
echo "HTTPS setup complete! You can now run the application with:"
echo "dotnet run --launch-profile https"
echo ""
echo "The API will be available at:"
echo "  HTTPS: https://localhost:7169"
echo "  HTTP:  http://localhost:5056"
echo "  Swagger: https://localhost:7169/swagger"