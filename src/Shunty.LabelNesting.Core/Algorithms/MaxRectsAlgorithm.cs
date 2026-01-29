using Shunty.LabelNesting.Core.Models;
using Shunty.LabelNesting.Core.Services;

namespace Shunty.LabelNesting.Core.Algorithms;

/// <summary>
/// MaxRects bin packing algorithm implementation.
/// Maintains a list of free rectangles and places items using the specified heuristic.
/// </summary>
public sealed class MaxRectsAlgorithm(IColorProvider colorProvider, PackingHeuristic heuristic = PackingHeuristic.BestShortSideFit) : IPackingAlgorithm
{
    private readonly record struct FreeRect(double X, double Y, double Width, double Height);

    private readonly record struct ItemToPlace(int ItemIndex, int InstanceIndex, double Width, double Height);

    private readonly record struct PlacementCandidate(FreeRect Rect, double X, double Y, double Width, double Height, bool Rotated, double Score1, double Score2);

    public PackingResult Pack(IEnumerable<Item> items, PaperSize paperSize, PackingConfiguration configuration)
    {
        var itemList = items.ToList();

        // Validate all items can fit
        ValidateItems(itemList, paperSize, configuration);

        // Expand items by quantity and sort by area descending (larger items first)
        var itemsToPlace = ExpandAndSortItems(itemList);

        // Get colors for each item type
        var colors = itemList.Select(_ => colorProvider.GetNextColor()).ToList();

        var placements = new List<ItemPlacement>();
        var currentPageIndex = 0;
        var usableWidth = configuration.GetUsableWidth(paperSize);
        var usableHeight = configuration.GetUsableHeight(paperSize);
        var freeRects = new List<FreeRect> { new(0, 0, usableWidth, usableHeight) };

        foreach (var item in itemsToPlace)
        {
            var placed = TryPlaceItem(item, freeRects, configuration, currentPageIndex, colors[item.ItemIndex], placements);

            if (!placed)
            {
                // Start a new page
                currentPageIndex++;
                freeRects = [new(0, 0, usableWidth, usableHeight)];
                placed = TryPlaceItem(item, freeRects, configuration, currentPageIndex, colors[item.ItemIndex], placements);

                if (!placed)
                {
                    // This should not happen if validation passed
                    throw new InvalidOperationException($"Failed to place item {item.ItemIndex + 1}.{item.InstanceIndex + 1}");
                }
            }
        }

        return new PackingResult
        {
            Placements = placements,
            PaperSize = paperSize,
            Configuration = configuration
        };
    }

