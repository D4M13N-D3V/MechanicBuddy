# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project directories
COPY MechanicBuddy.sln .
COPY MechanicBuddy.Management.DbUp/ ./MechanicBuddy.Management.DbUp/

RUN dotnet restore MechanicBuddy.Management.DbUp/MechanicBuddy.Management.DbUp.csproj

# Publish DbUp
RUN dotnet publish MechanicBuddy.Management.DbUp/MechanicBuddy.Management.DbUp.csproj \
    -c Release -o /app/publish

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MechanicBuddy.Management.DbUp.dll"]
