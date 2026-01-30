using System.Text.Json;
using Shunty.LabelNesting.Core.Models;

namespace Shunty.LabelNesting.Core.Extensions;

/// <summary>
/// Extension methods for PackingResult.
/// </summary>
public static class PackingResultExtensions
{
    private const string ExportFormatVersion = "1.0";

    /// <summary>
    /// Converts a PackingResult to a LayoutExport that can be serialized to JSON.
    /// </summary>
    /// <param name="result">The packing result to export.</param>
    /// <param name="originalItems">The original input items (needed for complete context).</param>
    /// <param name="generator">Name of the application generating this export (default: "Label Nesting").</param>
    /// <returns>A LayoutExport object containing all layout information.</returns>
    public static LayoutExport ToLayoutExport(
        this PackingResult result,
        IReadOnlyList<Item> originalItems,
        string generator = "Label Nesting")
    {
        return new LayoutExport
        {
            Header = new LayoutExportHeader
            {
                Version = ExportFormatVersion,
                GeneratedAt = DateTime.UtcNow,
                Generator = generator
            },
            Input = new LayoutExportInput
            {
                PaperSize = new LayoutExportPaperSize
                {
                    Name = result.PaperSize.Name,
                    WidthMm = result.PaperSize.Width,
                    HeightMm = result.PaperSize.Height
                },
                Configuration = new LayoutExportConfiguration
                {
                    MarginMm = result.Configuration.Margin,
                    GutterMm = result.Configuration.Gutter,
                    AllowRotation = result.Configuration.AllowRotation
                },
                Items = originalItems.Select((item, index) => new LayoutExportItem
                {
                    ItemNumber = index + 1,
                    WidthMm = item.Width,
                    HeightMm = item.Height,
                    Quantity = item.Quantity
                }).ToList()
            },
            Placements = result.Placements.Select(p => new LayoutExportPlacement
            {
                PageNumber = p.PageIndex + 1,
                ItemNumber = p.ItemIndex + 1,
                InstanceNumber = p.InstanceIndex + 1,
                XMm = p.X,
                YMm = p.Y,
                WidthMm = p.Width,
                HeightMm = p.Height,
                IsRotated = p.IsRotated
            }).ToList(),
            Summary = new LayoutExportSummary
            {
                TotalPages = result.PageCount,
                TotalItemsPlaced = result.TotalItemsPlaced,
                OverallEfficiency = result.OverallEfficiency,
                PageDetails = Enumerable.Range(0, result.PageCount)
                    .Select(pageIndex => new LayoutExportPageDetail
                    {
                        PageNumber = pageIndex + 1,
                        ItemsOnPage = result.GetPagePlacements(pageIndex).Count(),
                        Efficiency = result.GetPageEfficiency(pageIndex)
                    }).ToList()
            }
        };
    }

    /// <summary>
    /// Converts a PackingResult to a JSON string representation.
    /// </summary>
    /// <param name="result">The packing result to export.</param>
    /// <param name="originalItems">The original input items.</param>
    /// <param name="indented">Whether to format the JSON with indentation (default: true).</param>
    /// <param name="generator">Name of the application generating this export.</param>
    /// <returns>A JSON string containing the layout information.</returns>
    public static string ToJson(
        this PackingResult result,
        IReadOnlyList<Item> originalItems,
        bool indented = true,
        string generator = "Label Nesting")
    {
        var export = result.ToLayoutExport(originalItems, generator);
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented
        };
        return JsonSerializer.Serialize(export, options);
    }
}
