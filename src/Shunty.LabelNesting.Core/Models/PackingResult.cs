namespace Shunty.LabelNesting.Core.Models;

/// <summary>
/// Result of a packing operation containing all placements and metrics.
/// </summary>
public sealed class PackingResult
{
    /// <summary>
    /// All item placements across all pages.
    /// </summary>
    public required IReadOnlyList<ItemPlacement> Placements { get; init; }

    /// <summary>
    /// The paper size used for packing.
    /// </summary>
    public required PaperSize PaperSize { get; init; }

    /// <summary>
    /// The configuration used for packing.
    /// </summary>
    public required PackingConfiguration Configuration { get; init; }

    /// <summary>
    /// Total number of pages required.
    /// </summary>
    public int PageCount => Placements.Count > 0 ? Placements.Max(p => p.PageIndex) + 1 : 0;

    /// <summary>
    /// Total number of items placed.
    /// </summary>
    public int TotalItemsPlaced => Placements.Count;

    /// <summary>
    /// Gets placements for a specific page.
    /// </summary>
    public IEnumerable<ItemPlacement> GetPagePlacements(int pageIndex) =>
        Placements.Where(p => p.PageIndex == pageIndex);

    /// <summary>
    /// Calculates the efficiency (used area / total area) for a specific page.
    /// </summary>
    public double GetPageEfficiency(int pageIndex)
    {
        var pagePlacements = GetPagePlacements(pageIndex).ToList();
        if (pagePlacements.Count == 0) return 0;

        var usedArea = pagePlacements.Sum(p => p.Width * p.Height);
        var usableWidth = Configuration.GetUsableWidth(PaperSize);
        var usableHeight = Configuration.GetUsableHeight(PaperSize);
        var totalArea = usableWidth * usableHeight;

        return totalArea > 0 ? usedArea / totalArea : 0;
    }

    /// <summary>
    /// Calculates the overall efficiency across all pages.
    /// </summary>
    public double OverallEfficiency
    {
        get
        {
            if (PageCount == 0) return 0;

            var totalUsedArea = Placements.Sum(p => p.Width * p.Height);
            var usableWidth = Configuration.GetUsableWidth(PaperSize);
            var usableHeight = Configuration.GetUsableHeight(PaperSize);
            var totalAvailableArea = usableWidth * usableHeight * PageCount;

            return totalAvailableArea > 0 ? totalUsedArea / totalAvailableArea : 0;
        }
    }
}
