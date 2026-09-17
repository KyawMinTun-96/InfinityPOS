namespace InfinityPOS.API.DTOs;

public class CreateStockAdjustmentItemDto
{
    public long StockAdjustmentId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public bool IsIncrease { get; set; }

    public string? Notes { get; set; }
}