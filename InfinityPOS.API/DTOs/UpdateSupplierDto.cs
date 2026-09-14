namespace InfinityPOS.API.DTOs;

public class UpdateSupplierDto
{
    public string SupplierCode { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}