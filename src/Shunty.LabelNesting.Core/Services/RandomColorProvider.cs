namespace Shunty.LabelNesting.Core.Services;

/// <summary>
/// Provides a sequence of visually distinct colors for items.
/// Uses a predefined palette of pastel colors that work well for labels.
/// </summary>
public sealed class RandomColorProvider : IColorProvider
{
    // Predefined palette of distinct, visually appealing pastel colors
    private static readonly string[] Palette =
    [
        "#FFB3BA", // Light pink
        "#BAFFC9", // Light green
        "#BAE1FF", // Light blue
        "#FFFFBA", // Light yellow
        "#FFD9BA", // Light orange
        "#E0BBE4", // Light purple
        "#B5EAD7", // Mint
        "#FFDAC1", // Peach
        "#C7CEEA", // Periwinkle
        "#F0E6EF", // Lavender blush
        "#A8E6CF", // Light seafoam
        "#FDCFE8", // Pink lace
        "#FFF5BA", // Champagne
        "#B4F8C8", // Magic mint
        "#D4A5A5", // Dusty rose
        "#A0CED9", // Light cyan
        "#FFE5B4", // Papaya whip
        "#C1E1C1", // Tea green
        "#F9D5E5", // Fairy tale
        "#B8B8D1"  // Languid lavender
    ];

    private int _currentIndex;

    public string GetNextColor()
    {
        var index = Interlocked.Increment(ref _currentIndex) - 1;
        return Palette[((index % Palette.Length) + Palette.Length) % Palette.Length];
    }

    public void Reset() => Interlocked.Exchange(ref _currentIndex, 0);
}
