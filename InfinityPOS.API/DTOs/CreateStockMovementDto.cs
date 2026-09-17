namespace InfinityPOS.API.DTOs;

public class CreateStockMovementDto
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public int StockMovementTypeId { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }

    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }

    public DateTime? MovementDate { get; set; }

    public string? Notes { get; set; }
    public int? CreatedByUserId { get; set; }
}