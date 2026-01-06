# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and all project directories
COPY MechanicBuddy.sln .
COPY MechanicBuddy.Management.Api/ ./MechanicBuddy.Management.Api/

RUN dotnet restore MechanicBuddy.Management.Api/MechanicBuddy.Management.Api.csproj

# Publish Management API
RUN dotnet publish MechanicBuddy.Management.Api/MechanicBuddy.Management.Api.csproj \
    -c Release -o /app/publish

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
ENV ASPNETCORE_URLS=http://+:15568
WORKDIR /app
COPY --from=build /app/publish .

# Install Helm for tenant provisioning
RUN apt-get update && apt-get install -y \
    curl \
    ca-certificates \
    && curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash \
    && rm -rf /var/lib/apt/lists/*

# Copy Helm charts for tenant deployment
COPY ../../../infrastructure/helm/charts /app/charts

ENTRYPOINT ["dotnet", "MechanicBuddy.Management.Api.dll"]
