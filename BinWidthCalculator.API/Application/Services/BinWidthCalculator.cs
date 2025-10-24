using BinWidthCalculator.Domain.Entities;
using BinWidthCalculator.Domain.Interfaces;

namespace BinWidthCalculator.Application.Services;

public class BinWidthCalculatorService : IBinWidthCalculator
{
    private const decimal PHOTOBOOK_WIDTH = 19.0m;
    private const decimal CALENDAR_WIDTH = 10.0m;
    private const decimal CANVAS_WIDTH = 16.0m;
    private const decimal CARDS_WIDTH = 4.7m;
    private const decimal MUG_WIDTH = 94.0m;
    private const int MUGS_PER_STACK = 4;

    public decimal CalculateRequiredBinWidth(List<OrderItem> items)
    {
        if (items == null || !items.Any())
            return 0;

        decimal totalWidth = 0;

        foreach (var item in items)
        {
            switch (item.ProductType)
            {
                case ProductType.PhotoBook:
                    totalWidth += item.Quantity * PHOTOBOOK_WIDTH;
                    break;
                case ProductType.Calendar:
                    totalWidth += item.Quantity * CALENDAR_WIDTH;
                    break;
                case ProductType.Canvas:
                    totalWidth += item.Quantity * CANVAS_WIDTH;
                    break;
                case ProductType.Cards:
                    totalWidth += item.Quantity * CARDS_WIDTH;
                    break;
                case ProductType.Mug:
                    // Mugs can be stacked (4 per stack)
                    var mugStacks = (int)Math.Ceiling((double)item.Quantity / MUGS_PER_STACK);
                    totalWidth += mugStacks * MUG_WIDTH;
                    break;
                default:
                    throw new ArgumentException($"Unknown product type: {item.ProductType}");
            }
        }

        return Math.Round(totalWidth, 2);
    }
}