# Deployment Guide

This guide covers deploying the Label Nesting web application as a Docker container behind Caddy using caddy-docker-proxy for automatic configuration.

## Prerequisites

- Docker and Docker Compose installed on your remote host
- Caddy with caddy-docker-proxy running on the host
- A Docker Hub account
- GitHub repository configured with secrets
- DNS record configured (e.g., labels.shunty.net pointing to your server)

## GitHub Repository Setup

### Required GitHub Secrets

Configure the following secrets in your GitHub repository settings (Settings → Secrets and variables → Actions):

- `DOCKER_USERNAME`: Your Docker Hub username
- `DOCKER_PASSWORD`: Your Docker Hub password or access token

### How the CI/CD Works

When you push to the repository:

1. The GitHub Actions workflow (`.github/workflows/docker-build-push.yml`) triggers automatically
2. The workflow builds a Docker image for both `linux/amd64` and `linux/arm64` platforms
3. The image is tagged with:
   - Branch name (e.g., `main`, `develop`)
   - Commit SHA
   - `latest` (for main branch only)
   - Semantic version tags (if pushing a tag like `v1.0.0`)
4. The image is pushed to Docker Hub at `<DOCKER_USERNAME>/label-nesting-web`

## Local Setup

### 1. Environment Configuration

Copy the example environment file and configure it:

```bash
cp .env.example .env
```

Edit `.env` with your settings:

```bash
DOCKER_USERNAME=yourusername
DOCKER_REGISTRY=docker.io
VERSION=latest

# Application settings
ASPNETCORE_ENVIRONMENT=Production

# OpenTelemetry endpoint (if you have one)
OTEL_EXPORTER_OTLP_ENDPOINT=http://your-otel-endpoint:4317
```

### 2. Build and Run Locally

To build and run the container locally:

```bash
# Build the image
docker-compose build

# Start the container
docker-compose up -d

# View logs
docker-compose logs -f labelnesting-web

# Stop the container
docker-compose down
```

## Remote Host Deployment

### 1. Caddy Network Setup

Ensure your caddy-docker-proxy has a network named `caddy`:

```bash
docker network create caddy
```

### 2. Caddy-Docker-Proxy Configuration

The application uses **caddy-docker-proxy** for automatic reverse proxy configuration. No manual Caddyfile editing is required!

The docker-compose.yml file includes labels that automatically configure Caddy:

```yaml
labels:
  caddy: labels.shunty.net
  caddy.reverse_proxy: "{{upstreams 8080}}"
  caddy.encode: gzip
  caddy.header.Strict-Transport-Security: "max-age=31536000; includeSubDomains; preload"
  caddy.header.X-Content-Type-Options: "nosniff"
  caddy.header.X-Frame-Options: "SAMEORIGIN"
  caddy.header.X-XSS-Protection: "1; mode=block"
```

These labels tell Caddy to:
- Listen for requests to `labels.shunty.net`
- Reverse proxy to the container on port 8080
- Enable gzip compression
- Add security headers automatically
- Handle automatic HTTPS with Let's Encrypt

**Note**: Make sure your caddy-docker-proxy container is running with the `caddy` network attached.

### 3. Deploy the Application

On your remote host:

```bash
# Create a directory for the application
mkdir -p ~/label-nesting
cd ~/label-nesting

# Copy docker-compose.yml and .env files to the host
# (Use scp, rsync, or git clone)

# Pull the latest image
docker-compose pull

# Start the application
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f
```

### 4. Updating the Application

When a new version is pushed to Docker Hub:

```bash
cd ~/label-nesting
docker-compose pull
docker-compose up -d
```

## Health Checks

The application includes a health check endpoint at `/health`. You can verify it's running:

```bash
# Via the public domain
curl https://labels.shunty.net/health

# Or directly within the container
docker exec label-nesting-web curl -f http://localhost:8080/health

# Or check Docker health status
docker ps
```

## Troubleshooting

### Container won't start

Check logs:
```bash
docker-compose logs labelnesting-web
```

### Network issues with Caddy

Verify the Caddy network exists and the container is connected:
```bash
docker network ls
docker network inspect caddy
```

### HTTPS/SSL Issues

If HTTPS isn't working:
1. Verify DNS is pointing to your server: `nslookup labels.shunty.net`
2. Ensure caddy-docker-proxy is running and connected to the `caddy` network
3. Check Caddy logs: `docker logs <caddy-docker-proxy-container-name>`
4. Verify ports 80 and 443 are open on your firewall

### Application not accessible

If you can't reach the application:
1. Check if the container is running: `docker ps`
2. Verify the container is on the `caddy` network: `docker network inspect caddy`
3. Check caddy-docker-proxy logs for routing issues
4. Test health check directly in the container (see Health Checks section above)

## Monitoring

### View real-time logs

```bash
docker-compose logs -f labelnesting-web
```

### OpenTelemetry

If you have an OTEL endpoint configured, telemetry data will be sent automatically. The service name is `label-nesting-web`.

## Backup and Restore

If you add persistent data volumes in the future, you can backup with:

```bash
docker-compose down
docker run --rm -v label-nesting_app-data:/data -v $(pwd):/backup alpine tar czf /backup/backup.tar.gz /data
docker-compose up -d
```

## Security Considerations

1. **Environment Variables**: Never commit `.env` files to version control
2. **GitHub Secrets**: Use secrets for Docker credentials, never hardcode them
3. **HTTPS**: Always use Caddy with automatic HTTPS for production
4. **Non-root User**: The Dockerfile runs the application as a non-root user (appuser)
5. **Network Isolation**: The application uses an internal network for service-to-service communication

## Manual Docker Build

If you need to build manually without docker-compose:

```bash
docker build -t yourusername/label-nesting-web:latest .
docker push yourusername/label-nesting-web:latest
```

## GitHub Actions Manual Trigger

You can manually trigger the build workflow from GitHub:

1. Go to Actions tab in your repository
2. Select "Docker Build and Push" workflow
3. Click "Run workflow"
4. Select the branch and click "Run workflow"
