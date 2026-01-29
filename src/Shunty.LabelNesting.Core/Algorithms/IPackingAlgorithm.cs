using Shunty.LabelNesting.Core.Models;

namespace Shunty.LabelNesting.Core.Algorithms;

/// <summary>
/// Interface for packing algorithms that arrange items on paper.
/// </summary>
public interface IPackingAlgorithm
{
    /// <summary>
    /// Packs the given items onto paper according to the configuration.
    /// </summary>
    /// <param name="items">Items to pack with their quantities.</param>
    /// <param name="paperSize">The paper size to pack onto.</param>
    /// <param name="configuration">Packing configuration options.</param>
    /// <returns>A result containing all item placements.</returns>
    PackingResult Pack(IEnumerable<Item> items, PaperSize paperSize, PackingConfiguration configuration);
}
