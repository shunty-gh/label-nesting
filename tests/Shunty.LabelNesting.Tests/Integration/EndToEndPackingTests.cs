using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shunty.LabelNesting.Core.Algorithms;
using Shunty.LabelNesting.Core.Models;
using Shunty.LabelNesting.Core.Services;

namespace Shunty.LabelNesting.Tests.Integration;

[TestClass]
public sealed class EndToEndPackingTests
{
    [TestMethod]
    public void FullWorkflow_PackItemsAndGeneratePdf_Succeeds()
    {
        // Arrange
        var items = new List<Item>
        {
            new(100, 50, 3),
            new(75, 25, 5),
            new(50, 50, 4)
        };
        var paperSize = PaperSize.A4;
        var config = PackingConfiguration.Default;

        var colorProvider = new RandomColorProvider();
        var algorithm = new MaxRectsAlgorithm(colorProvider);
        var pdfGenerator = new PdfGenerator();

        // Act
        var result = algorithm.Pack(items, paperSize, config);
        var pdfBytes = pdfGenerator.GenerateBytes(result);

        // Assert
        Assert.AreEqual(12, result.TotalItemsPlaced);
        Assert.IsTrue(result.PageCount >= 1);
        Assert.IsNotNull(pdfBytes);
        Assert.IsTrue(pdfBytes.Length > 0);

        // Verify PDF starts with correct header
        Assert.AreEqual((byte)'%', pdfBytes[0]);
        Assert.AreEqual((byte)'P', pdfBytes[1]);
        Assert.AreEqual((byte)'D', pdfBytes[2]);
        Assert.AreEqual((byte)'F', pdfBytes[3]);
    }

    [TestMethod]
    public void FullWorkflow_CustomPaperSize_Succeeds()
    {
        var items = new List<Item> { new(80, 80, 2) };
        var paperSize = PaperSize.Custom(200, 200);
        var config = new PackingConfiguration(Margin: 5, Gutter: 2, AllowRotation: true);

        var colorProvider = new RandomColorProvider();
        var algorithm = new MaxRectsAlgorithm(colorProvider);

        var result = algorithm.Pack(items, paperSize, config);

        Assert.AreEqual(2, result.TotalItemsPlaced);
        Assert.AreEqual("Custom", result.PaperSize.Name);
    }

    [TestMethod]
    public void FullWorkflow_NoRotation_RespectsConstraint()
    {
        // Item that would benefit from rotation but shouldn't be rotated
        var items = new List<Item> { new(60, 150, 1) };
        var config = new PackingConfiguration(Margin: 5, Gutter: 0, AllowRotation: false);

        var colorProvider = new RandomColorProvider();
        var algorithm = new MaxRectsAlgorithm(colorProvider);

        var result = algorithm.Pack(items, PaperSize.A4, config);

        Assert.AreEqual(1, result.TotalItemsPlaced);
        Assert.IsFalse(result.Placements[0].IsRotated);
    }

    [TestMethod]
    public void FullWorkflow_LargeQuantity_HandlesMultiplePages()
    {
        var items = new List<Item> { new(50, 50, 100) };
        var config = PackingConfiguration.Default;

        var colorProvider = new RandomColorProvider();
        var algorithm = new MaxRectsAlgorithm(colorProvider);

        var result = algorithm.Pack(items, PaperSize.A4, config);

        Assert.AreEqual(100, result.TotalItemsPlaced);
        Assert.IsTrue(result.PageCount > 1);

        // Verify items are distributed across pages
        for (var i = 0; i < result.PageCount; i++)
        {
            var pageItems = result.GetPagePlacements(i).Count();
            Assert.IsTrue(pageItems > 0, $"Page {i} should have items");
        }
    }

    [TestMethod]
    public void FullWorkflow_MixedSizes_AchievesReasonableEfficiency()
    {
        var items = new List<Item>
        {
            new(100, 100, 2),
            new(80, 40, 4),
            new(50, 30, 8),
            new(25, 25, 16)
        };

        var colorProvider = new RandomColorProvider();
        var algorithm = new MaxRectsAlgorithm(colorProvider);

        var result = algorithm.Pack(items, PaperSize.A4, PackingConfiguration.Default);

        Assert.AreEqual(30, result.TotalItemsPlaced);
        Assert.IsTrue(result.OverallEfficiency > 0.5, $"Expected efficiency > 50%, got {result.OverallEfficiency:P1}");
    }

    [TestMethod]
    public void FullWorkflow_WithDifferentHeuristics_AllWork()
    {
        var items = new List<Item> { new(50, 50, 10) };

        foreach (var heuristic in Enum.GetValues<PackingHeuristic>())
        {
            var colorProvider = new RandomColorProvider();
            var algorithm = new MaxRectsAlgorithm(colorProvider, heuristic);

            var result = algorithm.Pack(items, PaperSize.A4, PackingConfiguration.Default);

            Assert.AreEqual(10, result.TotalItemsPlaced, $"Heuristic {heuristic} failed to place all items");
        }
    }

    [TestMethod]
    public void FullWorkflow_SaveAndLoadPdf_FileIsValid()
    {
        var items = new List<Item> { new(100, 50, 5) };

        var colorProvider = new RandomColorProvider();
        var algorithm = new MaxRectsAlgorithm(colorProvider);
        var pdfGenerator = new PdfGenerator();

        var result = algorithm.Pack(items, PaperSize.A4, PackingConfiguration.Default);

        var tempPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}.pdf");
        try
        {
            pdfGenerator.Generate(result, tempPath);

            Assert.IsTrue(File.Exists(tempPath));
            var fileInfo = new FileInfo(tempPath);
            Assert.IsTrue(fileInfo.Length > 0);

            // Read and verify PDF header
            var bytes = File.ReadAllBytes(tempPath);
            Assert.AreEqual((byte)'%', bytes[0]);
            Assert.AreEqual((byte)'P', bytes[1]);
            Assert.AreEqual((byte)'D', bytes[2]);
            Assert.AreEqual((byte)'F', bytes[3]);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [TestMethod]
    public void FullWorkflow_ItemParsing_RoundTrips()
    {
        var originalItems = new List<Item>
        {
            Item.Parse("100,50,3"),
            Item.Parse("75.5,25.5,2"),
            Item.Parse("50,50")
        };

        Assert.AreEqual(100, originalItems[0].Width);
        Assert.AreEqual(50, originalItems[0].Height);
        Assert.AreEqual(3, originalItems[0].Quantity);

        Assert.AreEqual(75.5, originalItems[1].Width);
        Assert.AreEqual(25.5, originalItems[1].Height);
        Assert.AreEqual(2, originalItems[1].Quantity);

        Assert.AreEqual(50, originalItems[2].Width);
        Assert.AreEqual(50, originalItems[2].Height);
        Assert.AreEqual(1, originalItems[2].Quantity); // Default quantity
    }
}
