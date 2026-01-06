FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/src/MechanicBuddy.sln .
COPY backend/src/. .

RUN dotnet restore MechanicBuddy.sln
RUN dotnet publish DbUp/DbUp.csproj -c Release -o /dbup

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /dbup .
ENTRYPOINT ["dotnet","DbUp.dll"]
