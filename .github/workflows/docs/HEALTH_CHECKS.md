# Health Check Endpoints

The Label Nesting web application provides multiple health check endpoints for monitoring and orchestration.

## Available Endpoints

### `/health`
**Purpose**: Comprehensive health check that verifies all registered health checks.

**Checks Performed**:
- Basic application liveness ("self" check)
- Core services availability (packing algorithm, PDF generator, color provider)

**Response Codes**:
- `200 OK`: All health checks are passing (Healthy)
- `200 OK` with status "Degraded": Application is running but some non-critical services may be unavailable
- `503 Service Unavailable`: One or more critical health checks failed (Unhealthy)

**Response Format** (JSON):
```json
{
  "status": "Healthy",
  "results": {
    "self": {
      "status": "Healthy",
      "description": null,
      "data": {}
    },
    "label_nesting_core": {
      "status": "Healthy",
      "description": "Label nesting core services are operational",
      "data": {}
    }
  }
}
```

**Usage**:
- Docker health checks
- Kubernetes liveness and readiness probes
- Load balancer health checks

### `/alive`
**Purpose**: Lightweight liveness check that only verifies the application can respond to requests.

**Checks Performed**:
- Basic application liveness only (checks tagged with "live")

**Response Codes**:
- `200 OK`: Application is alive and can accept requests
- `503 Service Unavailable`: Application cannot respond

**Usage**:
- Kubernetes liveness probes
- Basic uptime monitoring
- Quick availability checks

## Docker Integration

The Dockerfile includes a health check using the `/health` endpoint:

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
```

**Parameters**:
- `interval=30s`: Check every 30 seconds
- `timeout=3s`: Each check times out after 3 seconds
- `start-period=10s`: Wait 10 seconds after container starts before beginning checks
- `retries=3`: Mark as unhealthy after 3 consecutive failures

## Testing Health Checks Locally

### Using curl
```bash
# Full health check
curl http://localhost:8080/health

# Liveness check only
curl http://localhost:8080/alive

# Pretty print JSON response
curl -s http://localhost:8080/health | jq
```

### Using Docker
```bash
# Check container health status
docker ps

# View health check logs
docker inspect --format='{{json .State.Health}}' labelnesting-web | jq

# Force a health check
docker exec labelnesting-web curl -f http://localhost:8080/health
```

## Monitoring and Alerting

### Prometheus
If you're using Prometheus, you can scrape the health endpoints:

```yaml
scrape_configs:
  - job_name: 'label-nesting'
    metrics_path: '/health'
    static_configs:
      - targets: ['labelnesting-web:8080']
```

### Kubernetes Probes

#### Liveness Probe
```yaml
livenessProbe:
  httpGet:
    path: /alive
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 30
  timeoutSeconds: 3
  failureThreshold: 3
```

#### Readiness Probe
```yaml
readinessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 10
  timeoutSeconds: 3
  failureThreshold: 3
```

## Adding Custom Health Checks

To add additional health checks, modify `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<LabelNestingCoreHealthCheck>("label_nesting_core", tags: ["ready"])
    .AddCheck<YourCustomHealthCheck>("your_check_name", tags: ["ready"]);
```

For external dependency checks:
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<LabelNestingCoreHealthCheck>("label_nesting_core", tags: ["ready"])
    .AddUrlGroup(new Uri("https://api.example.com/health"), "external_api", tags: ["ready"])
    .AddDbContextCheck<YourDbContext>("database", tags: ["ready"]);
```

## Health Check Status Meanings

- **Healthy**: All checks passed, service is fully operational
- **Degraded**: Service is operational but with reduced functionality (e.g., optional features unavailable)
- **Unhealthy**: Critical checks failed, service should not receive traffic

## Troubleshooting

### Health check always returns unhealthy
1. Check application logs: `docker-compose logs labelnesting-web`
2. Verify all core services are registered in `Program.cs`
3. Check for exceptions in the application startup

### Health check times out
1. Increase the timeout in the health check configuration
2. Check if the application is under heavy load
3. Verify the `/health` endpoint is not blocked by middleware

### Docker reports unhealthy but `/health` returns 200
1. Check if curl is installed in the container
2. Verify the port number matches (8080)
3. Review Docker health check logs: `docker inspect <container-id>`
