# Build stage
FROM mcr.microsoft.com/dotnet/sdk:Framework AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY src/Company.WebApplication1.Api/*.csproj ./Company.WebApplication1.Api/
COPY src/Company.WebApplication1.ServiceDefaults/*.csproj ./Company.WebApplication1.ServiceDefaults/

# Restore dependencies
RUN dotnet restore Company.WebApplication1.Api/Company.WebApplication1.Api.csproj

# Copy source code
COPY src/Company.WebApplication1.Api/ ./Company.WebApplication1.Api/
COPY src/Company.WebApplication1.ServiceDefaults/ ./Company.WebApplication1.ServiceDefaults/

# Build and publish the API project only (not AppHost)
RUN dotnet publish Company.WebApplication1.Api/Company.WebApplication1.Api.csproj \
    -c Release \
    -o /app/publish \
    -p:PublishReadyToRun=true \
    -p:PublishSingleFile=true \
    -p:InvariantGlobalization=true \
    -p:TieredPGO=true \
    --self-contained=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:Framework AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser -u 10001 appuser

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:Port \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_USE_POLLING_FILE_WATCHER=true

# Copy published application
COPY --from=build /app/publish .

# Change ownership to appuser
RUN chown -R appuser:appuser /app

# Health check for container orchestrators
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:Port/health || exit 1

EXPOSE Port

# Switch to non-root user
USER appuser

# Start the application
ENTRYPOINT ["dotnet", "Company.WebApplication1.Api.dll"]