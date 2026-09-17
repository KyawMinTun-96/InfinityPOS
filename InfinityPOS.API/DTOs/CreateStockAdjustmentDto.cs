namespace InfinityPOS.API.DTOs;

public class CreateStockAdjustmentDto
{
    public string AdjustmentNumber { get; set; } = null!;

    public DateTime? AdjustmentDate { get; set; }

    public int WarehouseId { get; set; }
    public int StockAdjustmentReasonId { get; set; }

    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }
}