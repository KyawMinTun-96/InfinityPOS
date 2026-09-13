namespace InfinityPOS.API.DTOs;

public class CreateCategoryDto
{
    public string CategoryCode { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
}