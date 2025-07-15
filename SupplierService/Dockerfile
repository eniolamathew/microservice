# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7000
EXPOSE 7001


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SupplierService/SupplierService.csproj", "SupplierService/"]
COPY ["Microservice.DataAccess/Microservice.DataAccess.csproj", "Microservice.DataAccess/"]
COPY ["MicroServices.API/MicroServices.API.csproj", "MicroServices.API/"]
COPY ["MicroServices.Caching/MicroServices.Caching.csproj", "MicroServices.Caching/"]
COPY ["DatabaseMigrationLib/DatabaseMigrationLib.csproj", "DatabaseMigrationLib/"]
RUN dotnet restore "./SupplierService/SupplierService.csproj"
COPY . .
WORKDIR "/src/SupplierService"
RUN dotnet build "./SupplierService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SupplierService.csproj" -c $BUILD_CONFIGURATION -o /app/supplierservice_publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/supplierservice_publish .
ENTRYPOINT ["dotnet", "SupplierService.dll"]