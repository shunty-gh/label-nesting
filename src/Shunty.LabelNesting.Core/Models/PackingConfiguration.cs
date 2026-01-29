namespace Shunty.LabelNesting.Core.Models;

/// <summary>
/// Configuration options for the packing algorithm.
/// </summary>
/// <param name="Margin">Page margin in millimeters (non-printable area).</param>
/// <param name="Gutter">Space between items in millimeters.</param>
/// <param name="AllowRotation">Whether items can be rotated 90 degrees to fit better.</param>
public readonly record struct PackingConfiguration(
    double Margin = 5.0,
    double Gutter = 2.0,
    bool AllowRotation = true)
{
    /// <summary>
    /// Default configuration with 5mm margin, 2mm gutter, rotation enabled.
    /// </summary>
    public static readonly PackingConfiguration Default = new();

    /// <summary>
    /// Gets the usable width of a paper size after accounting for margins.
    /// </summary>
    public double GetUsableWidth(PaperSize paper) => paper.Width - (2 * Margin);

    /// <summary>
    /// Gets the usable height of a paper size after accounting for margins.
    /// </summary>
    public double GetUsableHeight(PaperSize paper) => paper.Height - (2 * Margin);

    /// <summary>
    /// Validates that an item can fit on the paper with this configuration.
    /// </summary>
    public bool CanFit(Item item, PaperSize paper)
    {
        var usableWidth = GetUsableWidth(paper);
        var usableHeight = GetUsableHeight(paper);

        // Check if item fits in normal orientation
        if (item.Width <= usableWidth && item.Height <= usableHeight)
        {
            return true;
        }

        // Check if item fits when rotated (if rotation is allowed)
        if (AllowRotation && item.Height <= usableWidth && item.Width <= usableHeight)
        {
            return true;
        }

        return false;
    }
}
