namespace Shunty.LabelNesting.Core.Models;

/// <summary>
/// Represents an input item to be placed on paper.
/// </summary>
/// <param name="Width">Width of the item in millimeters.</param>
/// <param name="Height">Height of the item in millimeters.</param>
/// <param name="Quantity">Number of copies of this item to place.</param>
public readonly record struct Item(double Width, double Height, int Quantity)
{
    /// <summary>
    /// Gets the area of a single item in square millimeters.
    /// </summary>
    public double Area => Width * Height;

    /// <summary>
    /// Parses an item from a string in the format "width,height,quantity".
    /// </summary>
    public static Item Parse(string input)
    {
        var parts = input.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length is not (2 or 3))
        {
            throw new FormatException($"Invalid item format: '{input}'. Expected 'width,height' or 'width,height,quantity'.");
        }

        if (!double.TryParse(parts[0], out var width) || width <= 0)
        {
            throw new FormatException($"Invalid width: '{parts[0]}'. Must be a positive number.");
        }

        if (!double.TryParse(parts[1], out var height) || height <= 0)
        {
            throw new FormatException($"Invalid height: '{parts[1]}'. Must be a positive number.");
        }

        var quantity = 1;
        if (parts.Length == 3)
        {
            if (!int.TryParse(parts[2], out quantity) || quantity <= 0)
            {
                throw new FormatException($"Invalid quantity: '{parts[2]}'. Must be a positive integer.");
            }
        }

        return new Item(width, height, quantity);
    }

    /// <summary>
    /// Tries to parse an item from a string.
    /// </summary>
    public static bool TryParse(string input, out Item item)
    {
        try
        {
            item = Parse(input);
            return true;
        }
        catch (FormatException)
        {
            item = default;
            return false;
        }
    }
}
