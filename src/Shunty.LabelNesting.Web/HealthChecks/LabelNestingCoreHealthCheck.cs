using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shunty.LabelNesting.Core.Algorithms;
using Shunty.LabelNesting.Core.Services;

namespace Shunty.LabelNesting.Web.HealthChecks;

/// <summary>
/// Health check that verifies the label nesting core services are available and functioning.
/// </summary>
public class LabelNestingCoreHealthCheck : IHealthCheck
{
    private readonly ILogger<LabelNestingCoreHealthCheck> _logger;
    private readonly IServiceProvider _serviceProvider;

    public LabelNestingCoreHealthCheck(
        ILogger<LabelNestingCoreHealthCheck> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify core services can be resolved
            var packingAlgorithm = _serviceProvider.GetService<IPackingAlgorithm>();
            var pdfGenerator = _serviceProvider.GetService<IPdfGenerator>();
            var colorProvider = _serviceProvider.GetService<IColorProvider>();

            if (packingAlgorithm == null || pdfGenerator == null || colorProvider == null)
            {
                _logger.LogWarning("One or more core services could not be resolved");
                return Task.FromResult(
                    HealthCheckResult.Degraded("Some core services are not available"));
            }

            // Additional checks could be added here, such as:
            // - Checking if required dependencies are available
            // - Validating configuration
            // - Testing basic functionality

            return Task.FromResult(
                HealthCheckResult.Healthy("Label nesting core services are operational"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(
                HealthCheckResult.Unhealthy("An error occurred while checking health", ex));
        }
    }
}
