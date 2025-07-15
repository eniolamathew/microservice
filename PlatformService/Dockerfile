# See https://aka.ms/customizecontainer to learn how to customize your debug container 
# and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only needed csproj files and restore
COPY ["PlatformService/PlatformService.csproj", "PlatformService/"]
COPY ["Microservice.DataAccess/Microservice.DataAccess.csproj", "Microservice.DataAccess/"]
COPY ["MicroServices.API/MicroServices.API.csproj", "MicroServices.API/"]
COPY ["MicroServices.Caching/MicroServices.Caching.csproj", "MicroServices.Caching/"]
COPY ["DatabaseMigrationLib/DatabaseMigrationLib.csproj", "DatabaseMigrationLib/"]

RUN dotnet restore "./PlatformService/PlatformService.csproj"

# Copy only the required project folders
COPY PlatformService/ PlatformService/
COPY Microservice.DataAccess/ Microservice.DataAccess/
COPY MicroServices.API/ MicroServices.API/
COPY MicroServices.Caching/ MicroServices.Caching/
COPY DatabaseMigrationLib/ DatabaseMigrationLib/

WORKDIR "/src/PlatformService"
RUN dotnet build "./PlatformService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PlatformService.csproj" -c $BUILD_CONFIGURATION -o /app/platformservice_publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/platformservice_publish .
ENTRYPOINT ["dotnet", "PlatformService.dll"]
