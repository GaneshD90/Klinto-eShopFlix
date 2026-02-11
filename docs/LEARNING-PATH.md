# eShopFlix - Complete Learning Path: Docker, Kubernetes & Azure
## From Zero to Interview-Ready Full Stack Developer with Cloud Expertise

> ?? **Goal**: Master containerization, orchestration, and cloud deployment using YOUR real microservices project.

---

## ?? Learning Path Overview

| Week | Topic | Hands-On Project |
|------|-------|------------------|
| Week 1 | Docker Fundamentals | Containerize all 8 services |
| Week 2 | Docker Compose & Multi-Container | Run full app locally |
| Week 3 | Kubernetes Basics | Deploy to local K8s (Docker Desktop) |
| Week 4 | Azure Fundamentals | Setup ACR, AKS, SQL |
| Week 5 | Production Deployment | Deploy to AKS with monitoring |
| Week 6 | Interview Prep | Practice explanations & demos |

---

# ?? WEEK 1: Docker Fundamentals

## Day 1: Understanding Docker Concepts

### What You Need to Know for Interviews
```
Q: What is Docker?
A: Docker is a containerization platform that packages applications with 
   their dependencies into isolated, portable containers that run consistently 
   across any environment.

Q: Why use Docker for microservices?
A: 1. Consistency - Same environment in dev, test, prod
   2. Isolation - Each service runs independently
   3. Portability - Run anywhere Docker is installed
   4. Scalability - Easy to scale individual services
   5. Resource efficiency - Lighter than VMs
```

### Hands-On: Understand Your Dockerfile
Open `BackendServices/CatalogService/CatalogService.API/Dockerfile`:

```dockerfile
# STAGE 1: Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# STAGE 2: Build stage - compile the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BackendServices/CatalogService/CatalogService.API/CatalogService.API.csproj", "..."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

# STAGE 3: Publish stage - create production-ready output
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# STAGE 4: Final image - minimal runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.API.dll"]
```

### Interview Explanation:
> "I use multi-stage builds to create optimized Docker images. The first stage 
> uses the full SDK to build and publish the application. The final stage uses 
> only the runtime image, reducing the final image size from ~700MB to ~200MB. 
> This improves security and deployment speed."

---

## Day 2: Build Your First Docker Image

### Exercise 1: Build Catalog Service
```powershell
# Navigate to solution root
cd E:\GaneshOfficial\source\.NET MicroServices\MicroservicesApp

# Build the Catalog Service image
docker build -t eshopflix/catalog-service:v1 -f BackendServices/CatalogService/CatalogService.API/Dockerfile .

# Verify the image was created
docker images | Select-String "eshopflix"
```

### Exercise 2: Inspect the Image
```powershell
# See image layers and size
docker history eshopflix/catalog-service:v1

# Inspect image metadata
docker inspect eshopflix/catalog-service:v1
```

### Exercise 3: Run the Container
```powershell
# Run the container
docker run -d --name catalog-test -p 5001:8080 eshopflix/catalog-service:v1

# Check if it's running
docker ps

# View logs
docker logs catalog-test

# Stop and remove
docker stop catalog-test
docker rm catalog-test
```

### ?? Interview Practice
```
Q: Explain the difference between docker build and docker run?
A: "docker build creates an image from a Dockerfile - it's like creating 
   a template. docker run creates a container from that image - it's like 
   creating an instance of the template that actually runs."

Q: What is the difference between COPY and ADD in Dockerfile?
A: "COPY simply copies files from host to container. ADD can also extract 
   tar files and download from URLs. Best practice is to use COPY unless 
   you specifically need ADD's extra features."

Q: What's the difference between CMD and ENTRYPOINT?
A: "ENTRYPOINT defines the main command that always runs. CMD provides 
   default arguments that can be overridden. I use ENTRYPOINT for the 
   main application and CMD for default parameters."
```

---

## Day 3: Docker Commands Mastery

