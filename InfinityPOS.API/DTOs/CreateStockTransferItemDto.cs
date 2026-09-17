namespace InfinityPOS.API.DTOs;

public class CreateStockTransferItemDto
{
    public long StockTransferId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }
}