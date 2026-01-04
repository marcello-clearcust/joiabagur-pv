# Setup Development HTTPS Certificates for Joiabagur PV Backend
# This script configures development certificates for HTTPS development

Write-Host "Setting up development HTTPS certificates for Joiabagur PV Backend..." -ForegroundColor Green

# Check if dotnet dev-certs is available
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: .NET SDK not found. Please install .NET 10 SDK first." -ForegroundColor Red
    exit 1
}

Write-Host "Found .NET SDK version: $dotnetVersion" -ForegroundColor Cyan

# Check if certificates already exist
Write-Host "Checking for existing development certificates..." -ForegroundColor Yellow
dotnet dev-certs https --check

if ($LASTEXITCODE -eq 0) {
    Write-Host "Development certificates already exist." -ForegroundColor Green
} else {
    Write-Host "Creating new development certificates..." -ForegroundColor Yellow
    dotnet dev-certs https

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Failed to create development certificates." -ForegroundColor Red
        exit 1
    }
}

# Trust the certificates (requires admin privileges on Windows)
Write-Host "Trusting development certificates (requires administrator privileges)..." -ForegroundColor Yellow

# Check if running as administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if ($isAdmin) {
    dotnet dev-certs https --trust

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Development certificates trusted successfully!" -ForegroundColor Green
    } else {
        Write-Host "Warning: Failed to trust certificates automatically." -ForegroundColor Yellow
        Write-Host "You may need to trust the certificate manually in Windows Certificate Manager." -ForegroundColor Yellow
    }
} else {
    Write-Host "Not running as administrator. Please run the following command as administrator to trust the certificate:" -ForegroundColor Yellow
    Write-Host "dotnet dev-certs https --trust" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "HTTPS setup complete! You can now run the application with:" -ForegroundColor Green
Write-Host "dotnet run --launch-profile https" -ForegroundColor Cyan
Write-Host ""
Write-Host "The API will be available at:" -ForegroundColor Green
Write-Host "  HTTPS: https://localhost:7169" -ForegroundColor Cyan
Write-Host "  HTTP:  http://localhost:5056" -ForegroundColor Cyan
Write-Host "  Swagger: https://localhost:7169/swagger" -ForegroundColor Cyan