namespace InfinityPOS.API.DTOs;

public class StockAdjustmentDto
{
    public long StockAdjustmentId { get; set; }

    public string AdjustmentNumber { get; set; } = null!;
    public DateTime AdjustmentDate { get; set; }

    public int WarehouseId { get; set; }
    public int StockAdjustmentReasonId { get; set; }
    public int DocumentStatusId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
}