namespace InfinityPOS.API.DTOs;

public class UpdateSalesInvoiceDto
{
    public string InvoiceNumber { get; set; } = null!;

    public DateTime InvoiceDate { get; set; }

    public int? CustomerId { get; set; }

    public int WarehouseId { get; set; }
    public int CurrencyId { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }
}