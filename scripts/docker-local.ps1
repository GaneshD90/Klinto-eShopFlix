# eShopFlix - Local Docker Development
# Quick start script for local development with Docker Compose

param(
    [Parameter(Mandatory=$false)]
    [switch]$Up,
    
    [Parameter(Mandatory=$false)]
    [switch]$Down,
    
    [Parameter(Mandatory=$false)]
    [switch]$Build,
    
    [Parameter(Mandatory=$false)]
    [switch]$Logs,
    
    [Parameter(Mandatory=$false)]
    [string]$Service
)

$ErrorActionPreference = "Stop"

function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

if ($Up) {
    Write-Header "Starting eShopFlix Microservices"
    
    if ($Build) {
        Write-Host "Building and starting all services..." -ForegroundColor Yellow
        docker-compose up -d --build
    } else {
        Write-Host "Starting all services..." -ForegroundColor Yellow
        docker-compose up -d
    }
    
    Write-Host "`nServices are starting up..." -ForegroundColor Green
    Write-Host "`nAccess points:" -ForegroundColor Cyan
    Write-Host "  Web Application:  http://localhost:5010" -ForegroundColor White
    Write-Host "  API Gateway:      http://localhost:5000" -ForegroundColor White
    Write-Host "  Catalog Service:  http://localhost:5001" -ForegroundColor White
    Write-Host "  Auth Service:     http://localhost:5002" -ForegroundColor White
    Write-Host "  Cart Service:     http://localhost:5003" -ForegroundColor White
    Write-Host "  Payment Service:  http://localhost:5004" -ForegroundColor White
    Write-Host "  Order Service:    http://localhost:5005" -ForegroundColor White
    Write-Host "  Stock Service:    http://localhost:5006" -ForegroundColor White
    Write-Host "  SQL Server:       localhost:1433" -ForegroundColor White
    
    Write-Host "`nView logs with: .\docker-local.ps1 -Logs" -ForegroundColor Gray
}

if ($Down) {
    Write-Header "Stopping eShopFlix Microservices"
    docker-compose down
    Write-Host "All services stopped." -ForegroundColor Green
}

if ($Logs) {
    if ($Service) {
        Write-Host "Showing logs for $Service..." -ForegroundColor Yellow
        docker-compose logs -f $Service
    } else {
        Write-Host "Showing logs for all services..." -ForegroundColor Yellow
        docker-compose logs -f
    }
}

if ($Build -and -not $Up) {
    Write-Header "Building Docker Images"
    docker-compose build
    Write-Host "Build complete!" -ForegroundColor Green
}

if (-not $Up -and -not $Down -and -not $Logs -and -not $Build) {
    Write-Host "eShopFlix Local Docker Development" -ForegroundColor Cyan
    Write-Host "`nUsage: .\docker-local.ps1 [-Up] [-Down] [-Build] [-Logs] [-Service <name>]" -ForegroundColor Yellow
    Write-Host "`nExamples:" -ForegroundColor Cyan
    Write-Host "  .\docker-local.ps1 -Up                # Start all services"
    Write-Host "  .\docker-local.ps1 -Up -Build         # Build and start all services"
    Write-Host "  .\docker-local.ps1 -Down              # Stop all services"
    Write-Host "  .\docker-local.ps1 -Logs              # View all logs"
    Write-Host "  .\docker-local.ps1 -Logs -Service cart-service  # View specific service logs"
}
