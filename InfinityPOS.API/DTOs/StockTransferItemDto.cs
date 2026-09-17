namespace InfinityPOS.API.DTOs;

public class StockTransferItemDto
{
    public long StockTransferItemId { get; set; }

    public long StockTransferId { get; set; }
    public int ProductId { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}