### Essential Commands Cheat Sheet
```powershell
# === IMAGE COMMANDS ===
docker images                    # List all images
docker build -t name:tag .       # Build image from Dockerfile
docker pull image:tag            # Download image from registry
docker push image:tag            # Upload image to registry
docker rmi image:tag             # Remove image
docker image prune               # Remove unused images

# === CONTAINER COMMANDS ===
docker ps                        # List running containers
docker ps -a                     # List all containers (including stopped)
docker run -d --name x image     # Run container in background
docker run -it image /bin/sh     # Run container interactively
docker exec -it container sh     # Execute command in running container
docker logs container            # View container logs
docker logs -f container         # Follow logs (live)
docker stop container            # Stop container gracefully
docker kill container            # Force stop container
docker rm container              # Remove container
docker container prune           # Remove all stopped containers

# === NETWORKING ===
docker network ls                # List networks
docker network create mynet      # Create network
docker run --network mynet       # Connect container to network

# === VOLUMES ===
docker volume ls                 # List volumes
docker volume create myvol       # Create volume
docker run -v myvol:/app/data    # Mount volume

# === DEBUGGING ===
docker inspect container         # Detailed container info
docker stats                     # Live resource usage
docker top container             # Running processes in container
```

### Exercise: Debug a Container
```powershell
# Run a container
docker run -d --name debug-test eshopflix/catalog-service:v1

# Check resource usage
docker stats debug-test --no-stream

# See running processes
docker top debug-test

# Get inside the container
docker exec -it debug-test /bin/sh

# Inside container, explore:
ls -la
cat /app/appsettings.json
exit

# Clean up
docker stop debug-test && docker rm debug-test
```

---

## Day 4-5: Build All Service Images

### Exercise: Build All 8 Services
```powershell
# Create a script to build all images
$services = @(
    @{Name="catalog-service"; Path="BackendServices/CatalogService/CatalogService.API/Dockerfile"},
    @{Name="auth-service"; Path="BackendServices/AuthService/AuthService.API/Dockerfile"},
    @{Name="cart-service"; Path="BackendServices/CartService/CartService.API/Dockerfile"},
    @{Name="payment-service"; Path="BackendServices/PaymentService/PaymentService.API/Dockerfile"},
    @{Name="order-service"; Path="BackendServices/OrderService/OrderService.API/Dockerfile"},
    @{Name="stock-service"; Path="BackendServices/StockService/StockService.API/Dockerfile"},
    @{Name="api-gateway"; Path="ApiGateways/OcelotApiGateway/Dockerfile"},
    @{Name="web-frontend"; Path="FrontendServices/eShopFlix.Web/Dockerfile"}
)

foreach ($svc in $services) {
    Write-Host "Building $($svc.Name)..." -ForegroundColor Yellow
    docker build -t "eshopflix/$($svc.Name):v1" -f $svc.Path .
}

# Verify all images
docker images | Select-String "eshopflix"
```

### ?? Week 1 Interview Questions
```
Q: How do you reduce Docker image size?
A: "I use multi-stage builds to separate build dependencies from runtime.
   I use Alpine-based images when possible. I combine RUN commands to 
   reduce layers. I add a .dockerignore file to exclude unnecessary files."

Q: How do you handle secrets in Docker?
A: "Never hardcode secrets in Dockerfile or images. I use environment 
   variables passed at runtime, Docker secrets for Swarm, or external 
   secret managers like Azure Key Vault."

Q: What is the difference between a Docker image and container?
A: "An image is a read-only template containing the application and 
   dependencies. A container is a running instance of an image. You can 
   create multiple containers from one image."
```

---

# ?? WEEK 2: Docker Compose & Multi-Container Apps

## Day 1: Understanding Docker Compose

### What You Need to Know
```
Q: What is Docker Compose?
A: "Docker Compose is a tool for defining and running multi-container 
   applications. It uses a YAML file to configure services, networks, 
   and volumes, then starts everything with a single command."

Q: When would you use Docker Compose vs Kubernetes?
A: "Docker Compose is ideal for local development and simple deployments.
   Kubernetes is for production-scale orchestration with advanced features 
   like auto-scaling, self-healing, and rolling updates."
```

