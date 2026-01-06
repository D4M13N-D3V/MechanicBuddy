# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all source files (needed for project references)
COPY backend/src/ ./

# Restore and publish
RUN dotnet restore DbUp/DbUp.csproj
RUN dotnet publish DbUp/DbUp.csproj -c Release -o /app/publish --no-restore

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DbUp.dll"]
