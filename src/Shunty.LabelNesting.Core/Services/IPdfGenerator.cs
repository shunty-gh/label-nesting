using Shunty.LabelNesting.Core.Models;

namespace Shunty.LabelNesting.Core.Services;

/// <summary>
/// Generates PDF documents from packing results.
/// </summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Generates a PDF document from a packing result.
    /// </summary>
    /// <param name="result">The packing result to render.</param>
    /// <param name="outputPath">The path to save the PDF file.</param>
    void Generate(PackingResult result, string outputPath);

    /// <summary>
    /// Generates a PDF document from a packing result and returns it as a byte array.
    /// </summary>
    /// <param name="result">The packing result to render.</param>
    /// <returns>The PDF document as a byte array.</returns>
    byte[] GenerateBytes(PackingResult result);
}