### Understand Your docker-compose.yml
```yaml
# Key sections explained:
version: '3.8'                    # Compose file version

services:                         # Define all your services
  catalog-service:
    image: catalogservice         # Image name
    build:                        # How to build the image
      context: .
      dockerfile: BackendServices/CatalogService/.../Dockerfile
    environment:                  # Environment variables
      - ASPNETCORE_ENVIRONMENT=Docker
      - ConnectionStrings__DefaultConnection=Server=sqlserver;...
    ports:                        # Port mapping (host:container)
      - "5001:8080"
    depends_on:                   # Start order dependency
      sqlserver:
        condition: service_healthy
    networks:                     # Network to join
      - eshopflix-network

networks:                         # Define networks
  eshopflix-network:
    driver: bridge

volumes:                          # Define persistent storage
  sqlserver-data:
```

---

## Day 2: Run Full Application Locally

### Exercise: Start Everything
```powershell
# Start all services
docker-compose up -d --build

# Watch the startup
docker-compose logs -f

# Check status
docker-compose ps

# Access the application
Start-Process "http://localhost:5010"
```

### Exercise: Understand Service Communication
```powershell
# See how services communicate
docker network ls
docker network inspect eshopflix-network

# Test internal DNS
docker exec -it eshopflix-gateway ping catalog-service
```

### Exercise: Scale a Service
```powershell
# Scale cart service to 3 instances
docker-compose up -d --scale cart-service=3

# Verify
docker-compose ps | Select-String "cart"
```

---

## Day 3-4: Debugging & Troubleshooting

### Common Issues and Solutions
```powershell
# Issue: Container keeps restarting
docker-compose logs cart-service          # Check logs for errors
docker inspect eshopflix-cart              # Check exit code

# Issue: Service can't connect to database
docker exec -it eshopflix-cart ping sqlserver   # Test connectivity
docker exec -it eshopflix-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "SELECT 1"

# Issue: Port already in use
docker-compose down                        # Stop all services
netstat -ano | findstr "5001"              # Find what's using the port

# Issue: Out of disk space
docker system df                           # Check Docker disk usage
docker system prune -a                     # Clean up everything unused
```

### Health Checks
```yaml
# Add to docker-compose.yml for better reliability
services:
  catalog-service:
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

---

## Day 5: Docker Compose Best Practices

### Exercise: Create Environment-Specific Configs
```powershell
# docker-compose.yml - Base configuration
# docker-compose.override.yml - Development overrides (auto-loaded)
# docker-compose.prod.yml - Production settings

# Run with specific config
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### ?? Week 2 Interview Questions
```
Q: How do services communicate in Docker Compose?
A: "Services on the same network can communicate using service names 
   as hostnames. Docker's internal DNS resolves service names to 
   container IPs. For example, my API Gateway calls 'http://catalog-service:8080'."

Q: How do you handle database migrations in Docker?
A: "I create an initialization script that runs when the database 
   container starts. For .NET, I can use EF Core migrations in the 
   application startup or a separate migration container."

Q: What is the difference between 'depends_on' and health checks?
A: "depends_on only waits for the container to start, not for the 
   application inside to be ready. Health checks verify the application 
   is actually responding. I use 'condition: service_healthy' for 
   proper startup ordering."
```

---

# ?? WEEK 3: Kubernetes Basics

## Day 1: Kubernetes Concepts

