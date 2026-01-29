using Shunty.LabelNesting.Core.Models;

namespace Shunty.LabelNesting.Web.Services;

/// <summary>
/// Service to manage nesting state across pages.
/// </summary>
public sealed class NestingStateService
{
    public PaperSize PaperSize { get; set; } = PaperSize.A4;
    public PackingConfiguration Configuration { get; set; } = PackingConfiguration.Default;
    public List<Item> Items { get; set; } = [];
    public PackingResult? Result { get; set; }
    public byte[]? PdfBytes { get; set; }

    public event Action? OnStateChanged;

    public void NotifyStateChanged() => OnStateChanged?.Invoke();

    public void Reset()
    {
        PaperSize = PaperSize.A4;
        Configuration = PackingConfiguration.Default;
        Items = [];
        Result = null;
        PdfBytes = null;
        NotifyStateChanged();
    }
}
