namespace InfinityPOS.API.DTOs;

public class UpdateProductDto
{
    public string SKU { get; set; } = null!;

    public string? Barcode { get; set; }

    public string ProductName { get; set; } = null!;

    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int ProductTypeId { get; set; }

    public int UnitId { get; set; }

    public string? Description { get; set; }

    public bool TrackInventory { get; set; }

    public bool AllowNegativeStock { get; set; }

    public bool IsActive { get; set; }
}