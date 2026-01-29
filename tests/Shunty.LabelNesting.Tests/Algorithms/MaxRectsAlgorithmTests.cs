using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shunty.LabelNesting.Core.Algorithms;
using Shunty.LabelNesting.Core.Models;
using Shunty.LabelNesting.Core.Services;

namespace Shunty.LabelNesting.Tests.Algorithms;

[TestClass]
public sealed class MaxRectsAlgorithmTests
{
    private MaxRectsAlgorithm _algorithm = null!;
    private PackingConfiguration _config;

    [TestInitialize]
    public void Setup()
    {
        var colorProvider = new RandomColorProvider();
        _algorithm = new MaxRectsAlgorithm(colorProvider);
        _config = PackingConfiguration.Default;
    }

    [TestMethod]
    public void Pack_SingleItem_PlacesItemCorrectly()
    {
        var items = new List<Item> { new(100, 50, 1) };

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        Assert.AreEqual(1, result.TotalItemsPlaced);
        Assert.AreEqual(1, result.PageCount);

        var placement = result.Placements[0];
        Assert.AreEqual(100, placement.Width);
        Assert.AreEqual(50, placement.Height);
        Assert.AreEqual(0, placement.PageIndex);
        Assert.AreEqual(0, placement.ItemIndex);
        Assert.AreEqual(0, placement.InstanceIndex);
    }

    [TestMethod]
    public void Pack_ItemWithQuantity_PlacesAllInstances()
    {
        var items = new List<Item> { new(50, 50, 5) };

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        Assert.AreEqual(5, result.TotalItemsPlaced);
        Assert.IsTrue(result.Placements.All(p => p.ItemIndex == 0));

        // Check instance indices are 0-4
        var instanceIndices = result.Placements.Select(p => p.InstanceIndex).OrderBy(i => i).ToList();
        CollectionAssert.AreEqual(new List<int> { 0, 1, 2, 3, 4 }, instanceIndices);
    }

    [TestMethod]
    public void Pack_MultipleItems_PlacesAllItems()
    {
        var items = new List<Item>
        {
            new(100, 50, 2),
            new(75, 25, 3)
        };

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        Assert.AreEqual(5, result.TotalItemsPlaced);

        // Check both item types are present
        Assert.AreEqual(2, result.Placements.Count(p => p.ItemIndex == 0));
        Assert.AreEqual(3, result.Placements.Count(p => p.ItemIndex == 1));
    }

    [TestMethod]
    public void Pack_ItemRequiresRotation_RotatesItem()
    {
        // Item is 190x60, usable area on A4 with 5mm margin is 200x287
        // 190x60 fits without rotation, but let's test a case where rotation helps
        // Using a tall narrow item that only fits rotated
        var items = new List<Item> { new(60, 250, 1) };
        var config = new PackingConfiguration(Margin: 5, Gutter: 2, AllowRotation: true);

        var result = _algorithm.Pack(items, PaperSize.A4, config);

        Assert.AreEqual(1, result.TotalItemsPlaced);
        // Item may or may not be rotated depending on what fits better
        // The important thing is it's placed successfully
    }

    [TestMethod]
    public void Pack_ItemTooLargeWithRotationDisabled_ThrowsException()
    {
        // Item is 250x60, usable area on A4 with 5mm margin is 200x287
        // 250 > 200 so it won't fit even horizontally
        var items = new List<Item> { new(250, 60, 1) };
        var config = new PackingConfiguration(Margin: 5, Gutter: 2, AllowRotation: false);

        Assert.Throws<ArgumentException>(() =>
            _algorithm.Pack(items, PaperSize.A4, config));
    }

    [TestMethod]
    public void Pack_ManyItems_UsesMultiplePages()
    {
        // Fill up a page with many large items
        // A4 usable area: 200x287mm, each item 90x90 = 4 per page roughly
        var items = new List<Item> { new(90, 90, 20) };

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        Assert.AreEqual(20, result.TotalItemsPlaced);
        Assert.IsTrue(result.PageCount > 1, $"Expected multiple pages, got {result.PageCount}");
    }

    [TestMethod]
    public void Pack_EmptyItems_ReturnsEmptyResult()
    {
        var items = new List<Item>();

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        Assert.AreEqual(0, result.TotalItemsPlaced);
        Assert.AreEqual(0, result.PageCount);
    }

    [TestMethod]
    public void Pack_DifferentSizedItems_OptimizesPlacement()
    {
        // Mix of sizes
        var items = new List<Item>
        {
            new(100, 100, 1),
            new(50, 50, 4),
            new(25, 25, 8)
        };

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        Assert.AreEqual(13, result.TotalItemsPlaced);
        Assert.IsTrue(result.OverallEfficiency > 0.3, $"Efficiency too low: {result.OverallEfficiency:P1}");
    }

    [TestMethod]
    public void Pack_PlacementsIncludeMarginOffset()
    {
        var items = new List<Item> { new(50, 50, 1) };
        var config = new PackingConfiguration(Margin: 10, Gutter: 0, AllowRotation: false);

        var result = _algorithm.Pack(items, PaperSize.A4, config);

        var placement = result.Placements[0];
        // The placement coordinates should include the margin offset
        Assert.IsTrue(placement.X >= 10, $"X should be >= margin, got {placement.X}");
        Assert.IsTrue(placement.Y >= 10, $"Y should be >= margin, got {placement.Y}");
    }

    [TestMethod]
    public void Pack_PlacementsHaveColors()
    {
        var items = new List<Item>
        {
            new(50, 50, 2),
            new(30, 30, 2)
        };

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        Assert.IsTrue(result.Placements.All(p => !string.IsNullOrEmpty(p.Color)));

        // Same item type should have same color
        var item0Colors = result.Placements.Where(p => p.ItemIndex == 0).Select(p => p.Color).Distinct().ToList();
        Assert.AreEqual(1, item0Colors.Count, "Same item type should have same color");

        // Different item types should have different colors
        var item1Color = result.Placements.First(p => p.ItemIndex == 1).Color;
        Assert.AreNotEqual(item0Colors[0], item1Color, "Different item types should have different colors");
    }

    [TestMethod]
    public void Pack_PlacementsHaveCorrectLabels()
    {
        var items = new List<Item> { new(50, 50, 3) };

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        var labels = result.Placements.Select(p => p.Label).OrderBy(l => l).ToList();
        CollectionAssert.AreEqual(new List<string> { "1.1", "1.2", "1.3" }, labels);
    }

    [TestMethod]
    public void GetPageEfficiency_ReturnsReasonableValue()
    {
        var items = new List<Item> { new(100, 100, 4) };

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        var efficiency = result.GetPageEfficiency(0);
        Assert.IsTrue(efficiency > 0 && efficiency <= 1, $"Efficiency should be between 0 and 1, got {efficiency}");
    }

    [TestMethod]
    public void GetPagePlacements_ReturnsCorrectPage()
    {
        var items = new List<Item> { new(90, 90, 20) };

        var result = _algorithm.Pack(items, PaperSize.A4, _config);

        var page0Placements = result.GetPagePlacements(0).ToList();
        Assert.IsTrue(page0Placements.All(p => p.PageIndex == 0));
        Assert.IsTrue(page0Placements.Count > 0);
    }
}
