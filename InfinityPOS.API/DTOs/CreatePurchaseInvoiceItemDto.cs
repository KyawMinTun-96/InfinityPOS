namespace InfinityPOS.API.DTOs;

public class CreatePurchaseInvoiceItemDto
{
    public long PurchaseInvoiceId { get; set; }
    public int ProductId { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }

    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
}