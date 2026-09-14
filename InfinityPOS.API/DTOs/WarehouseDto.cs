namespace InfinityPOS.API.DTOs;

public class WarehouseDto
{
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = null!;
    public string WarehouseName { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}