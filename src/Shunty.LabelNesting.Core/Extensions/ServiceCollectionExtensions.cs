using Microsoft.Extensions.DependencyInjection;
using Shunty.LabelNesting.Core.Algorithms;
using Shunty.LabelNesting.Core.Services;

namespace Shunty.LabelNesting.Core.Extensions;

/// <summary>
/// Extension methods for registering label nesting services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds label nesting core services to the service collection.
    /// </summary>
    public static IServiceCollection AddLabelNestingCore(
        this IServiceCollection services,
        PackingHeuristic heuristic = PackingHeuristic.BestShortSideFit)
    {
        services.AddTransient<IColorProvider, RandomColorProvider>();
        services.AddTransient<IPackingAlgorithm>(sp =>
            new MaxRectsAlgorithm(sp.GetRequiredService<IColorProvider>(), heuristic));
        services.AddTransient<IPdfGenerator, PdfGenerator>();

        return services;
    }
}