### Core Concepts (MUST KNOW for Interviews)
```
???????????????????????????????????????????????????????????????????
?                        KUBERNETES CLUSTER                        ?
???????????????????????????????????????????????????????????????????
?                                                                   ?
?  ????????????????????????????????????????????????????????????   ?
?  ?                      NAMESPACE: eshopflix                  ?   ?
?  ?                                                            ?   ?
?  ?  ???????????????????????????????????????????????????????  ?   ?
?  ?  ?              DEPLOYMENT: catalog-service             ?  ?   ?
?  ?  ?                                                       ?  ?   ?
?  ?  ?   ???????????????  ???????????????                   ?  ?   ?
?  ?  ?   ?   POD 1     ?  ?   POD 2     ?   (ReplicaSet)    ?  ?   ?
?  ?  ?   ? ??????????? ?  ? ??????????? ?                   ?  ?   ?
?  ?  ?   ? ?Container? ?  ? ?Container? ?                   ?  ?   ?
?  ?  ?   ? ??????????? ?  ? ??????????? ?                   ?  ?   ?
?  ?  ?   ???????????????  ???????????????                   ?  ?   ?
?  ?  ???????????????????????????????????????????????????????  ?   ?
?  ?                          ?                                 ?   ?
?  ?                          ?                                 ?   ?
?  ?  ???????????????????????????????????????????????????????  ?   ?
?  ?  ?           SERVICE: catalog-service                   ?  ?   ?
?  ?  ?           (ClusterIP - Internal Load Balancer)       ?  ?   ?
?  ?  ???????????????????????????????????????????????????????  ?   ?
?  ?                          ?                                 ?   ?
?  ?                          ?                                 ?   ?
?  ?  ???????????????????????????????????????????????????????  ?   ?
?  ?  ?                    INGRESS                           ?  ?   ?
?  ?  ?            (External Load Balancer + Routing)        ?  ?   ?
?  ?  ???????????????????????????????????????????????????????  ?   ?
?  ?                          ?                                 ?   ?
?  ????????????????????????????????????????????????????????????   ?
?                              ?                                    ?
?                         INTERNET                                  ?
???????????????????????????????????????????????????????????????????

KEY CONCEPTS:
• Cluster    = The entire Kubernetes environment
• Node       = A worker machine (VM or physical) that runs pods
• Namespace  = Virtual cluster for resource isolation
• Pod        = Smallest deployable unit (1+ containers)
• Deployment = Manages pods (scaling, updates, rollbacks)
• Service    = Stable network endpoint for pods
• Ingress    = External HTTP/HTTPS routing
• ConfigMap  = Configuration data (non-secret)
• Secret     = Sensitive data (passwords, keys)
```

### Interview Explanations
```
Q: What is a Pod?
A: "A Pod is the smallest deployable unit in Kubernetes. It can contain 
   one or more containers that share storage and network. In my project, 
   each microservice runs in its own pod. Pods are ephemeral - Kubernetes 
   can recreate them anytime."

Q: What is a Deployment?
A: "A Deployment manages the desired state of pods. I specify how many 
   replicas I want, and Kubernetes ensures that many are always running. 
   It also handles rolling updates and rollbacks."

Q: What is a Service?
A: "A Service provides a stable network endpoint for pods. Since pod IPs 
   change when they restart, Services give a consistent DNS name and IP. 
   I use ClusterIP for internal communication and LoadBalancer for 
   external access."

Q: What is an Ingress?
A: "Ingress manages external HTTP/HTTPS access to services. It provides 
   URL routing, SSL termination, and load balancing. I use NGINX Ingress 
   Controller to route traffic to my API Gateway and Web Frontend."
```

---

## Day 2: Setup Local Kubernetes

### Enable Kubernetes in Docker Desktop
```
1. Open Docker Desktop
2. Go to Settings ? Kubernetes
3. Check "Enable Kubernetes"
4. Click "Apply & Restart"
5. Wait for Kubernetes to start (green indicator)
```

### Verify Installation
```powershell
# Check kubectl is working
kubectl version

# Check cluster info
kubectl cluster-info

# Check nodes
kubectl get nodes
```

---

## Day 3: Your First Kubernetes Deployment

### Exercise: Deploy Catalog Service Manually
```powershell
# Create namespace
kubectl create namespace eshopflix-test

# Create a simple deployment
kubectl create deployment catalog-service --image=eshopflix/catalog-service:v1 -n eshopflix-test

# Check the deployment
kubectl get deployments -n eshopflix-test

# Check the pods
kubectl get pods -n eshopflix-test

# Expose as a service
kubectl expose deployment catalog-service --port=80 --target-port=8080 -n eshopflix-test

# Check the service
kubectl get services -n eshopflix-test

# Port forward to access locally
kubectl port-forward service/catalog-service 5001:80 -n eshopflix-test

# Access in browser: http://localhost:5001/api/catalog

# Clean up
kubectl delete namespace eshopflix-test
```

