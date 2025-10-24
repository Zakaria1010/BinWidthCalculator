using BinWidthCalculator.Domain.Entities;

namespace BinWidthCalculator.Domain.Interfaces;

public interface IBinWidthCalculator
{
    decimal CalculateRequiredBinWidth(List<OrderItem> items);
}