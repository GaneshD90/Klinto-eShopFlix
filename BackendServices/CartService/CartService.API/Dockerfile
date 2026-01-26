# CartService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["BackendServices/CartService/CartService.API/CartService.API.csproj", "BackendServices/CartService/CartService.API/"]
COPY ["BackendServices/CartService/CartService.Application/CartService.Application.csproj", "BackendServices/CartService/CartService.Application/"]
COPY ["BackendServices/CartService/CartService.Domain/CartService.Domain.csproj", "BackendServices/CartService/CartService.Domain/"]
COPY ["BackendServices/CartService/CartService.Infrastructure/CartService.Infrastructure.csproj", "BackendServices/CartService/CartService.Infrastructure/"]
COPY ["eShopFlix.Shared/Contracts/Contracts.csproj", "eShopFlix.Shared/Contracts/"]

RUN dotnet restore "BackendServices/CartService/CartService.API/CartService.API.csproj"

# Copy source code
COPY BackendServices/CartService/ BackendServices/CartService/
COPY eShopFlix.Shared/ eShopFlix.Shared/

WORKDIR "/src/BackendServices/CartService/CartService.API"
RUN dotnet build "CartService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CartService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CartService.API.dll"]
