namespace InfinityPOS.API.DTOs;

public class StockBalanceDto
{
    public long StockBalanceId { get; set; }

    public int ProductId { get; set; }
    public int WarehouseId { get; set; }

    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }

    public DateTime UpdatedAt { get; set; }
}