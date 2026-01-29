namespace Shunty.LabelNesting.Core.Algorithms;

/// <summary>
/// Heuristics for choosing where to place items in the MaxRects algorithm.
/// </summary>
public enum PackingHeuristic
{
    /// <summary>
    /// Best Short Side Fit: Positions the item so that the shorter leftover side is minimized.
    /// Generally produces good results for mixed-size items.
    /// </summary>
    BestShortSideFit,

    /// <summary>
    /// Best Long Side Fit: Positions the item so that the longer leftover side is minimized.
    /// </summary>
    BestLongSideFit,

    /// <summary>
    /// Best Area Fit: Positions the item in the free rectangle with the smallest area.
    /// </summary>
    BestAreaFit,

    /// <summary>
    /// Bottom-Left: Positions the item as close to the bottom-left corner as possible.
    /// </summary>
    BottomLeft
}
