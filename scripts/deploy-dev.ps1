# eShopFlix - Budget-Friendly AKS Deployment for Individual Developers
# Optimized for learning with minimal Azure costs (~$50-80/month)

param(
    [Parameter(Mandatory=$false)]
    [string]$AksCluster = "eshopflix-aks",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "eshopflix-rg",
    
    [Parameter(Mandatory=$false)]
    [string]$AcrName = "eshopflixacr",
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [switch]$SetupInfra,
    
    [Parameter(Mandatory=$false)]
    [switch]$Deploy,
    
    [Parameter(Mandatory=$false)]
    [switch]$Teardown,
    
    [Parameter(Mandatory=$false)]
    [switch]$Status
)

$ErrorActionPreference = "Stop"

function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host " $Message" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-Cost {
    param([string]$Resource, [string]$Cost)
    Write-Host "  ?? $Resource : $Cost" -ForegroundColor Yellow
}

function Setup-BudgetInfrastructure {
    Write-Header "Setting up BUDGET-FRIENDLY Azure Infrastructure"
    
    Write-Host "?? This will create:" -ForegroundColor White
    Write-Cost "AKS Cluster" "~$30-40/month (1 node, B2s)"
    Write-Cost "ACR (Basic)" "~$5/month"
    Write-Cost "Azure SQL Serverless" "~$5-15/month (auto-pause)"
    Write-Host "`n  Total Estimated: ~$50-60/month" -ForegroundColor Green
    Write-Host ""
    
    $confirm = Read-Host "Continue? (y/n)"
    if ($confirm -ne "y") {
        Write-Host "Cancelled." -ForegroundColor Yellow
        return
    }
    
    # Create Resource Group
    Write-Host "`n?? Creating Resource Group: $ResourceGroup" -ForegroundColor Yellow
    az group create --name $ResourceGroup --location $Location --output none
    
    # Create Azure Container Registry (Basic tier - cheapest)
    Write-Host "?? Creating ACR (Basic tier): $AcrName" -ForegroundColor Yellow
    az acr create `
        --resource-group $ResourceGroup `
        --name $AcrName `
        --sku Basic `
        --output none
    
    # Create AKS Cluster with MINIMAL configuration
    Write-Host "??  Creating AKS Cluster (Budget config - ~10 minutes)..." -ForegroundColor Yellow
    Write-Host "   - 1 node (Standard_B2s - burstable, cheapest)" -ForegroundColor Gray
    Write-Host "   - No monitoring addon (saves cost)" -ForegroundColor Gray
    Write-Host "   - System managed identity" -ForegroundColor Gray
    
    az aks create `
        --resource-group $ResourceGroup `
        --name $AksCluster `
        --node-count 1 `
        --node-vm-size Standard_B2s `
        --enable-managed-identity `
        --attach-acr $AcrName `
        --generate-ssh-keys `
        --network-plugin kubenet `
        --output none
    
    # Get AKS credentials
    Write-Host "?? Getting AKS credentials..." -ForegroundColor Yellow
    az aks get-credentials --resource-group $ResourceGroup --name $AksCluster --overwrite-existing
    
    # Install NGINX Ingress Controller
    Write-Host "?? Installing NGINX Ingress Controller..." -ForegroundColor Yellow
    kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.9.4/deploy/static/provider/cloud/deploy.yaml
    
    # Create Azure SQL Server (Serverless - auto-pause, very cheap for learning)
    Write-Host "???  Creating Azure SQL Server (Serverless)..." -ForegroundColor Yellow
    $sqlServerName = "$($AcrName)sql"
    $sqlPassword = "P@ssw0rd$(Get-Random -Maximum 9999)!"
    
    az sql server create `
        --resource-group $ResourceGroup `
        --name $sqlServerName `
        --admin-user sqladmin `
        --admin-password $sqlPassword `
        --output none
    
    # Allow Azure services to access SQL
    az sql server firewall-rule create `
        --resource-group $ResourceGroup `
        --server $sqlServerName `
        --name AllowAzureServices `
        --start-ip-address 0.0.0.0 `
        --end-ip-address 0.0.0.0 `
        --output none
    
    # Create databases (Serverless tier - auto-pause after 1 hour of inactivity)
    $databases = @("CatalogDb", "AuthDb", "CartDb", "PaymentDb", "OrderDb", "StockDb")
    foreach ($db in $databases) {
        Write-Host "   Creating database: $db" -ForegroundColor Gray
        az sql db create `
            --resource-group $ResourceGroup `
            --server $sqlServerName `
            --name $db `
            --edition GeneralPurpose `
            --compute-model Serverless `
            --family Gen5 `
            --capacity 1 `
            --auto-pause-delay 60 `
            --min-capacity 0.5 `
            --output none
    }
    
    Write-Header "? Infrastructure Setup Complete!"
    
    Write-Host "?? IMPORTANT - Save these details:" -ForegroundColor Yellow
    Write-Host "   SQL Server: $sqlServerName.database.windows.net" -ForegroundColor White
    Write-Host "   SQL Admin: sqladmin" -ForegroundColor White
    Write-Host "   SQL Password: $sqlPassword" -ForegroundColor White
    Write-Host ""
    Write-Host "? Next Steps:" -ForegroundColor Cyan
    Write-Host "   1. Update k8s/dev/secrets.yaml with the SQL connection strings"
    Write-Host "   2. Run: .\scripts\build-images.ps1 -All -Registry $AcrName.azurecr.io"
    Write-Host "   3. Run: .\scripts\deploy-dev.ps1 -Deploy"
    Write-Host ""
    Write-Host "?? Cost Saving Tips:" -ForegroundColor Green
    Write-Host "   - SQL auto-pauses after 1 hour of inactivity"
    Write-Host "   - Stop AKS when not using: az aks stop --name $AksCluster --resource-group $ResourceGroup"
    Write-Host "   - Start AKS when needed: az aks start --name $AksCluster --resource-group $ResourceGroup"
}

