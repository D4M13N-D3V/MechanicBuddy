# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore
COPY backend/src/DbUp/*.csproj ./DbUp/
RUN dotnet restore DbUp/DbUp.csproj

# Copy source and publish
COPY backend/src/DbUp/ ./DbUp/
RUN dotnet publish DbUp/DbUp.csproj -c Release -o /app/publish

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DbUp.dll"]
