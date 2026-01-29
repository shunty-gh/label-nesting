namespace Shunty.LabelNesting.Core.Services;

/// <summary>
/// Provides colors for visual distinction of items.
/// </summary>
public interface IColorProvider
{
    /// <summary>
    /// Gets the next color in the sequence.
    /// </summary>
    /// <returns>A color in hex format (e.g., "#FF5733").</returns>
    string GetNextColor();

    /// <summary>
    /// Resets the color sequence to the beginning.
    /// </summary>
    void Reset();
}
