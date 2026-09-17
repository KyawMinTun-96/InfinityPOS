namespace InfinityPOS.API.DTOs;

public class CreateStockTransferDto
{
    public string TransferNumber { get; set; } = null!;

    public DateTime? TransferDate { get; set; }

    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }

    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }
}