namespace InfinityPOS.API.DTOs;

public class StockMovementReportItemDto
{
    public long StockMovementId { get; set; }

    public DateTime MovementDate { get; set; }

    public int ProductId { get; set; }
    public string Sku { get; set; } = null!;
    public string ProductName { get; set; } = null!;

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;

    public int StockMovementTypeId { get; set; }
    public string MovementTypeCode { get; set; } = null!;
    public string Direction { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }

    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }

    public string? Notes { get; set; }
}

public class StockMovementReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public int MovementCount { get; set; }

    public decimal TotalInQuantity { get; set; }
    public decimal TotalOutQuantity { get; set; }

    public List<StockMovementReportItemDto> Items { get; set; } = new();
}