using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shunty.LabelNesting.Core.Models;

namespace Shunty.LabelNesting.Core.Services;

/// <summary>
/// QuestPDF implementation of PDF generation for packing results.
/// </summary>
public sealed class PdfGenerator : IPdfGenerator
{
    static PdfGenerator()
    {
        // Configure QuestPDF license (Community license for open source)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void Generate(PackingResult result, string outputPath)
    {
        var document = CreateDocument(result);
        document.GeneratePdf(outputPath);
    }

    public byte[] GenerateBytes(PackingResult result)
    {
        var document = CreateDocument(result);
        return document.GeneratePdf();
    }

    private static IDocument CreateDocument(PackingResult result)
    {
        return Document.Create(container =>
        {
            for (var pageIndex = 0; pageIndex < result.PageCount; pageIndex++)
            {
                var pagePlacements = result.GetPagePlacements(pageIndex).ToList();
                var capturedPageIndex = pageIndex;
                var pageCount = result.PageCount;
                var margin = result.Configuration.Margin;
                var paperWidth = result.PaperSize.Width;
                var paperHeight = result.PaperSize.Height;

                container.Page(page =>
                {
                    page.Size((float)paperWidth, (float)paperHeight, Unit.Millimetre);
                    page.Margin(0);

                    page.Content().Layers(layers =>
                    {
                        // Background layer - draw margin indicator
                        layers.Layer().Padding((float)margin, Unit.Millimetre).Border(0.2f).BorderColor(Colors.Grey.Lighten2);

                        // Items layer - use absolute positioning via SVG or nested containers
                        layers.PrimaryLayer().Container().Element(c =>
                        {
                            DrawPlacements(c, pagePlacements, margin, paperWidth, paperHeight);
                        });

                        // Footer with page number
                        layers.Layer()
                            .AlignBottom()
                            .AlignLeft()
                            .Padding((float)(margin / 2), Unit.Millimetre)
                            .Text($"Page {capturedPageIndex + 1} of {pageCount}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium);
                    });
                });
            }
        });
    }

    private static void DrawPlacements(IContainer container, List<ItemPlacement> placements, double margin, double paperWidth, double paperHeight)
    {
        // Generate SVG for all placements
        var svg = GenerateSvg(placements, paperWidth, paperHeight);
        container.Svg(svg);
    }

    private static string GenerateSvg(List<ItemPlacement> placements, double paperWidth, double paperHeight)
    {
        var svgElements = new System.Text.StringBuilder();

        svgElements.AppendLine($@"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 {paperWidth} {paperHeight}"" width=""{paperWidth}mm"" height=""{paperHeight}mm"">");

        foreach (var placement in placements)
        {
            // Draw rectangle
            svgElements.AppendLine($@"  <rect x=""{placement.X}"" y=""{placement.Y}"" width=""{placement.Width}"" height=""{placement.Height}"" fill=""{placement.Color}"" stroke=""#555"" stroke-width=""0.2""/>");

            // Draw label centered
            var centerX = placement.X + placement.Width / 2;
            var centerY = placement.Y + placement.Height / 2;
            var fontSize = Math.Min(placement.Width, placement.Height) * 0.25;
            svgElements.AppendLine($@"  <text x=""{centerX}"" y=""{centerY}"" text-anchor=""middle"" dominant-baseline=""central"" font-size=""{fontSize}"" fill=""#000"">{placement.Label}</text>");

            // Draw rotation indicator
            if (placement.IsRotated)
            {
                svgElements.AppendLine($@"  <text x=""{placement.X + 1}"" y=""{placement.Y + 4}"" font-size=""3"" fill=""#666"">R</text>");
            }
        }

        svgElements.AppendLine("</svg>");

        return svgElements.ToString();
    }
}