---

## Day 4: Understanding YAML Manifests

### Deployment YAML Explained
```yaml
# k8s/services/catalog-service.yaml - LINE BY LINE

apiVersion: apps/v1              # API version for Deployment resource
kind: Deployment                 # Type of resource
metadata:
  name: catalog-service          # Name of the deployment
  namespace: eshopflix           # Namespace it belongs to
  labels:                        # Labels for organizing/selecting
    app: catalog-service
spec:
  replicas: 1                    # Number of pod copies to run
  selector:                      # How deployment finds its pods
    matchLabels:
      app: catalog-service
  template:                      # Pod template
    metadata:
      labels:
        app: catalog-service     # Must match selector
    spec:
      containers:
      - name: catalog-service
        image: eshopflix/catalog-service:latest    # Container image
        ports:
        - containerPort: 8080    # Port the container listens on
        env:                     # Environment variables
        - name: ASPNETCORE_URLS
          value: "http://+:8080"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:             # Get value from secret
            secretKeyRef:
              name: db-secrets
              key: catalog-db
        resources:               # Resource requests and limits
          requests:              # Minimum resources needed
            memory: "64Mi"
            cpu: "50m"
          limits:                # Maximum resources allowed
            memory: "256Mi"
            cpu: "250m"

---
apiVersion: v1
kind: Service                    # Kubernetes Service
metadata:
  name: catalog-service
  namespace: eshopflix
spec:
  type: ClusterIP                # Internal only (default)
  ports:
  - port: 80                     # Service port
    targetPort: 8080             # Container port
  selector:                      # Which pods to route to
    app: catalog-service
```

---

## Day 5: Deploy Full Application to Local K8s

### Exercise: Deploy eShopFlix
```powershell
# Apply the all-in-one configuration
kubectl apply -f k8s/dev/secrets.yaml
kubectl apply -f k8s/dev/all-in-one.yaml

# Watch pods come up
kubectl get pods -n eshopflix -w

# Check all resources
kubectl get all -n eshopflix

# Get external IP (for Docker Desktop, use port-forward)
kubectl port-forward service/web-frontend 5010:80 -n eshopflix

# Access at http://localhost:5010
```

### Essential kubectl Commands
```powershell
# === VIEWING RESOURCES ===
kubectl get pods -n eshopflix                    # List pods
kubectl get pods -o wide -n eshopflix            # More details
kubectl get all -n eshopflix                     # All resources
kubectl describe pod <pod-name> -n eshopflix     # Detailed info

# === LOGS ===
kubectl logs <pod-name> -n eshopflix             # View logs
kubectl logs -f <pod-name> -n eshopflix          # Follow logs
kubectl logs --previous <pod-name> -n eshopflix  # Previous container logs

# === DEBUGGING ===
kubectl exec -it <pod-name> -n eshopflix -- /bin/sh   # Shell into pod
kubectl port-forward pod/<pod-name> 8080:8080 -n eshopflix  # Port forward

# === SCALING ===
kubectl scale deployment catalog-service --replicas=3 -n eshopflix

# === UPDATING ===
kubectl set image deployment/catalog-service catalog-service=eshopflix/catalog-service:v2 -n eshopflix
kubectl rollout status deployment/catalog-service -n eshopflix
kubectl rollout undo deployment/catalog-service -n eshopflix  # Rollback

# === CLEANUP ===
kubectl delete -f k8s/dev/all-in-one.yaml
kubectl delete namespace eshopflix
```

### ?? Week 3 Interview Questions
```
Q: How does Kubernetes handle a pod failure?
A: "When a pod fails, the Deployment's ReplicaSet notices and creates 
   a new pod to maintain the desired replica count. This is self-healing. 
   The Service automatically routes traffic only to healthy pods."

Q: What is the difference between ClusterIP, NodePort, and LoadBalancer?
A: "ClusterIP is internal-only access. NodePort exposes on each node's IP. 
   LoadBalancer provisions an external load balancer in cloud environments. 
   I use ClusterIP for service-to-service communication and LoadBalancer 
   or Ingress for external access."

Q: How do you do zero-downtime deployments?
A: "Kubernetes uses rolling updates by default. It gradually replaces old 
   pods with new ones, ensuring minimum availability. I configure 
   maxSurge and maxUnavailable to control the rollout speed."

Q: What is a ConfigMap vs Secret?
A: "ConfigMaps store non-sensitive configuration data. Secrets store 
   sensitive data like passwords and API keys. Secrets are base64 
   encoded and can be encrypted at rest. Both can be mounted as 
   files or environment variables."
```

