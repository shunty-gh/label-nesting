namespace Shunty.LabelNesting.Core.Models;

/// <summary>
/// Represents a placed item with its position, page, and visual properties.
/// </summary>
/// <param name="X">X coordinate of the top-left corner in millimeters.</param>
/// <param name="Y">Y coordinate of the top-left corner in millimeters.</param>
/// <param name="Width">Width of the placed item in millimeters (may differ from original if rotated).</param>
/// <param name="Height">Height of the placed item in millimeters (may differ from original if rotated).</param>
/// <param name="PageIndex">Zero-based index of the page this item is placed on.</param>
/// <param name="ItemIndex">Index of the original item this placement corresponds to.</param>
/// <param name="InstanceIndex">Instance number for items with quantity > 1.</param>
/// <param name="IsRotated">Whether the item was rotated 90 degrees to fit.</param>
/// <param name="Color">Background color for visual distinction (hex format).</param>
public readonly record struct ItemPlacement(
    double X,
    double Y,
    double Width,
    double Height,
    int PageIndex,
    int ItemIndex,
    int InstanceIndex,
    bool IsRotated,
    string Color)
{
    /// <summary>
    /// Gets the right edge X coordinate.
    /// </summary>
    public double Right => X + Width;

    /// <summary>
    /// Gets the bottom edge Y coordinate.
    /// </summary>
    public double Bottom => Y + Height;

    /// <summary>
    /// Gets a display label for this placement.
    /// </summary>
    public string Label => $"{ItemIndex + 1}.{InstanceIndex + 1}";
}