    private static void ValidateItems(List<Item> items, PaperSize paperSize, PackingConfiguration configuration)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (!configuration.CanFit(item, paperSize))
            {
                var rotationNote = configuration.AllowRotation ? " even when rotated" : "";
                throw new ArgumentException(
                    $"Item {i + 1} ({item.Width}x{item.Height}mm) is too large to fit on {paperSize}{rotationNote}. " +
                    $"Usable area is {configuration.GetUsableWidth(paperSize)}x{configuration.GetUsableHeight(paperSize)}mm.");
            }
        }
    }

    private static List<ItemToPlace> ExpandAndSortItems(List<Item> items)
    {
        var expanded = new List<ItemToPlace>();

        for (var itemIndex = 0; itemIndex < items.Count; itemIndex++)
        {
            var item = items[itemIndex];
            for (var instance = 0; instance < item.Quantity; instance++)
            {
                expanded.Add(new ItemToPlace(itemIndex, instance, item.Width, item.Height));
            }
        }

        // Sort by area descending (larger items first for better packing)
        return [.. expanded.OrderByDescending(i => i.Width * i.Height)];
    }

    private bool TryPlaceItem(
        ItemToPlace item,
        List<FreeRect> freeRects,
        PackingConfiguration config,
        int pageIndex,
        string color,
        List<ItemPlacement> placements)
    {
        var candidate = FindBestPlacement(item, freeRects, config.AllowRotation);

        if (candidate is null)
        {
            return false;
        }

        var c = candidate.Value;

        // Add the placement with margin offset
        placements.Add(new ItemPlacement(
            X: c.X + config.Margin,
            Y: c.Y + config.Margin,
            Width: c.Width,
            Height: c.Height,
            PageIndex: pageIndex,
            ItemIndex: item.ItemIndex,
            InstanceIndex: item.InstanceIndex,
            IsRotated: c.Rotated,
            Color: color
        ));

        // Split the free rectangles
        SplitFreeRects(freeRects, c.X, c.Y, c.Width + config.Gutter, c.Height + config.Gutter);

        return true;
    }

    private PlacementCandidate? FindBestPlacement(ItemToPlace item, List<FreeRect> freeRects, bool allowRotation)
    {
        PlacementCandidate? best = null;

        foreach (var rect in freeRects)
        {
            // Try normal orientation
            if (item.Width <= rect.Width && item.Height <= rect.Height)
            {
                var (score1, score2) = CalculateScore(rect, item.Width, item.Height);
                if (best is null || IsBetterScore(score1, score2, best.Value.Score1, best.Value.Score2))
                {
                    best = new PlacementCandidate(rect, rect.X, rect.Y, item.Width, item.Height, false, score1, score2);
                }
            }

            // Try rotated orientation
            if (allowRotation && item.Height <= rect.Width && item.Width <= rect.Height)
            {
                var (score1, score2) = CalculateScore(rect, item.Height, item.Width);
                if (best is null || IsBetterScore(score1, score2, best.Value.Score1, best.Value.Score2))
                {
                    best = new PlacementCandidate(rect, rect.X, rect.Y, item.Height, item.Width, true, score1, score2);
                }
            }
        }

        return best;
    }

    private (double Score1, double Score2) CalculateScore(FreeRect rect, double itemWidth, double itemHeight)
    {
        var leftoverH = rect.Width - itemWidth;
        var leftoverV = rect.Height - itemHeight;

        return heuristic switch
        {
            PackingHeuristic.BestShortSideFit => (Math.Min(leftoverH, leftoverV), Math.Max(leftoverH, leftoverV)),
            PackingHeuristic.BestLongSideFit => (Math.Max(leftoverH, leftoverV), Math.Min(leftoverH, leftoverV)),
            PackingHeuristic.BestAreaFit => (rect.Width * rect.Height, Math.Min(leftoverH, leftoverV)),
            PackingHeuristic.BottomLeft => (rect.Y, rect.X),
            _ => (Math.Min(leftoverH, leftoverV), Math.Max(leftoverH, leftoverV))
        };
    }

    private static bool IsBetterScore(double score1, double score2, double bestScore1, double bestScore2)
    {
        return score1 < bestScore1 || (Math.Abs(score1 - bestScore1) < 0.001 && score2 < bestScore2);
    }

    private static void SplitFreeRects(List<FreeRect> freeRects, double x, double y, double width, double height)
    {
        var newRects = new List<FreeRect>();
        var toRemove = new List<int>();

        for (var i = 0; i < freeRects.Count; i++)
        {
            var rect = freeRects[i];

            // Check if the placed item intersects with this free rect
            if (!Intersects(rect, x, y, width, height))
            {
                continue;
            }

            toRemove.Add(i);

            // Split into up to 4 new rectangles

            // Left
            if (x > rect.X)
            {
                newRects.Add(new FreeRect(rect.X, rect.Y, x - rect.X, rect.Height));
            }

            // Right
            if (x + width < rect.X + rect.Width)
            {
                newRects.Add(new FreeRect(x + width, rect.Y, rect.X + rect.Width - (x + width), rect.Height));
            }

            // Top
            if (y > rect.Y)
            {
                newRects.Add(new FreeRect(rect.X, rect.Y, rect.Width, y - rect.Y));
            }

            // Bottom
            if (y + height < rect.Y + rect.Height)
            {
                newRects.Add(new FreeRect(rect.X, y + height, rect.Width, rect.Y + rect.Height - (y + height)));
            }
        }

        // Remove intersected rects in reverse order to maintain indices
        for (var i = toRemove.Count - 1; i >= 0; i--)
        {
            freeRects.RemoveAt(toRemove[i]);
        }

        // Add new rects
        freeRects.AddRange(newRects);

        // Remove redundant rectangles (those fully contained in another)
        PruneFreeRects(freeRects);
    }

    private static bool Intersects(FreeRect rect, double x, double y, double width, double height)
    {
        return x < rect.X + rect.Width &&
               x + width > rect.X &&
               y < rect.Y + rect.Height &&
               y + height > rect.Y;
    }

    private static void PruneFreeRects(List<FreeRect> freeRects)
    {
        var toRemove = new HashSet<int>();

        for (var i = 0; i < freeRects.Count; i++)
        {
            if (toRemove.Contains(i)) continue;

            for (var j = i + 1; j < freeRects.Count; j++)
            {
                if (toRemove.Contains(j)) continue;

                if (Contains(freeRects[i], freeRects[j]))
                {
                    toRemove.Add(j);
                }
                else if (Contains(freeRects[j], freeRects[i]))
                {
                    toRemove.Add(i);
                    break;
                }
            }
        }

        foreach (var i in toRemove.OrderByDescending(x => x))
        {
            freeRects.RemoveAt(i);
        }
    }

    private static bool Contains(FreeRect outer, FreeRect inner)
    {
        return inner.X >= outer.X &&
               inner.Y >= outer.Y &&
               inner.X + inner.Width <= outer.X + outer.Width &&
               inner.Y + inner.Height <= outer.Y + outer.Height;
    }
}
