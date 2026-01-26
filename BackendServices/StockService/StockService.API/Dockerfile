# StockService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["BackendServices/StockService/StockService.API/StockService.API.csproj", "BackendServices/StockService/StockService.API/"]
COPY ["BackendServices/StockService/StockService.Application/StockService.Application.csproj", "BackendServices/StockService/StockService.Application/"]
COPY ["BackendServices/StockService/StockService.Domain/StockService.Domain.csproj", "BackendServices/StockService/StockService.Domain/"]
COPY ["BackendServices/StockService/StockService.Infrastructure/StockService.Infrastructure.csproj", "BackendServices/StockService/StockService.Infrastructure/"]
COPY ["eShopFlix.Shared/Contracts/Contracts.csproj", "eShopFlix.Shared/Contracts/"]

RUN dotnet restore "BackendServices/StockService/StockService.API/StockService.API.csproj"

# Copy source code
COPY BackendServices/StockService/ BackendServices/StockService/
COPY eShopFlix.Shared/ eShopFlix.Shared/

WORKDIR "/src/BackendServices/StockService/StockService.API"
RUN dotnet build "StockService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StockService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockService.API.dll"]
