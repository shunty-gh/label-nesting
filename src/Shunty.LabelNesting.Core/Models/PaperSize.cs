namespace Shunty.LabelNesting.Core.Models;

/// <summary>
/// Represents a paper size with width and height in millimeters.
/// </summary>
public readonly record struct PaperSize(string Name, double Width, double Height)
{
    // Standard ISO A sizes (portrait orientation)
    public static readonly PaperSize A2 = new("A2", 420, 594);
    public static readonly PaperSize A3 = new("A3", 297, 420);
    public static readonly PaperSize A4 = new("A4", 210, 297);
    public static readonly PaperSize A5 = new("A5", 148, 210);
    public static readonly PaperSize A6 = new("A6", 105, 148);

    /// <summary>
    /// All standard paper sizes.
    /// </summary>
    public static readonly IReadOnlyList<PaperSize> StandardSizes = [A2, A3, A4, A5, A6];

    /// <summary>
    /// Gets the area of the paper in square millimeters.
    /// </summary>
    public double Area => Width * Height;

    /// <summary>
    /// Creates a custom paper size.
    /// </summary>
    public static PaperSize Custom(double width, double height) => new("Custom", width, height);

    /// <summary>
    /// Parses a paper size from a string. Accepts standard names (A2-A6) or custom format (WxH).
    /// </summary>
    public static PaperSize Parse(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);

        var normalized = input.Trim().ToUpperInvariant();

        // Check standard sizes
        foreach (var size in StandardSizes)
        {
            if (size.Name.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            {
                return size;
            }
        }

        // Try custom format (WxH)
        var separators = new[] { 'x', 'X', '×' };
        foreach (var sep in separators)
        {
            var idx = input.IndexOf(sep);
            if (idx > 0)
            {
                var widthStr = input[..idx].Trim();
                var heightStr = input[(idx + 1)..].Trim();

                if (double.TryParse(widthStr, out var width) && width > 0 &&
                    double.TryParse(heightStr, out var height) && height > 0)
                {
                    return Custom(width, height);
                }
            }
        }

        throw new FormatException($"Invalid paper size: '{input}'. Use A2-A6 or custom format like '200x300'.");
    }

    /// <summary>
    /// Tries to parse a paper size from a string.
    /// </summary>
    public static bool TryParse(string input, out PaperSize size)
    {
        try
        {
            size = Parse(input);
            return true;
        }
        catch
        {
            size = default;
            return false;
        }
    }

    public override string ToString() => Name == "Custom" ? $"{Width}x{Height}mm" : Name;
}
