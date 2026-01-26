# OcelotApiGateway Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["ApiGateways/OcelotApiGateway/OcelotApiGateway.csproj", "ApiGateways/OcelotApiGateway/"]

RUN dotnet restore "ApiGateways/OcelotApiGateway/OcelotApiGateway.csproj"

# Copy source code
COPY ApiGateways/OcelotApiGateway/ ApiGateways/OcelotApiGateway/

WORKDIR "/src/ApiGateways/OcelotApiGateway"
RUN dotnet build "OcelotApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OcelotApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OcelotApiGateway.dll"]
