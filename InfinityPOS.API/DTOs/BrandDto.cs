namespace InfinityPOS.API.DTOs;

public class BrandDto
{
    public int BrandId { get; set; }
    public string BrandCode { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}