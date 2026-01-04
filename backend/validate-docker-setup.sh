#!/bin/bash

# Validate Docker Setup for Joiabagur PV Backend
# This script validates that the Docker environment is properly configured

set -e

echo "Validating Docker setup for Joiabagur PV Backend..."

# Check if Docker is installed
echo "Checking Docker installation..."
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed or not in PATH."
    echo "Please install Docker from: https://docs.docker.com/get-docker/"
    exit 1
fi

DOCKER_VERSION=$(docker --version)
echo "✅ Docker found: $DOCKER_VERSION"

# Check if Docker daemon is running
echo "Checking Docker daemon..."
if ! docker info &> /dev/null; then
    echo "❌ Docker daemon is not running."
    echo "Please start Docker and try again."
    exit 1
fi

echo "✅ Docker daemon is running"

# Check if Docker Compose is available
echo "Checking Docker Compose..."
if command -v docker-compose &> /dev/null; then
    COMPOSE_VERSION=$(docker-compose --version)
    echo "✅ Docker Compose found: $COMPOSE_VERSION"
elif docker compose version &> /dev/null; then
    COMPOSE_VERSION=$(docker compose version)
    echo "✅ Docker Compose (plugin) found: $COMPOSE_VERSION"
else
    echo "❌ Docker Compose is not available."
    echo "Please install Docker Compose."
    exit 1
fi

# Validate docker-compose.yml file
echo "Validating docker-compose.yml..."
if [ ! -f "docker-compose.yml" ]; then
    echo "❌ docker-compose.yml not found in current directory."
    exit 1
fi

# Test docker-compose configuration
if command -v docker-compose &> /dev/null; then
    docker-compose config > /dev/null
else
    docker compose config > /dev/null
fi

if [ $? -ne 0 ]; then
    echo "❌ docker-compose.yml has configuration errors."
    if command -v docker-compose &> /dev/null; then
        docker-compose config
    else
        docker compose config
    fi
    exit 1
fi

echo "✅ docker-compose.yml is valid"

# Check if required ports are available
echo "Checking port availability..."

PORTS=(5432 8080)  # PostgreSQL and pgAdmin ports
PORTS_IN_USE=()

for port in "${PORTS[@]}"; do
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        PORTS_IN_USE+=($port)
    fi
done

if [ ${#PORTS_IN_USE[@]} -gt 0 ]; then
    echo "⚠️  Warning: Ports ${PORTS_IN_USE[*]} are already in use."
    echo "   This might cause conflicts when starting the containers."
else
    echo "✅ Required ports are available"
fi

echo ""
echo "🎉 Docker setup validation complete!"
echo ""
echo "You can now start the development environment with:"
if command -v docker-compose &> /dev/null; then
    echo "docker-compose up -d"
    echo ""
    echo "To view logs:"
    echo "docker-compose logs -f"
    echo ""
    echo "To stop the environment:"
    echo "docker-compose down"
else
    echo "docker compose up -d"
    echo ""
    echo "To view logs:"
    echo "docker compose logs -f"
    echo ""
    echo "To stop the environment:"
    echo "docker compose down"
fi
echo ""
echo "Services will be available at:"
echo "  PostgreSQL: localhost:5432"
echo "  pgAdmin:    http://localhost:8080"
echo "  API:        https://localhost:7169 (after starting the API)"