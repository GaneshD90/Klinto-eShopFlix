# eShopFlix - Kubernetes Deployment Script
# PowerShell script for deploying to AKS

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "dev",
    
    [Parameter(Mandatory=$false)]
    [string]$AksCluster = "eshopflix-aks",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "eshopflix-rg",
    
    [Parameter(Mandatory=$false)]
    [string]$AcrName = "eshopflixacr",
    
    [Parameter(Mandatory=$false)]
    [switch]$SetupInfra,
    
    [Parameter(Mandatory=$false)]
    [switch]$Deploy,
    
    [Parameter(Mandatory=$false)]
    [switch]$All
)

$ErrorActionPreference = "Stop"

function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Setup-AzureInfrastructure {
    Write-Header "Setting up Azure Infrastructure"
    
    # Create Resource Group
    Write-Host "Creating Resource Group: $ResourceGroup" -ForegroundColor Yellow
    az group create --name $ResourceGroup --location eastus
    
    # Create Azure Container Registry
    Write-Host "Creating Azure Container Registry: $AcrName" -ForegroundColor Yellow
    az acr create --resource-group $ResourceGroup --name $AcrName --sku Standard
    
    # Create AKS Cluster
    Write-Host "Creating AKS Cluster: $AksCluster (this may take 10-15 minutes)" -ForegroundColor Yellow
    az aks create `
        --resource-group $ResourceGroup `
        --name $AksCluster `
        --node-count 3 `
        --node-vm-size Standard_DS2_v2 `
        --enable-managed-identity `
        --attach-acr $AcrName `
        --generate-ssh-keys `
        --network-plugin azure `
        --enable-addons monitoring
    
    # Get AKS credentials
    Write-Host "Getting AKS credentials..." -ForegroundColor Yellow
    az aks get-credentials --resource-group $ResourceGroup --name $AksCluster --overwrite-existing
    
    # Install NGINX Ingress Controller
    Write-Host "Installing NGINX Ingress Controller..." -ForegroundColor Yellow
    kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.9.4/deploy/static/provider/cloud/deploy.yaml
    
    Write-Host "Azure Infrastructure setup complete!" -ForegroundColor Green
}

function Deploy-Application {
    Write-Header "Deploying eShopFlix to Kubernetes"
    
    # Get AKS credentials
    Write-Host "Getting AKS credentials..." -ForegroundColor Yellow
    az aks get-credentials --resource-group $ResourceGroup --name $AksCluster --overwrite-existing
    
    # Replace ACR name in manifests
    $acrUrl = "$AcrName.azurecr.io"
    
    # Apply base configurations
    Write-Host "Applying namespace..." -ForegroundColor Yellow
    kubectl apply -f k8s/base/namespace.yaml
    
    Write-Host "Applying ConfigMaps..." -ForegroundColor Yellow
    kubectl apply -f k8s/base/configmaps.yaml
    
    Write-Host "Applying Secrets..." -ForegroundColor Yellow
    # Note: Update secrets with actual values before applying!
    kubectl apply -f k8s/base/secrets.yaml
    
    # Apply services
    Write-Host "Deploying backend services..." -ForegroundColor Yellow
    $serviceFiles = Get-ChildItem -Path "k8s/services" -Filter "*.yaml"
    foreach ($file in $serviceFiles) {
        Write-Host "  Applying $($file.Name)..." -ForegroundColor Gray
        # Replace placeholder with actual ACR name
        $content = Get-Content $file.FullName -Raw
        $content = $content -replace '\$\{ACR_NAME\}', $AcrName
        $content | kubectl apply -f -
    }
    
    # Apply ingress
    Write-Host "Applying Ingress configuration..." -ForegroundColor Yellow
    kubectl apply -f k8s/ingress/ingress.yaml
    
    # Apply Azure-specific configurations
    if ($Environment -eq "prod") {
        Write-Host "Applying production configurations..." -ForegroundColor Yellow
        kubectl apply -f k8s/azure/autoscaling.yaml
    }
    
    Write-Host "`nDeployment complete!" -ForegroundColor Green
    
    # Show status
    Write-Host "`nPod Status:" -ForegroundColor Cyan
    kubectl get pods -n eshopflix
    
    Write-Host "`nService Status:" -ForegroundColor Cyan
    kubectl get svc -n eshopflix
    
    Write-Host "`nIngress Status:" -ForegroundColor Cyan
    kubectl get ingress -n eshopflix
}

# Main execution
if ($All) {
    Setup-AzureInfrastructure
    Deploy-Application
} else {
    if ($SetupInfra) { Setup-AzureInfrastructure }
    if ($Deploy) { Deploy-Application }
}

if (-not $SetupInfra -and -not $Deploy -and -not $All) {
    Write-Host "Usage: .\deploy-k8s.ps1 [-SetupInfra] [-Deploy] [-All]" -ForegroundColor Yellow
    Write-Host "`nExamples:" -ForegroundColor Cyan
    Write-Host "  .\deploy-k8s.ps1 -SetupInfra                 # Create Azure resources (ACR, AKS)"
    Write-Host "  .\deploy-k8s.ps1 -Deploy                     # Deploy app to existing AKS"
    Write-Host "  .\deploy-k8s.ps1 -All                        # Create infra and deploy"
    Write-Host "  .\deploy-k8s.ps1 -Deploy -Environment prod   # Deploy with production settings"
}
