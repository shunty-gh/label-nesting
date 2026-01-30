using System.Text.Json.Serialization;

namespace Shunty.LabelNesting.Core.Models;

/// <summary>
/// Represents a complete layout export that can be serialized to JSON
/// and used to recreate the packing layout in another system.
/// </summary>
public sealed class LayoutExport
{
    /// <summary>
    /// Metadata about this layout export.
    /// </summary>
    [JsonPropertyName("header")]
    public required LayoutExportHeader Header { get; init; }

    /// <summary>
    /// The original input items before packing.
    /// </summary>
    [JsonPropertyName("input")]
    public required LayoutExportInput Input { get; init; }

    /// <summary>
    /// The placed items with their positions and pages.
    /// </summary>
    [JsonPropertyName("placements")]
    public required IReadOnlyList<LayoutExportPlacement> Placements { get; init; }

    /// <summary>
    /// Summary statistics.
    /// </summary>
    [JsonPropertyName("summary")]
    public required LayoutExportSummary Summary { get; init; }
}

/// <summary>
/// Header information for the layout export.
/// </summary>
public sealed class LayoutExportHeader
{
    /// <summary>
    /// Version of the export format.
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    /// <summary>
    /// When this layout was generated.
    /// </summary>
    [JsonPropertyName("generatedAt")]
    public required DateTime GeneratedAt { get; init; }

    /// <summary>
    /// Application that generated this layout.
    /// </summary>
    [JsonPropertyName("generator")]
    public required string Generator { get; init; }
}

/// <summary>
/// Input configuration and items.
/// </summary>
public sealed class LayoutExportInput
{
    /// <summary>
    /// Paper size details.
    /// </summary>
    [JsonPropertyName("paperSize")]
    public required LayoutExportPaperSize PaperSize { get; init; }

    /// <summary>
    /// Packing configuration.
    /// </summary>
    [JsonPropertyName("configuration")]
    public required LayoutExportConfiguration Configuration { get; init; }

    /// <summary>
    /// Original items to be placed.
    /// </summary>
    [JsonPropertyName("items")]
    public required IReadOnlyList<LayoutExportItem> Items { get; init; }
}

/// <summary>
/// Paper size information.
/// </summary>
public sealed class LayoutExportPaperSize
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("widthMm")]
    public required double WidthMm { get; init; }

    [JsonPropertyName("heightMm")]
    public required double HeightMm { get; init; }
}

/// <summary>
/// Packing configuration.
/// </summary>
public sealed class LayoutExportConfiguration
{
    [JsonPropertyName("marginMm")]
    public required double MarginMm { get; init; }

    [JsonPropertyName("gutterMm")]
    public required double GutterMm { get; init; }

    [JsonPropertyName("allowRotation")]
    public required bool AllowRotation { get; init; }
}

/// <summary>
/// An original input item.
/// </summary>
public sealed class LayoutExportItem
{
    [JsonPropertyName("itemNumber")]
    public required int ItemNumber { get; init; }

    [JsonPropertyName("widthMm")]
    public required double WidthMm { get; init; }

    [JsonPropertyName("heightMm")]
    public required double HeightMm { get; init; }

    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }
}

/// <summary>
/// A placed item with its position on a page.
/// </summary>
public sealed class LayoutExportPlacement
{
    /// <summary>
    /// Page number (1-based).
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public required int PageNumber { get; init; }

    /// <summary>
    /// Original item number (1-based).
    /// </summary>
    [JsonPropertyName("itemNumber")]
    public required int ItemNumber { get; init; }

    /// <summary>
    /// Instance number for this item (1-based).
    /// </summary>
    [JsonPropertyName("instanceNumber")]
    public required int InstanceNumber { get; init; }

    /// <summary>
    /// X coordinate of top-left corner (mm from left edge of paper).
    /// </summary>
    [JsonPropertyName("xMm")]
    public required double XMm { get; init; }

    /// <summary>
    /// Y coordinate of top-left corner (mm from top edge of paper).
    /// </summary>
    [JsonPropertyName("yMm")]
    public required double YMm { get; init; }

    /// <summary>
    /// Width of the placed item (mm). May differ from original if rotated.
    /// </summary>
    [JsonPropertyName("widthMm")]
    public required double WidthMm { get; init; }

    /// <summary>
    /// Height of the placed item (mm). May differ from original if rotated.
    /// </summary>
    [JsonPropertyName("heightMm")]
    public required double HeightMm { get; init; }

    /// <summary>
    /// Whether this item was rotated 90 degrees.
    /// </summary>
    [JsonPropertyName("isRotated")]
    public required bool IsRotated { get; init; }
}

/// <summary>
/// Summary statistics of the layout.
/// </summary>
public sealed class LayoutExportSummary
{
    [JsonPropertyName("totalPages")]
    public required int TotalPages { get; init; }

    [JsonPropertyName("totalItemsPlaced")]
    public required int TotalItemsPlaced { get; init; }

    [JsonPropertyName("overallEfficiency")]
    public required double OverallEfficiency { get; init; }

    [JsonPropertyName("pageDetails")]
    public required IReadOnlyList<LayoutExportPageDetail> PageDetails { get; init; }
}

/// <summary>
/// Details for a single page.
/// </summary>
public sealed class LayoutExportPageDetail
{
    [JsonPropertyName("pageNumber")]
    public required int PageNumber { get; init; }

    [JsonPropertyName("itemsOnPage")]
    public required int ItemsOnPage { get; init; }

    [JsonPropertyName("efficiency")]
    public required double Efficiency { get; init; }
}
