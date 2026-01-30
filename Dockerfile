# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY global.json .
COPY LabelNesting.slnx .

# Copy all project files
COPY src/Shunty.LabelNesting.Core/*.csproj ./src/Shunty.LabelNesting.Core/
COPY src/Shunty.LabelNesting.ServiceDefaults/*.csproj ./src/Shunty.LabelNesting.ServiceDefaults/
COPY src/Shunty.LabelNesting.Web/*.csproj ./src/Shunty.LabelNesting.Web/

# Restore dependencies
RUN dotnet restore src/Shunty.LabelNesting.Web/Shunty.LabelNesting.Web.csproj

# Copy the rest of the source code
COPY src/ ./src/

# Build and publish
WORKDIR /src/src/Shunty.LabelNesting.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create a non-root user with UID that won't conflict with base image
RUN useradd -m -u 10001 appuser && chown -R appuser:appuser /app
USER appuser

# Copy published app
COPY --from=build --chown=appuser:appuser /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8443

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Shunty.LabelNesting.Web.dll"]