---

# ?? WEEK 4: Azure Fundamentals

## Day 1: Azure Account Setup

### Create Free Azure Account
```
1. Go to https://azure.microsoft.com/free/
2. Sign up for free account ($200 credit for 30 days)
3. Or use Pay-As-You-Go (you control spending)
```

### Install Azure CLI
```powershell
# Install via winget
winget install Microsoft.AzureCLI

# Login
az login

# Verify
az account show
```

---

## Day 2: Azure Container Registry (ACR)

### What is ACR?
```
Q: What is Azure Container Registry?
A: "ACR is Azure's private Docker registry service. It stores and manages 
   container images securely. It integrates with Azure services like AKS 
   for seamless image pulling without managing credentials."
```

### Exercise: Create and Use ACR
```powershell
# Set variables
$resourceGroup = "eshopflix-rg"
$acrName = "eshopflixacr$(Get-Random -Maximum 999)"  # Must be globally unique
$location = "eastus"

# Create resource group
az group create --name $resourceGroup --location $location

# Create ACR (Basic tier = $5/month)
az acr create --resource-group $resourceGroup --name $acrName --sku Basic

# Login to ACR
az acr login --name $acrName

# Tag your local image for ACR
docker tag eshopflix/catalog-service:v1 "$acrName.azurecr.io/eshopflix/catalog-service:v1"

# Push to ACR
docker push "$acrName.azurecr.io/eshopflix/catalog-service:v1"

# Verify it's in ACR
az acr repository list --name $acrName

# View image tags
az acr repository show-tags --name $acrName --repository eshopflix/catalog-service
```

### ?? ACR Interview Points
```
Q: Why use ACR instead of Docker Hub?
A: "ACR provides: 1) Private images by default, 2) Geo-replication for 
   faster pulls worldwide, 3) Integrated security scanning, 4) Seamless 
   integration with Azure services, 5) Managed identity authentication 
   with AKS."
```

---

## Day 3: Azure Kubernetes Service (AKS)

### What is AKS?
```
Q: What is AKS?
A: "AKS is Azure's managed Kubernetes service. Azure handles the control 
   plane (API server, scheduler, etc.) for free. You only pay for worker 
   nodes. It simplifies Kubernetes operations with built-in monitoring, 
   scaling, and upgrades."
```

### Exercise: Create AKS Cluster
```powershell
# Create AKS cluster (Budget: 1 node B2s)
az aks create `
    --resource-group $resourceGroup `
    --name eshopflix-aks `
    --node-count 1 `
    --node-vm-size Standard_B2s `
    --enable-managed-identity `
    --attach-acr $acrName `
    --generate-ssh-keys

# Get credentials to use kubectl
az aks get-credentials --resource-group $resourceGroup --name eshopflix-aks

# Verify connection
kubectl get nodes

# Check cluster info
az aks show --resource-group $resourceGroup --name eshopflix-aks --output table
```

### ?? AKS Interview Points
```
Q: What is managed identity in AKS?
A: "Managed identity is Azure's way to authenticate services without 
   storing credentials. AKS uses it to pull images from ACR, access 
   Key Vault, and interact with other Azure services securely."

Q: How do you scale AKS?
A: "I can scale manually: 'az aks scale --node-count 3'. Or enable 
   cluster autoscaler to automatically adjust nodes based on pod 
   resource requests. I also use Horizontal Pod Autoscaler to scale 
   pods based on CPU/memory usage."
```

---

## Day 4: Azure SQL Serverless

### What is Azure SQL Serverless?
```
Q: What is Azure SQL Serverless?
A: "Serverless is a compute tier that automatically pauses when idle 
   and resumes when accessed. You pay only for compute used. Perfect 
   for development and variable workloads. It can save 50-70% compared 
   to provisioned compute."
