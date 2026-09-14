namespace InfinityPOS.API.DTOs;

public class CreateWarehouseDto
{
    public string WarehouseCode { get; set; } = null!;
    public string WarehouseName { get; set; } = null!;
    public string? Address { get; set; }
}