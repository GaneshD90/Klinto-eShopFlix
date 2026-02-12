# Klinto eShopFlix - Microservices E-Commerce Platform

.NET 9, container-first microservices reference for an e-commerce store. The solution includes a Razor Pages frontend, Ocelot API gateway, domain-focused backend services, shared contracts, and Kubernetes/Docker tooling for local or Azure AKS deployment.

## Architecture
- **Frontend**: `FrontendServices/eShopFlix.Web` (Razor Pages) + `eShopFlix.Support` helpers
- **API Gateway**: `ApiGateways/OcelotApiGateway` for routing/aggregation
- **Backend services** (SQL Server per service):
  - `CatalogService` – product catalog, search, stored procedures, promotions
  - `AuthService` – authentication/identity flows
  - `CartService` – cart domain, event sourcing, outbox/inbox, Azure Service Bus publisher
  - `OrderService` – order workflow, fraud checks
  - `PaymentService` – payment flows (e.g., RazorPay DTOs)
  - `StockService` – inventory, reservations, alerts; MassTransit with Azure Service Bus fallback to in-memory
- **Shared**: `eShopFlix.Shared/Contracts` for integration events and resilience primitives

## Key capabilities
- .NET 9, ASP.NET Core APIs + Razor Pages
- CQRS across services; outbox/inbox patterns for reliability
- Messaging via Azure Service Bus (MassTransit or native publisher) with logging fallback
- Resilience: Polly policies, circuit breakers, retries, bulkhead + OpenTelemetry tracing
- Observability: Serilog structured logging, health checks (`/health/live`, `/health/ready`), Swagger/OpenAPI per service
- Containerized: Dockerfiles for every service + Ocelot + frontend
- Kubernetes ready: `k8s/` manifests (dev/all-in-one, base, services, ingress, azure)

## Repository layout
- `BackendServices/<Service>` – API, Application, Domain, Infrastructure projects for each service
- `FrontendServices/eShopFlix.Web` – customer + admin UI (Razor Pages/MVC areas)
- `FrontendServices/eShopFlix.Support` – shared web helpers
- `ApiGateways/OcelotApiGateway` – gateway + `ocelot-docker.json`
- `eShopFlix.Shared/Contracts` – shared contracts/events/resilience primitives
- `scripts/` – `docker-local.ps1`, `build-images.ps1`, `deploy-dev.ps1`, `deploy-k8s.ps1`
- `k8s/` – dev (all-in-one + secrets template), base, services, ingress, azure overlays
- `docs/DEPLOYMENT.md` – detailed AKS and budget guidance

## Prerequisites
- .NET 9 SDK
- Docker Desktop
- SQL Server (local or Azure SQL Serverless if running services individually)
- For cloud: Azure CLI + kubectl (see `docs/DEPLOYMENT.md`)

## Run locally (Docker)
```powershell
# Build and start all services + SQL + gateway + frontend
./scripts/docker-local.ps1 -Up -Build

# Tail logs
./scripts/docker-local.ps1 -Logs

# Stop stack
./scripts/docker-local.ps1 -Down
```

Default endpoints:
| Service | URL |
| --- | --- |
| Web App | http://localhost:5010 |
| API Gateway | http://localhost:5000 |
| Catalog API | http://localhost:5001 |
| Auth API | http://localhost:5002 |
| Cart API | http://localhost:5003 |
| Payment API | http://localhost:5004 |
| Order API | http://localhost:5005 |
| Stock API | http://localhost:5006 |
| SQL Server | localhost:1433 |

## Run locally (without Docker)
```bash
dotnet restore
dotnet build

# Example: run a service
dotnet run --project BackendServices/CatalogService/CatalogService.API/CatalogService.API.csproj
```
Update `appsettings.Development.json` connection strings (per service) to point at your SQL instance. Each API exposes Swagger at `/swagger` and health checks at `/health/live` and `/health/ready`.

## Tests
```bash
dotnet test
```

## Deploy to Azure AKS (budget friendly)
See `docs/DEPLOYMENT.md` for full steps. Summary:
```powershell
# 1) Provision ACR, AKS (B2s), SQL serverless
./scripts/deploy-dev.ps1 -SetupInfra

# 2) Build & push images
./scripts/build-images.ps1 -All -Registry <acr>.azurecr.io

# 3) Deploy manifests (uses k8s/dev/all-in-one.yaml)
./scripts/deploy-dev.ps1 -Deploy

# 4) Check status / ingress IP
./scripts/deploy-dev.ps1 -Status
```
Stop the AKS cluster when idle to save cost: `az aks stop --name eshopflix-aks --resource-group eshopflix-rg`.

## Notes
- Outbox/inbox implementations exist in Cart and Catalog services for integration reliability.
- Stock service uses comprehensive resilience via Polly pipelines for downstream calls.
- Ocelot gateway + Dockerfiles enable single-command local spin-up.
