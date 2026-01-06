# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all source files (needed for project references)
COPY backend/src/ ./

# Restore and publish
RUN dotnet restore MechanicBuddy.Management.Api/MechanicBuddy.Management.Api.csproj
RUN dotnet publish MechanicBuddy.Management.Api/MechanicBuddy.Management.Api.csproj \
    -c Release -o /app/publish --no-restore

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

# Copy Helm charts for tenant deployment (if exists)
COPY infrastructure/helm/charts /app/charts

ENTRYPOINT ["dotnet", "MechanicBuddy.Management.Api.dll"]
