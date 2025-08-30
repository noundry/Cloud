# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Company.WebApplication1.Api/Company.WebApplication1.Api.csproj", "src/Company.WebApplication1.Api/"]
RUN dotnet restore "src/Company.WebApplication1.Api/Company.WebApplication1.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/Company.WebApplication1.Api"
RUN dotnet build "Company.WebApplication1.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Company.WebApplication1.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Create directories with proper permissions
RUN mkdir -p /app/storage /var/log && chown -R appuser:appgroup /app /var/log

# Copy published application
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Company.WebApplication1.Api.dll"]