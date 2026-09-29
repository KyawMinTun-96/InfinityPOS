namespace InfinityPOS.API.DTOs;

public class InventoryReportItemDto
{
    public long StockBalanceId { get; set; }

    public int ProductId { get; set; }
    public string SKU { get; set; } = null!;
    public string ProductName { get; set; } = null!;

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal StockValue { get; set; }
}

public class InventoryReportDto
{
    public DateTime GeneratedAt { get; set; }

    public decimal TotalStockValue { get; set; }

    public List<InventoryReportItemDto> Items { get; set; } = new();
}