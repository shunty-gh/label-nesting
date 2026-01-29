using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shunty.LabelNesting.Core.Models;

namespace Shunty.LabelNesting.Tests.Models;

[TestClass]
public sealed class PaperSizeTests
{
    [TestMethod]
    [DataRow("A4", 210, 297)]
    [DataRow("a4", 210, 297)]
    [DataRow("A3", 297, 420)]
    [DataRow("A2", 420, 594)]
    [DataRow("A5", 148, 210)]
    [DataRow("A6", 105, 148)]
    public void Parse_StandardSizes_ReturnsCorrectDimensions(string input, double expectedWidth, double expectedHeight)
    {
        var result = PaperSize.Parse(input);

        Assert.AreEqual(expectedWidth, result.Width);
        Assert.AreEqual(expectedHeight, result.Height);
    }

    [TestMethod]
    [DataRow("200x300", 200, 300)]
    [DataRow("100X150", 100, 150)]
    [DataRow("50.5x75.5", 50.5, 75.5)]
    public void Parse_CustomSize_ReturnsCorrectDimensions(string input, double expectedWidth, double expectedHeight)
    {
        var result = PaperSize.Parse(input);

        Assert.AreEqual(expectedWidth, result.Width);
        Assert.AreEqual(expectedHeight, result.Height);
        Assert.AreEqual("Custom", result.Name);
    }

    [TestMethod]
    [DataRow("invalid")]
    [DataRow("A7")]
    [DataRow("x100")]
    [DataRow("100x")]
    [DataRow("-100x200")]
    public void Parse_InvalidInput_ThrowsFormatException(string input)
    {
        Assert.Throws<FormatException>(() => PaperSize.Parse(input));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Parse_EmptyInput_ThrowsArgumentException(string input)
    {
        Assert.Throws<ArgumentException>(() => PaperSize.Parse(input));
    }

    [TestMethod]
    public void TryParse_ValidInput_ReturnsTrue()
    {
        Assert.IsTrue(PaperSize.TryParse("A4", out var result));
        Assert.AreEqual(PaperSize.A4, result);
    }

    [TestMethod]
    public void TryParse_InvalidInput_ReturnsFalse()
    {
        Assert.IsFalse(PaperSize.TryParse("invalid", out _));
    }

    [TestMethod]
    public void StandardSizes_ContainsAllExpectedSizes()
    {
        Assert.AreEqual(5, PaperSize.StandardSizes.Count);
        CollectionAssert.Contains(PaperSize.StandardSizes.ToList(), PaperSize.A2);
        CollectionAssert.Contains(PaperSize.StandardSizes.ToList(), PaperSize.A3);
        CollectionAssert.Contains(PaperSize.StandardSizes.ToList(), PaperSize.A4);
        CollectionAssert.Contains(PaperSize.StandardSizes.ToList(), PaperSize.A5);
        CollectionAssert.Contains(PaperSize.StandardSizes.ToList(), PaperSize.A6);
    }

    [TestMethod]
    public void Area_ReturnsCorrectValue()
    {
        var paper = PaperSize.Custom(100, 200);
        Assert.AreEqual(20000, paper.Area);
    }

    [TestMethod]
    public void ToString_StandardSize_ReturnsName()
    {
        Assert.AreEqual("A4", PaperSize.A4.ToString());
    }

    [TestMethod]
    public void ToString_CustomSize_ReturnsDimensions()
    {
        var paper = PaperSize.Custom(200, 300);
        Assert.AreEqual("200x300mm", paper.ToString());
    }
}
