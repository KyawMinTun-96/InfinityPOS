namespace InfinityPOS.API.DTOs;

public class UpdateWarehouseDto
{
    public string WarehouseCode { get; set; } = null!;
    public string WarehouseName { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}