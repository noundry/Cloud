# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY src/MyApp.Api/*.csproj ./MyApp.Api/
COPY src/MyApp.ServiceDefaults/*.csproj ./MyApp.ServiceDefaults/

# Restore dependencies
RUN dotnet restore MyApp.Api/MyApp.Api.csproj

# Copy source code
COPY src/MyApp.Api/ ./MyApp.Api/
COPY src/MyApp.ServiceDefaults/ ./MyApp.ServiceDefaults/

# Build and publish the API project only (not AppHost)
RUN dotnet publish MyApp.Api/MyApp.Api.csproj \
    -c Release \
    -o /app/publish \
    -p:PublishReadyToRun=true \
    -p:PublishSingleFile=true \
    -p:InvariantGlobalization=true \
    -p:TieredPGO=true \
    --self-contained=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser -u 10001 appuser

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0

# Copy published application
COPY --from=build /app/publish .

# Change ownership to appuser
RUN chown -R appuser:appuser /app

# Health check for container orchestrators
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080

# Switch to non-root user
USER appuser

# Start the application
ENTRYPOINT ["dotnet", "MyApp.Api.dll"]