namespace InfinityPOS.API.DTOs;

public class UpdatePurchaseInvoiceDto
{
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }

    public int? SupplierId { get; set; }
    public int WarehouseId { get; set; }
    public int CurrencyId { get; set; }

    public decimal ExchangeRate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public int DocumentStatusId { get; set; }
    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }
}