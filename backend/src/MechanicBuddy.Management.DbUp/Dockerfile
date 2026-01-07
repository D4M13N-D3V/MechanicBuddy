# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy all source files (needed for project references)
COPY backend/src/ ./

# Restore and publish
RUN dotnet restore MechanicBuddy.Management.DbUp/MechanicBuddy.Management.DbUp.csproj
RUN dotnet publish MechanicBuddy.Management.DbUp/MechanicBuddy.Management.DbUp.csproj \
    -c Release -o /app/publish --no-restore

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MechanicBuddy.Management.DbUp.dll"]
