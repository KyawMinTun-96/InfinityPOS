namespace InfinityPOS.API.DTOs;

public class CreateBrandDto
{
    public string BrandCode { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public string? Description { get; set; }
}