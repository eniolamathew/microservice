# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 1010 1011

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. First copy just project files for better layer caching
COPY ["DatabaseMigrationLib/DatabaseMigrationLib.csproj", "DatabaseMigrationLib/"]
COPY ["DbMigrationRunner/DbMigrationRunner.csproj", "DbMigrationRunner/"]
COPY ["DbMigrationRunner/Scripts/BrandService", "/staging/Scripts/BrandService"]
COPY ["DbMigrationRunner/Scripts/PlatformService", "/staging/Scripts/PlatformService"]

# 2. Restore dependencies
RUN dotnet restore "./DbMigrationRunner/DbMigrationRunner.csproj"

# 3. Copy remaining source
COPY DbMigrationRunner/ DbMigrationRunner/
COPY DatabaseMigrationLib/ DatabaseMigrationLib/
COPY Microservice.DataAccess/ Microservice.DataAccess/

# 4. Build
WORKDIR "/src/DbMigrationRunner"
RUN dotnet build "./DbMigrationRunner.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DbMigrationRunner.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Copy scripts from the build context (correct path)
COPY --from=build /staging/Scripts /app/Scripts

# Verify directory structure (optional, for debugging)
RUN ls -la /app/Scripts && \
    ls -la /app/Scripts/PlatformService && \
    ls -la /app/Scripts/BrandService

ENTRYPOINT ["dotnet", "DbMigrationRunner.dll"]