namespace InfinityPOS.API.DTOs;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryCode { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}