function Deploy-Application {
    Write-Header "Deploying eShopFlix to AKS"
    
    # Get AKS credentials
    Write-Host "?? Getting AKS credentials..." -ForegroundColor Yellow
    az aks get-credentials --resource-group $ResourceGroup --name $AksCluster --overwrite-existing
    
    # Apply secrets first
    Write-Host "?? Applying secrets..." -ForegroundColor Yellow
    kubectl apply -f k8s/dev/secrets.yaml
    
    # Apply all-in-one configuration
    Write-Host "??  Deploying all services..." -ForegroundColor Yellow
    $content = Get-Content "k8s/dev/all-in-one.yaml" -Raw
    $content = $content -replace '\$\{ACR_NAME\}', $AcrName
    $content | kubectl apply -f -
    
    # Wait for pods
    Write-Host "`n? Waiting for pods to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    # Show status
    Show-Status
}

function Show-Status {
    Write-Header "eShopFlix Status"
    
    Write-Host "?? Pods:" -ForegroundColor Cyan
    kubectl get pods -n eshopflix -o wide
    
    Write-Host "`n?? Services:" -ForegroundColor Cyan
    kubectl get svc -n eshopflix
    
    Write-Host "`n?? Ingress:" -ForegroundColor Cyan
    kubectl get ingress -n eshopflix
    
    # Get external IP
    $externalIP = kubectl get ingress -n eshopflix -o jsonpath='{.items[0].status.loadBalancer.ingress[0].ip}' 2>$null
    if ($externalIP) {
        Write-Host "`n? Application URL: http://$externalIP" -ForegroundColor Green
    } else {
        Write-Host "`n? External IP not ready yet. Run this script with -Status again." -ForegroundColor Yellow
    }
}

function Teardown-Infrastructure {
    Write-Header "??  TEARDOWN - Delete All Azure Resources"
    
    Write-Host "This will DELETE:" -ForegroundColor Red
    Write-Host "  - AKS Cluster: $AksCluster"
    Write-Host "  - ACR: $AcrName"  
    Write-Host "  - SQL Server and all databases"
    Write-Host "  - Resource Group: $ResourceGroup"
    Write-Host ""
    
    $confirm = Read-Host "Type 'DELETE' to confirm"
    if ($confirm -ne "DELETE") {
        Write-Host "Cancelled." -ForegroundColor Yellow
        return
    }
    
    Write-Host "`n???  Deleting Resource Group (this deletes everything)..." -ForegroundColor Yellow
    az group delete --name $ResourceGroup --yes --no-wait
    
    Write-Host "? Deletion initiated. Resources will be removed in a few minutes." -ForegroundColor Green
}

# Main execution
if ($SetupInfra) { Setup-BudgetInfrastructure }
elseif ($Deploy) { Deploy-Application }
elseif ($Teardown) { Teardown-Infrastructure }
elseif ($Status) { Show-Status }
else {
    Write-Host "eShopFlix - Budget AKS Deployment" -ForegroundColor Cyan
    Write-Host "`nUsage: .\deploy-dev.ps1 [-SetupInfra] [-Deploy] [-Teardown] [-Status]" -ForegroundColor Yellow
    Write-Host "`nCommands:" -ForegroundColor Cyan
    Write-Host "  -SetupInfra   Create Azure resources (AKS, ACR, SQL) - ~$50-60/month"
    Write-Host "  -Deploy       Deploy the application to AKS"
    Write-Host "  -Status       Show current deployment status"
    Write-Host "  -Teardown     DELETE all Azure resources (saves money!)"
    Write-Host "`n?? Quick Start:" -ForegroundColor Green
    Write-Host "  1. .\scripts\deploy-dev.ps1 -SetupInfra"
    Write-Host "  2. Update k8s/dev/secrets.yaml with your values"
    Write-Host "  3. .\scripts\build-images.ps1 -All"
    Write-Host "  4. .\scripts\deploy-dev.ps1 -Deploy"
    Write-Host "`n?? Save Money:" -ForegroundColor Yellow
    Write-Host "  Stop AKS: az aks stop --name $AksCluster --resource-group $ResourceGroup"
    Write-Host "  Start AKS: az aks start --name $AksCluster --resource-group $ResourceGroup"
}
