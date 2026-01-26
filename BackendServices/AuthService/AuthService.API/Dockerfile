# AuthService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["BackendServices/AuthService/AuthService.API/AuthService.API.csproj", "BackendServices/AuthService/AuthService.API/"]
COPY ["BackendServices/AuthService/AuthService.Application/AuthService.Application.csproj", "BackendServices/AuthService/AuthService.Application/"]
COPY ["BackendServices/AuthService/AuthService.Domain/AuthService.Domain.csproj", "BackendServices/AuthService/AuthService.Domain/"]
COPY ["BackendServices/AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj", "BackendServices/AuthService/AuthService.Infrastructure/"]
COPY ["eShopFlix.Shared/Contracts/Contracts.csproj", "eShopFlix.Shared/Contracts/"]

RUN dotnet restore "BackendServices/AuthService/AuthService.API/AuthService.API.csproj"

# Copy source code
COPY BackendServices/AuthService/ BackendServices/AuthService/
COPY eShopFlix.Shared/ eShopFlix.Shared/

WORKDIR "/src/BackendServices/AuthService/AuthService.API"
RUN dotnet build "AuthService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuthService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
