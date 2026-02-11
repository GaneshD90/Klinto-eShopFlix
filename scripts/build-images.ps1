# eShopFlix Microservices - Docker & Kubernetes Deployment Scripts
# PowerShell script for building and pushing Docker images

param(
    [Parameter(Mandatory=$false)]
    [string]$Registry = "eshopflixacr.azurecr.io",
    
    [Parameter(Mandatory=$false)]
    [string]$Tag = "latest",
    
    [Parameter(Mandatory=$false)]
    [switch]$Build,
    
    [Parameter(Mandatory=$false)]
    [switch]$Push,
    
    [Parameter(Mandatory=$false)]
    [switch]$All
)

$ErrorActionPreference = "Stop"

# Service configurations
$services = @(
    @{ Name = "catalog-service"; Path = "BackendServices/CatalogService/CatalogService.API"; Image = "eshopflix/catalog-service" },
    @{ Name = "auth-service"; Path = "BackendServices/AuthService/AuthService.API"; Image = "eshopflix/auth-service" },
    @{ Name = "cart-service"; Path = "BackendServices/CartService/CartService.API"; Image = "eshopflix/cart-service" },
    @{ Name = "payment-service"; Path = "BackendServices/PaymentService/PaymentService.API"; Image = "eshopflix/payment-service" },
    @{ Name = "order-service"; Path = "BackendServices/OrderService/OrderService.API"; Image = "eshopflix/order-service" },
    @{ Name = "stock-service"; Path = "BackendServices/StockService/StockService.API"; Image = "eshopflix/stock-service" },
    @{ Name = "api-gateway"; Path = "ApiGateways/OcelotApiGateway"; Image = "eshopflix/api-gateway" },
    @{ Name = "web-frontend"; Path = "FrontendServices/eShopFlix.Web"; Image = "eshopflix/web-frontend" }
)

function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Build-Images {
    Write-Header "Building Docker Images"
    
    foreach ($service in $services) {
        Write-Host "Building $($service.Name)..." -ForegroundColor Yellow
        $imageName = "$Registry/$($service.Image):$Tag"
        
        docker build -t $imageName -f "$($service.Path)/Dockerfile" .
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to build $($service.Name)" -ForegroundColor Red
            exit 1
        }
        Write-Host "Successfully built $imageName" -ForegroundColor Green
    }
}

function Push-Images {
    Write-Header "Pushing Docker Images to $Registry"
    
    # Login to Azure Container Registry
    Write-Host "Logging into Azure Container Registry..." -ForegroundColor Yellow
    az acr login --name ($Registry -replace ".azurecr.io", "")
    
    foreach ($service in $services) {
        Write-Host "Pushing $($service.Name)..." -ForegroundColor Yellow
        $imageName = "$Registry/$($service.Image):$Tag"
        
        docker push $imageName
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to push $($service.Name)" -ForegroundColor Red
            exit 1
        }
        Write-Host "Successfully pushed $imageName" -ForegroundColor Green
    }
}

# Main execution
if ($All) {
    Build-Images
    Push-Images
} else {
    if ($Build) { Build-Images }
    if ($Push) { Push-Images }
}

if (-not $Build -and -not $Push -and -not $All) {
    Write-Host "Usage: .\build-images.ps1 [-Build] [-Push] [-All] [-Registry <registry>] [-Tag <tag>]" -ForegroundColor Yellow
    Write-Host "`nExamples:" -ForegroundColor Cyan
    Write-Host "  .\build-images.ps1 -Build                    # Build all images locally"
    Write-Host "  .\build-images.ps1 -Push                     # Push all images to registry"
    Write-Host "  .\build-images.ps1 -All                      # Build and push all images"
    Write-Host "  .\build-images.ps1 -All -Tag v1.0.0          # Build and push with specific tag"
}
