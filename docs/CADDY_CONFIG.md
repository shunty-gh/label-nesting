# Caddy-Docker-Proxy Configuration

This document explains how the Label Nesting application integrates with caddy-docker-proxy for automatic reverse proxy configuration.

## Overview

The application uses [caddy-docker-proxy](https://github.com/lucaslorentz/caddy-docker-proxy) which automatically configures Caddy based on Docker container labels. This eliminates the need for manual Caddyfile configuration.

## How It Works

When the container starts, caddy-docker-proxy reads the labels from the docker-compose.yml file and automatically configures Caddy to:
1. Route traffic from `labels.shunty.net` to the application container
2. Obtain and renew SSL certificates from Let's Encrypt automatically
3. Apply security headers
4. Enable gzip compression

## Docker Compose Labels

The following labels in `docker-compose.yml` configure the reverse proxy:

```yaml
labels:
  # Primary domain configuration
  caddy: labels.shunty.net

  # Reverse proxy to the application on port 8080
  caddy.reverse_proxy: "{{upstreams 8080}}"

  # Enable gzip compression
  caddy.encode: gzip

  # Security headers
  caddy.header.Strict-Transport-Security: "max-age=31536000; includeSubDomains; preload"
  caddy.header.X-Content-Type-Options: "nosniff"
  caddy.header.X-Frame-Options: "SAMEORIGIN"
  caddy.header.X-XSS-Protection: "1; mode=block"
```

### Label Explanation

| Label | Purpose |
|-------|---------|
| `caddy` | Defines the domain name(s) for the service |
| `caddy.reverse_proxy` | Configures reverse proxy upstream. `{{upstreams 8080}}` automatically discovers container IP and port |
| `caddy.encode` | Enables compression (gzip, zstd, etc.) |
| `caddy.header.*` | Adds HTTP response headers for security and caching |

## Network Configuration

The application must be on the same Docker network as caddy-docker-proxy:

```yaml
networks:
  caddy:
    external: true
    name: caddy
```

The `caddy` network must be created before deploying:

```bash
docker network create caddy
```

## SSL/TLS (HTTPS)

caddy-docker-proxy automatically handles HTTPS:

- Automatically obtains SSL certificates from Let's Encrypt
- Renews certificates before expiration
- Redirects HTTP to HTTPS
- Uses modern TLS settings by default

**Requirements for automatic HTTPS:**
1. DNS must point to your server (e.g., `labels.shunty.net` → your server IP)
2. Ports 80 and 443 must be open and accessible
3. caddy-docker-proxy must be able to complete Let's Encrypt HTTP or DNS challenges

## Advanced Configuration

### Multiple Domains

To serve the application on multiple domains:

```yaml
labels:
  caddy: labels.shunty.net, www.labels.shunty.net, alt.shunty.net
```

### Path-Based Routing

To serve on a specific path:

```yaml
labels:
  caddy: labels.shunty.net
  caddy.@app.path: /app /app/*
  caddy.reverse_proxy: "@app {{upstreams 8080}}"
```

### Custom TLS Configuration

To use a custom certificate or DNS challenge:

```yaml
labels:
  caddy: labels.shunty.net
  caddy.tls: "your-email@example.com"
  # Or for DNS challenge (e.g., Cloudflare)
  caddy.tls.dns: cloudflare {env.CLOUDFLARE_API_TOKEN}
```

### Rate Limiting

To add rate limiting:

```yaml
labels:
  caddy: labels.shunty.net
  caddy.rate_limit: "{remote.ip} 100r/m"
```

### IP Whitelisting

To restrict access to specific IPs:

```yaml
labels:
  caddy: labels.shunty.net
  caddy.@allowed.remote_ip: "10.0.0.0/8 192.168.0.0/16"
  caddy.reverse_proxy: "@allowed {{upstreams 8080}}"
```

### Custom Headers

To add custom response headers:

```yaml
labels:
  caddy.header.Cache-Control: "public, max-age=3600"
  caddy.header.Custom-Header: "value"
```

## Caddy-Docker-Proxy Setup

If you don't have caddy-docker-proxy running yet, here's a basic setup:

### Create docker-compose.yml for Caddy-Docker-Proxy

```yaml
version: '3.8'

services:
  caddy:
    image: lucaslorentz/caddy-docker-proxy:latest
    container_name: caddy-docker-proxy
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    environment:
      - CADDY_INGRESS_NETWORKS=caddy
    networks:
      - caddy
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro
      - caddy_data:/data
      - caddy_config:/config

networks:
  caddy:
    external: true

volumes:
  caddy_data:
  caddy_config:
```

### Start Caddy-Docker-Proxy

```bash
# Create the network
docker network create caddy

# Start caddy-docker-proxy
docker-compose up -d
```

## Debugging

### View Caddy Configuration

To see the generated Caddy configuration:

```bash
docker exec caddy-docker-proxy caddy config
```

### View Caddy Logs

```bash
docker logs -f caddy-docker-proxy
```

### Test Label Detection

Check if caddy-docker-proxy detects your container:

```bash
# List all containers on the caddy network
docker network inspect caddy

# Check if your app container is listed
docker ps --filter "network=caddy"
```

### Force Certificate Renewal

If you need to force certificate renewal:

```bash
docker exec caddy-docker-proxy rm -rf /data/caddy/certificates
docker restart caddy-docker-proxy
```

## Common Issues

### Certificates Not Obtained

**Problem**: HTTPS not working, certificate errors

**Solutions**:
1. Verify DNS resolves correctly: `nslookup labels.shunty.net`
2. Check ports 80 and 443 are accessible from the internet
3. Check Caddy logs for Let's Encrypt errors
4. Ensure no firewall is blocking Let's Encrypt validation

### Container Not Detected

**Problem**: Application not accessible through Caddy

**Solutions**:
1. Verify container is on the `caddy` network: `docker network inspect caddy`
2. Restart caddy-docker-proxy: `docker restart caddy-docker-proxy`
3. Check labels are correctly set: `docker inspect label-nesting-web`

### SSL Certificate Mismatch

**Problem**: Browser shows certificate for wrong domain

**Solutions**:
1. Clear Caddy's certificates: `docker exec caddy-docker-proxy rm -rf /data/caddy/certificates`
2. Restart Caddy: `docker restart caddy-docker-proxy`
3. Wait a few minutes for new certificates to be obtained

## Security Considerations

1. **Docker Socket Access**: caddy-docker-proxy needs read access to Docker socket (`/var/run/docker.sock`)
2. **Network Isolation**: Only containers needing reverse proxy should be on the `caddy` network
3. **Certificate Storage**: Certificates are stored in Docker volumes - ensure regular backups
4. **HTTPS Only**: Caddy automatically redirects HTTP to HTTPS - this is secure by default
5. **Security Headers**: The provided headers configuration follows OWASP best practices

## References

- [caddy-docker-proxy GitHub](https://github.com/lucaslorentz/caddy-docker-proxy)
- [Caddy Documentation](https://caddyserver.com/docs/)
- [Docker Labels](https://docs.docker.com/config/labels-custom-metadata/)
- [Let's Encrypt](https://letsencrypt.org/)
