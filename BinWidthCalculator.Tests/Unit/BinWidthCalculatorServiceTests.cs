using BinWidthCalculator.Application.Services;
using BinWidthCalculator.Domain.Entities;
using FluentAssertions;

namespace BinWidthCalculator.Tests.Unit;

public class BinWidthCalculatorServiceTests
{
    private readonly BinWidthCalculatorService _calculator = new();

    [Fact]
    public void CalculateRequiredBinWidth_EmptyItems_ReturnsZero()
    {
        var items = new List<OrderItem>();

        var result = _calculator.CalculateRequiredBinWidth(items);

        result.Should().Be(0);
    }

    [Fact]
    public void CalculateRequiredBinWidth_SinglePhotoBook_ReturnsCorrectWidth()
    {
        var items = new List<OrderItem>
        {
            new(ProductType.PhotoBook, 1)
        };

        var result = _calculator.CalculateRequiredBinWidth(items);

        result.Should().Be(19.0m);
    }

    [Fact]
    public void CalculateRequiredBinWidth_MultipleProducts_ReturnsCorrectWidth()
    {
        var items = new List<OrderItem>
        {
            new(ProductType.PhotoBook, 1),  // 19mm
            new(ProductType.Calendar, 2),   // 20mm
            new(ProductType.Canvas, 1)      // 16mm
        };

        var result = _calculator.CalculateRequiredBinWidth(items);

        result.Should().Be(55.0m); // 19 + 20 + 16
    }

    [Fact]
    public void CalculateRequiredBinWidth_Mugs_StacksCorrectly()
    {
        var items = new List<OrderItem>
        {
            new(ProductType.Mug, 4)  // 1 stack of 4 mugs = 94mm
        };

        var result = _calculator.CalculateRequiredBinWidth(items);

        result.Should().Be(94.0m);
    }

    [Fact]
    public void CalculateRequiredBinWidth_Mugs_RequiresExtraStack()
    {
        var items = new List<OrderItem>
        {
            new(ProductType.Mug, 5)  // 2 stacks = 188mm
        };

        var result = _calculator.CalculateRequiredBinWidth(items);

        result.Should().Be(188.0m);
    }

    [Fact]
    public void CalculateRequiredBinWidth_ComplexOrder_ReturnsCorrectWidth()
    {
        var items = new List<OrderItem>
        {
            new(ProductType.PhotoBook, 1),  // 19mm
            new(ProductType.Calendar, 2),   // 20mm
            new(ProductType.Canvas, 1),     // 16mm
            new(ProductType.Cards, 3),      // 14.1mm
            new(ProductType.Mug, 7)         // 2 stacks = 188mm
        };

        var result = _calculator.CalculateRequiredBinWidth(items);

        result.Should().Be(257.1m); // 19 + 20 + 16 + 14.1 + 188
    }
}