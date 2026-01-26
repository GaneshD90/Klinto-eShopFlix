# CatalogService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["BackendServices/CatalogService/CatalogService.API/CatalogService.API.csproj", "BackendServices/CatalogService/CatalogService.API/"]
COPY ["BackendServices/CatalogService/CatalogService.Application/CatalogService.Application.csproj", "BackendServices/CatalogService/CatalogService.Application/"]
COPY ["BackendServices/CatalogService/CatalogService.Domain/CatalogService.Domain.csproj", "BackendServices/CatalogService/CatalogService.Domain/"]
COPY ["BackendServices/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj", "BackendServices/CatalogService/CatalogService.Infrastructure/"]
COPY ["eShopFlix.Shared/Contracts/Contracts.csproj", "eShopFlix.Shared/Contracts/"]

RUN dotnet restore "BackendServices/CatalogService/CatalogService.API/CatalogService.API.csproj"

# Copy source code
COPY BackendServices/CatalogService/ BackendServices/CatalogService/
COPY eShopFlix.Shared/ eShopFlix.Shared/

WORKDIR "/src/BackendServices/CatalogService/CatalogService.API"
RUN dotnet build "CatalogService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CatalogService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.API.dll"]
