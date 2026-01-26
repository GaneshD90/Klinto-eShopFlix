# OrderService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["BackendServices/OrderService/OrderService.API/OrderService.API.csproj", "BackendServices/OrderService/OrderService.API/"]
COPY ["BackendServices/OrderService/OrderService.Application/OrderService.Application.csproj", "BackendServices/OrderService/OrderService.Application/"]
COPY ["BackendServices/OrderService/OrderService.Domain/OrderService.Domain.csproj", "BackendServices/OrderService/OrderService.Domain/"]
COPY ["BackendServices/OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "BackendServices/OrderService/OrderService.Infrastructure/"]
COPY ["eShopFlix.Shared/Contracts/Contracts.csproj", "eShopFlix.Shared/Contracts/"]

RUN dotnet restore "BackendServices/OrderService/OrderService.API/OrderService.API.csproj"

# Copy source code
COPY BackendServices/OrderService/ BackendServices/OrderService/
COPY eShopFlix.Shared/ eShopFlix.Shared/

WORKDIR "/src/BackendServices/OrderService/OrderService.API"
RUN dotnet build "OrderService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrderService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.API.dll"]
