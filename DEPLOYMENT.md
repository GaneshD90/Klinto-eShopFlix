# eShopFlix Microservices - Deployment Guide

> For Individual Developers and Learning - Optimized for personal projects and portfolio building with minimal Azure costs (~$50-80/month).

## Quick Start

### Option 1: Local Docker (FREE)
```powershell
.\scripts\docker-local.ps1 -Up -Build
# Access at http://localhost:5010
```

### Option 2: Azure AKS (~$50-80/month)
```powershell
# 1. Create Azure resources
.\scripts\deploy-dev.ps1 -SetupInfra

# 2. Update k8s/dev/secrets.yaml with connection strings

# 3. Build and push images
.\scripts\build-images.ps1 -All

# 4. Deploy
.\scripts\deploy-dev.ps1 -Deploy

# 5. Save money when not using!
az aks stop --name eshopflix-aks --resource-group eshopflix-rg
```

---

## Architecture Overview

```
eShopFlix on Azure (Budget Configuration)

   [Ingress/NGINX] <- Public IP
         |
   [Web Frontend] <- Razor Pages (1 pod)
         |
   [API Gateway] <- Ocelot routing (1 pod)
         |
   +-----+-----+------+---------+--------+-------+
   |Catalog|Auth|Cart |Payment |Order  |Stock  |
   |(1 pod)|(1) |(1)  |(1 pod) |(1 pod)|(1 pod)|
   +-------+----+-----+--------+-------+-------+
                      |
   [Azure SQL Serverless] <- Auto-pauses when idle

   Budget: AKS B2s (~$30) + ACR Basic (~$5) + SQL (~$20) = ~$55/mo
```

---

## Prerequisites

```powershell
# Required tools
- Docker Desktop
- Azure CLI (az)
- kubectl
- .NET 9 SDK

# Install Azure CLI
winget install Microsoft.AzureCLI

# Login to Azure
az login

# Install kubectl
az aks install-cli
```

---

## Local Development with Docker

Cost: FREE - Best for daily development!

```powershell
# Start everything
.\scripts\docker-local.ps1 -Up -Build

# View logs
.\scripts\docker-local.ps1 -Logs

# Stop everything  
.\scripts\docker-local.ps1 -Down
```

### Access Points
| Service | URL |
|---------|-----|
| Web App | http://localhost:5010 |
| API Gateway | http://localhost:5000 |
| Catalog API | http://localhost:5001 |
| Auth API | http://localhost:5002 |
| Cart API | http://localhost:5003 |
| Payment API | http://localhost:5004 |
| Order API | http://localhost:5005 |
| Stock API | http://localhost:5006 |
| SQL Server | localhost:1433 |

---

## Deploy to Azure AKS

Cost: ~$50-80/month - Great for learning and portfolio!

### What You Will Learn
- Azure Container Registry (ACR)
- Azure Kubernetes Service (AKS)
- Kubernetes Deployments, Services, Ingress
- Azure SQL Serverless
- kubectl commands

### Step 1: Create Azure Resources
```powershell
.\scripts\deploy-dev.ps1 -SetupInfra
```

This creates (budget-optimized):
| Resource | Config | Cost/Month |
|----------|--------|------------|
| AKS Cluster | 1x Standard_B2s | ~$30-40 |
| ACR | Basic tier | ~$5 |
| SQL Server | Serverless (auto-pause) | ~$15-25 |

### Step 2: Update Secrets
Edit `k8s/dev/secrets.yaml` with your SQL connection strings.

### Step 3: Build and Push Images
```powershell
.\scripts\build-images.ps1 -All -Registry eshopflixacr.azurecr.io
```

### Step 4: Deploy to Kubernetes
```powershell
.\scripts\deploy-dev.ps1 -Deploy
```

### Step 5: Check Status
```powershell
.\scripts\deploy-dev.ps1 -Status
```

---

## Cost Management

### Daily Workflow (Save 50%!)
```powershell
# END of day - STOP AKS
az aks stop --name eshopflix-aks --resource-group eshopflix-rg

# START of day - Resume AKS  
az aks start --name eshopflix-aks --resource-group eshopflix-rg
```

### Budget Breakdown
| Resource | Monthly Cost |
|----------|--------------|
| AKS (1 node, B2s) | ~$30-40 |
| ACR (Basic) | ~$5 |
| SQL Serverless (6 DBs) | ~$15-25 |
| Load Balancer | ~$5 |
| **Total** | **~$55-75** |

### Delete Everything When Done
```powershell
.\scripts\deploy-dev.ps1 -Teardown
```

---

## Learning Resources

### Useful kubectl Commands
```bash
# View all resources
kubectl get all -n eshopflix

# Check pod logs
kubectl logs -f deployment/catalog-service -n eshopflix

# Troubleshoot a pod
kubectl describe pod <pod-name> -n eshopflix

# Shell into a pod
kubectl exec -it <pod-name> -n eshopflix -- /bin/sh

# Restart a deployment
kubectl rollout restart deployment/web-frontend -n eshopflix
```

### Azure CLI Commands
```bash
# Check AKS status
az aks show --name eshopflix-aks --resource-group eshopflix-rg --query powerState

# List ACR images
az acr repository list --name eshopflixacr
```

---

## Project Structure

```
k8s/
  dev/                    <- USE THIS for learning!
    all-in-one.yaml       All K8s resources (simple)
    secrets.yaml          Your secrets
  base/                   Enterprise reference
  services/               Individual service files
  azure/                  Production autoscaling

scripts/
  deploy-dev.ps1          <- Main script for AKS
  docker-local.ps1        Local Docker
  build-images.ps1        Build and push
```

---

## Troubleshooting

### Pods not starting?
```bash
kubectl describe pod <pod-name> -n eshopflix
kubectl logs <pod-name> -n eshopflix
```

### SQL connection issues?
- SQL Serverless may take ~1 min to wake up
- Check firewall allows Azure services
- Verify connection string in secrets

### Images not pulling?
```bash
az acr login --name eshopflixacr
az aks check-acr --name eshopflix-aks --resource-group eshopflix-rg --acr eshopflixacr.azurecr.io
```

---

## Portfolio Tips

1. GitHub: Push code (exclude secrets via .gitignore)
2. LinkedIn: Add Azure Kubernetes Service, Docker, Microservices
3. Resume: Mention .NET 9, Docker, Kubernetes, Azure, Microservices
4. Demo: Record a video walkthrough of deployment

Good luck with your cloud journey!
