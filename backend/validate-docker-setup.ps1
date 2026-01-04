# Validate Docker Setup for Joiabagur PV Backend
# This script validates that the Docker environment is properly configured

Write-Host "Validating Docker setup for Joiabagur PV Backend..." -ForegroundColor Green

# Check if Docker is installed
Write-Host "Checking Docker installation..." -ForegroundColor Yellow
$dockerVersion = docker --version 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker is not installed or not in PATH." -ForegroundColor Red
    Write-Host "Please install Docker Desktop from: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Docker found: $dockerVersion" -ForegroundColor Green

# Check if Docker daemon is running
Write-Host "Checking Docker daemon..." -ForegroundColor Yellow
docker info >$null 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker daemon is not running." -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Docker daemon is running" -ForegroundColor Green

# Check if Docker Compose is available
Write-Host "Checking Docker Compose..." -ForegroundColor Yellow
$composeVersion = docker-compose --version 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker Compose is not available." -ForegroundColor Red
    Write-Host "Docker Compose should be included with Docker Desktop." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Docker Compose found: $composeVersion" -ForegroundColor Green

# Validate docker-compose.yml file
Write-Host "Validating docker-compose.yml..." -ForegroundColor Yellow
if (!(Test-Path "docker-compose.yml")) {
    Write-Host "❌ docker-compose.yml not found in current directory." -ForegroundColor Red
    exit 1
}

# Test docker-compose configuration
docker-compose config >$null 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ docker-compose.yml has configuration errors." -ForegroundColor Red
    docker-compose config
    exit 1
}

Write-Host "✅ docker-compose.yml is valid" -ForegroundColor Green

# Check if required ports are available
Write-Host "Checking port availability..." -ForegroundColor Yellow

$ports = @(5432, 8080)  # PostgreSQL and pgAdmin ports
$portsInUse = @()

foreach ($port in $ports) {
    $connection = Test-NetConnection -ComputerName localhost -Port $port -WarningAction SilentlyContinue
    if ($connection.TcpTestSucceeded) {
        $portsInUse += $port
    }
}

if ($portsInUse.Count -gt 0) {
    Write-Host "⚠️  Warning: Ports $($portsInUse -join ', ') are already in use." -ForegroundColor Yellow
    Write-Host "   This might cause conflicts when starting the containers." -ForegroundColor Yellow
} else {
    Write-Host "✅ Required ports are available" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎉 Docker setup validation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "You can now start the development environment with:" -ForegroundColor Cyan
Write-Host "docker-compose up -d" -ForegroundColor White
Write-Host ""
Write-Host "To view logs:" -ForegroundColor Cyan
Write-Host "docker-compose logs -f" -ForegroundColor White
Write-Host ""
Write-Host "To stop the environment:" -ForegroundColor Cyan
Write-Host "docker-compose down" -ForegroundColor White
Write-Host ""
Write-Host "Services will be available at:" -ForegroundColor Green
Write-Host "  PostgreSQL: localhost:5432" -ForegroundColor White
Write-Host "  pgAdmin:    http://localhost:8080" -ForegroundColor White
Write-Host "  API:        https://localhost:7169 (after starting the API)" -ForegroundColor White