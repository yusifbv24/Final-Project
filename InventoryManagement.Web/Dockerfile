# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InventoryManagement.Web/InventoryManagement.Web.csproj", "InventoryManagement.Web/"]
COPY ["InventoryService.Application/InventoryService.Application.csproj", "InventoryService.Application/"]
COPY ["InventoryService.Domain/InventoryService.Domain.csproj", "InventoryService.Domain/"]
COPY ["OrderService.Application/OrderService.Application.csproj", "OrderService.Application/"]
COPY ["OrderService.Domain/OrderService.Domain.csproj", "OrderService.Domain/"]
COPY ["ProductService.Application/ProductService.Application.csproj", "ProductService.Application/"]
COPY ["ProductService.Domain/ProductService.Domain.csproj", "ProductService.Domain/"]
RUN dotnet restore "./InventoryManagement.Web/InventoryManagement.Web.csproj"
COPY . .
WORKDIR "/src/InventoryManagement.Web"
RUN dotnet build "./InventoryManagement.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InventoryManagement.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InventoryManagement.Web.dll"]