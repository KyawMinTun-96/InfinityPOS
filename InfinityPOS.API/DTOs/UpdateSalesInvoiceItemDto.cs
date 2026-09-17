namespace InfinityPOS.API.DTOs;

public class UpdateSalesInvoiceItemDto
{
    public long SalesInvoiceId { get; set; }
    public int ProductId { get; set; }
    public long? ProductPriceId { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
}