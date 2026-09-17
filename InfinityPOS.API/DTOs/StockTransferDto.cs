namespace InfinityPOS.API.DTOs;

public class StockTransferDto
{
    public long StockTransferId { get; set; }

    public string TransferNumber { get; set; } = null!;
    public DateTime TransferDate { get; set; }

    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }

    public int DocumentStatusId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
}