```

### Exercise: Create Azure SQL
```powershell
$sqlServerName = "eshopflix-sql-$(Get-Random -Maximum 999)"
$sqlAdmin = "sqladmin"
$sqlPassword = "P@ssw0rd$(Get-Random -Maximum 9999)!"

# Create SQL Server
az sql server create `
    --resource-group $resourceGroup `
    --name $sqlServerName `
    --admin-user $sqlAdmin `
    --admin-password $sqlPassword

# Allow Azure services to access
az sql server firewall-rule create `
    --resource-group $resourceGroup `
    --server $sqlServerName `
    --name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

# Create database (Serverless tier)
az sql db create `
    --resource-group $resourceGroup `
    --server $sqlServerName `
    --name CatalogDb `
    --edition GeneralPurpose `
    --compute-model Serverless `
    --family Gen5 `
    --capacity 1 `
    --auto-pause-delay 60 `
    --min-capacity 0.5

# Get connection string
Write-Host "Server: $sqlServerName.database.windows.net"
Write-Host "User: $sqlAdmin"
Write-Host "Password: $sqlPassword"
```

---

## Day 5: Connect Everything

### Exercise: Deploy to AKS with Azure SQL
```powershell
# Update k8s/dev/secrets.yaml with Azure SQL connection string
# Connection format:
# Server=tcp:YOUR_SERVER.database.windows.net,1433;Database=CatalogDb;User ID=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;

# Apply secrets
kubectl apply -f k8s/dev/secrets.yaml

# Deploy application
$content = Get-Content "k8s/dev/all-in-one.yaml" -Raw
$content = $content -replace '\$\{ACR_NAME\}', $acrName
$content | kubectl apply -f -

# Check deployment
kubectl get pods -n eshopflix -w

# Get external IP
kubectl get ingress -n eshopflix
```

---

# ?? WEEK 5: Production Deployment & Monitoring

## Day 1: Install NGINX Ingress Controller

```powershell
# Install NGINX Ingress Controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.9.4/deploy/static/provider/cloud/deploy.yaml

# Wait for external IP
kubectl get service ingress-nginx-controller -n ingress-nginx -w

# Once you have an IP, your app is accessible!
```

---

## Day 2: Monitoring with kubectl

### Essential Monitoring Commands
```powershell
# Real-time pod monitoring
kubectl get pods -n eshopflix -w

# Resource usage
kubectl top pods -n eshopflix
kubectl top nodes

# Events (for debugging)
kubectl get events -n eshopflix --sort-by='.lastTimestamp'

# Describe for troubleshooting
kubectl describe pod <pod-name> -n eshopflix
```

---

## Day 3: Cost Management

### View Azure Costs
```powershell
# View current costs
az consumption usage list --subscription <sub-id> --query "[].{Name:name, Cost:pretaxCost}"

# Set budget alert
az consumption budget create --budget-name monthly-limit --amount 100 --time-grain Monthly
```

### Cost Saving Commands
```powershell
# STOP AKS (saves ~$1/day)
az aks stop --name eshopflix-aks --resource-group $resourceGroup

# START AKS
az aks start --name eshopflix-aks --resource-group $resourceGroup

# Check status
az aks show --name eshopflix-aks --resource-group $resourceGroup --query powerState
```

---

## Day 4-5: Full Deployment Exercise

### Complete Deployment Script
```powershell
# Run the complete deployment
.\scripts\deploy-dev.ps1 -SetupInfra   # Creates everything
.\scripts\build-images.ps1 -All         # Builds and pushes images
.\scripts\deploy-dev.ps1 -Deploy        # Deploys to AKS
.\scripts\deploy-dev.ps1 -Status        # Shows status
```

---

# ?? WEEK 6: Interview Preparation

## Key Topics to Master

### 1. Docker Questions
```
Q: Walk me through how you containerized your microservices.
A: "I created multi-stage Dockerfiles for each service. The first stage 
   uses the .NET SDK to restore, build, and publish. The final stage 
   uses the runtime-only image for a smaller footprint. I use Docker 
   Compose for local development with all services and SQL Server."

