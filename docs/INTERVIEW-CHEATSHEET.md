# Interview Cheat Sheet - Docker, Kubernetes & Azure
## Quick Reference for eShopFlix Microservices Project

---

## ?? DOCKER

### Key Commands
```bash
# Build
docker build -t image:tag -f Dockerfile .

# Run
docker run -d -p 8080:80 --name container image:tag

# Debug
docker logs container
docker exec -it container /bin/sh

# Clean
docker system prune -a
```

### Interview Answers
| Question | Answer |
|----------|--------|
| What is Docker? | Containerization platform that packages apps with dependencies for consistent execution anywhere |
| Image vs Container? | Image = template (read-only), Container = running instance |
| Multi-stage builds? | Separate build and runtime stages to reduce final image size (700MB ? 200MB) |
| How handle secrets? | Never in Dockerfile. Use env vars, Docker secrets, or Key Vault |

---

## ?? KUBERNETES

### Key Commands
```bash
# View
kubectl get pods/services/deployments -n namespace
kubectl describe pod <name> -n namespace
kubectl logs <pod> -n namespace

# Debug
kubectl exec -it <pod> -n namespace -- /bin/sh
kubectl port-forward pod/<name> 8080:80 -n namespace

# Scale
kubectl scale deployment <name> --replicas=3 -n namespace

# Update
kubectl rollout restart deployment/<name> -n namespace
kubectl rollout undo deployment/<name> -n namespace
```

### Core Concepts
```
Cluster ? Node ? Pod ? Container

Pod        = Smallest unit, 1+ containers, ephemeral
Deployment = Manages pods (replicas, updates, rollbacks)
Service    = Stable network endpoint (ClusterIP/LoadBalancer)
Ingress    = External HTTP routing + SSL
ConfigMap  = Non-sensitive config
Secret     = Sensitive data (base64, can encrypt)
```

### Interview Answers
| Question | Answer |
|----------|--------|
| Pod failure? | Deployment creates new pod automatically (self-healing) |
| ClusterIP vs LoadBalancer? | ClusterIP = internal only, LoadBalancer = external access |
| Zero-downtime deploy? | Rolling updates - new pods up before old pods down |
| ConfigMap vs Secret? | ConfigMap for config, Secret for passwords (base64 encoded) |

---

## ?? AZURE

### Key Commands
```bash
# Login
az login

# ACR
az acr create --name NAME --sku Basic
az acr login --name NAME
docker push NAME.azurecr.io/image:tag

# AKS
az aks create --name NAME --node-count 1 --attach-acr ACR_NAME
az aks get-credentials --name NAME
az aks stop/start --name NAME  # Save money!

# SQL
az sql server create --name NAME --admin-user USER --admin-password PASS
az sql db create --server NAME --name DB --compute-model Serverless
```

### Interview Answers
| Question | Answer |
|----------|--------|
| Why ACR? | Private registry, integrated with AKS, security scanning, managed identity |
| Why AKS? | Managed K8s - Azure handles control plane for free, only pay for nodes |
| SQL Serverless? | Auto-pauses when idle, saves 50-70% for dev/variable workloads |
| Cost management? | Stop AKS when idle, SQL auto-pauses, budget alerts, B2s for dev |

---

## ?? MY PROJECT METRICS

| Metric | Value |
|--------|-------|
| Microservices | 8 (Catalog, Auth, Cart, Payment, Order, Stock, Gateway, Web) |
| Docker Image Size | ~200MB each (multi-stage) |
| K8s Resources | 8 Deployments, 8 Services, 1 Ingress |
| Azure Cost | ~$55-75/month (budget config) |
| Deployment Time | ~5 minutes |

---

## ?? INTERVIEW STORY

> "I built an e-commerce platform called eShopFlix with 8 microservices 
> using .NET 9. Each service follows clean architecture with Domain, 
> Application, Infrastructure, and API layers.
>
> For containerization, I created multi-stage Dockerfiles to optimize 
> image size. Docker Compose handles local development with all services 
> and SQL Server.
>
> For production, I deployed to Azure Kubernetes Service. I use Azure 
> Container Registry for private images, Azure SQL Serverless to save 
> costs, and NGINX Ingress for routing.
>
> One challenge I solved was managing costs - I configured SQL to 
> auto-pause and stop AKS when not in use, keeping monthly costs 
> around $55-75."

---

## ?? TROUBLESHOOTING

| Problem | Solution |
|---------|----------|
| Pod CrashLoopBackOff | `kubectl describe pod` ? check events, `kubectl logs` |
| Image pull error | Check ACR name, `az acr login`, AKS-ACR attachment |
| Service not reachable | Check selectors match labels, port mapping |
| SQL connection fails | Check firewall, connection string, SQL may need to wake up |

---

## ?? KEY FILES

```
Dockerfiles:    BackendServices/*/API/Dockerfile
Compose:        docker-compose.yml
K8s Dev:        k8s/dev/all-in-one.yaml
K8s Secrets:    k8s/dev/secrets.yaml
Deploy Script:  scripts/deploy-dev.ps1
```

---

## ?? PRO TIPS FOR INTERVIEW

1. **Know your numbers** - image sizes, pod counts, costs
2. **Mention challenges** - "I faced X, solved it by Y"
3. **Show GitHub** - clean repo with good README
4. **Have demo ready** - 5-min walkthrough
5. **Admit unknowns** - "I haven't used X, but I'd approach it by..."

**Good luck! ??**
