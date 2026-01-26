# PaymentService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["BackendServices/PaymentService/PaymentService.API/PaymentService.API.csproj", "BackendServices/PaymentService/PaymentService.API/"]
COPY ["BackendServices/PaymentService/PaymentService.Application/PaymentService.Application.csproj", "BackendServices/PaymentService/PaymentService.Application/"]
COPY ["BackendServices/PaymentService/PaymentService.Domain/PaymentService.Domain.csproj", "BackendServices/PaymentService/PaymentService.Domain/"]
COPY ["BackendServices/PaymentService/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "BackendServices/PaymentService/PaymentService.Infrastructure/"]
COPY ["eShopFlix.Shared/Contracts/Contracts.csproj", "eShopFlix.Shared/Contracts/"]

RUN dotnet restore "BackendServices/PaymentService/PaymentService.API/PaymentService.API.csproj"

# Copy source code
COPY BackendServices/PaymentService/ BackendServices/PaymentService/
COPY eShopFlix.Shared/ eShopFlix.Shared/

WORKDIR "/src/BackendServices/PaymentService/PaymentService.API"
RUN dotnet build "PaymentService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