Q: How do you optimize Docker images?
A: "I use multi-stage builds, Alpine-based images when possible, combine 
   RUN commands to reduce layers, use .dockerignore to exclude 
   unnecessary files, and order commands to maximize cache usage."
```

### 2. Kubernetes Questions
```
Q: Explain your Kubernetes architecture.
A: "I have 8 services deployed as Deployments with Services for internal 
   communication. I use ConfigMaps for configuration and Secrets for 
   sensitive data. NGINX Ingress Controller handles external routing. 
   Each service has resource limits to prevent resource contention."

Q: How do you handle rolling updates?
A: "I use Deployment's rolling update strategy. New pods are created 
   before old ones are terminated. I configure readiness probes so 
   traffic only goes to ready pods. If something goes wrong, I can 
   quickly rollback with 'kubectl rollout undo'."

Q: How do you troubleshoot a failing pod?
A: "First, I check pod status with 'kubectl get pods'. Then 'kubectl 
   describe pod' for events. 'kubectl logs' for application logs. If 
   needed, 'kubectl exec' to get inside the container. I check resource 
   limits, image availability, and secret mounting."
```

### 3. Azure Questions
```
Q: Why Azure for this project?
A: "Azure provides a managed Kubernetes service (AKS) where I don't 
   manage the control plane. ACR integrates seamlessly for private 
   images. Azure SQL Serverless reduces costs with auto-pause. 
   Managed identities simplify security."

Q: How do you manage costs?
A: "I use the smallest viable VM size (B2s). Azure SQL Serverless 
   auto-pauses when idle. I stop AKS when not in use. I set up budget 
   alerts. For production, I'd consider reserved instances for 
   predictable workloads."
```

---

## Demo Script for Interviews

### Prepare a 5-Minute Demo
```powershell
# 1. Show local development (30 sec)
docker-compose up -d
Start-Process "http://localhost:5010"
"This runs all 8 services locally with Docker Compose"

# 2. Show AKS deployment (1 min)
kubectl get pods -n eshopflix
kubectl get services -n eshopflix
"Here's my production deployment with 8 microservices on AKS"

# 3. Show scaling (30 sec)
kubectl scale deployment catalog-service --replicas=3 -n eshopflix
kubectl get pods -n eshopflix -w
"Kubernetes automatically creates new pods for high availability"

# 4. Show logs/monitoring (30 sec)
kubectl logs -f deployment/catalog-service -n eshopflix
"I can monitor logs in real-time for debugging"

# 5. Show architecture diagram (1 min)
"Let me walk you through the architecture..."

# 6. Q&A (1-2 min)
```

---

## Your Learning Checklist

### Docker ?
- [ ] Build images with multi-stage Dockerfile
- [ ] Run containers with proper networking
- [ ] Use Docker Compose for multi-container apps
- [ ] Debug containers with exec and logs
- [ ] Optimize image size

### Kubernetes ?
- [ ] Deploy applications with Deployments
- [ ] Expose with Services
- [ ] Configure with ConfigMaps and Secrets
- [ ] External access with Ingress
- [ ] Scale pods manually and with HPA
- [ ] Rolling updates and rollbacks
- [ ] Troubleshoot with describe, logs, exec

### Azure ?
- [ ] Create and use ACR
- [ ] Deploy AKS cluster
- [ ] Configure Azure SQL
- [ ] Manage costs (start/stop, budget alerts)
- [ ] Use Azure CLI effectively

---

## Final Interview Tips

1. **Tell a Story**: "I built an e-commerce platform with 8 microservices..."

2. **Show Real Experience**: "I actually deployed this to Azure and learned..."

3. **Mention Challenges**: "One issue I faced was... I solved it by..."

4. **Know the Numbers**: "My Docker image is ~200MB, deployment takes ~2 minutes..."

5. **Have GitHub Ready**: Share your repo link with the interviewer

6. **Prepare for Deep Dives**: Be ready to explain any component in detail

---

**Good luck! You've got this! ??**
