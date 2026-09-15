namespace InfinityPOS.API.DTOs;

public class CreatePurchaseInvoiceDto
{
    public string InvoiceNumber { get; set; } = null!;
    public DateTime? InvoiceDate { get; set; }

    public int? SupplierId { get; set; }
    public int WarehouseId { get; set; }
    public int CurrencyId { get; set; }

    public decimal ExchangeRate { get; set; } = 1;
    public decimal SubTotal { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;

    public int DocumentStatusId { get; set; }
    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }
}