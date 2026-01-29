using Shunty.LabelNesting.Core.Models;

namespace Shunty.LabelNesting.Core.Validation;

/// <summary>
/// Validates items before packing.
/// </summary>
public static class ItemValidator
{
    /// <summary>
    /// Validates that all items can fit on the specified paper size.
    /// </summary>
    /// <param name="items">The items to validate.</param>
    /// <param name="paperSize">The target paper size.</param>
    /// <param name="configuration">The packing configuration.</param>
    /// <returns>A list of validation errors, empty if all items are valid.</returns>
    public static List<string> ValidateItems(
        IEnumerable<Item> items,
        PaperSize paperSize,
        PackingConfiguration configuration)
    {
        var errors = new List<string>();
        var itemList = items.ToList();

        for (var i = 0; i < itemList.Count; i++)
        {
            var item = itemList[i];
            var itemNumber = i + 1;

            // Validate dimensions are positive
            if (item.Width <= 0)
            {
                errors.Add($"Item {itemNumber}: Width must be positive (got {item.Width}).");
            }

            if (item.Height <= 0)
            {
                errors.Add($"Item {itemNumber}: Height must be positive (got {item.Height}).");
            }

            if (item.Quantity <= 0)
            {
                errors.Add($"Item {itemNumber}: Quantity must be positive (got {item.Quantity}).");
            }

            // Validate item fits on paper
            if (item.Width > 0 && item.Height > 0 && !configuration.CanFit(item, paperSize))
            {
                var usableWidth = configuration.GetUsableWidth(paperSize);
                var usableHeight = configuration.GetUsableHeight(paperSize);
                var rotationNote = configuration.AllowRotation
                    ? " (even when rotated)"
                    : " (rotation is disabled)";

                errors.Add(
                    $"Item {itemNumber}: Size {item.Width}x{item.Height}mm is too large to fit on {paperSize}" +
                    $"{rotationNote}. Usable area: {usableWidth}x{usableHeight}mm.");
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates a single item specification string.
    /// </summary>
    public static (bool IsValid, string? Error) ValidateItemString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (false, "Item specification cannot be empty.");
        }

        if (!Item.TryParse(input, out var item))
        {
            return (false, $"Invalid format: '{input}'. Expected 'width,height' or 'width,height,quantity'.");
        }

        if (item.Width <= 0 || item.Height <= 0)
        {
            return (false, "Width and height must be positive numbers.");
        }

        if (item.Quantity <= 0)
        {
            return (false, "Quantity must be a positive integer.");
        }

        return (true, null);
    }
}
