namespace InfinityPOS.API.DTOs;

public class UpdateProductPriceDto
{
    public int ProductId { get; set; }
    public int PriceTypeId { get; set; }
    public decimal Price { get; set; }
    public int CurrencyId { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}