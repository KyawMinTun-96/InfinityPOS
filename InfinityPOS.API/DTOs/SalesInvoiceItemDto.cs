namespace InfinityPOS.API.DTOs;

public class SalesInvoiceItemDto
{
    public long SalesInvoiceItemId { get; set; }

    public long SalesInvoiceId { get; set; }
    public int ProductId { get; set; }
    public long? ProductPriceId { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }

    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }

    public decimal? TotalAmount { get; set; }
    public decimal? COGSAmount { get; set; }
    public decimal? GrossProfit { get; set; }
}