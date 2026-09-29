namespace InfinityPOS.API.DTOs;

public class PurchaseInvoiceDto
{
    public long PurchaseInvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public DateTime InvoiceDate { get; set; }

    public int? SupplierId { get; set; }

    public string? SupplierName { get; set; }

    public int WarehouseId { get; set; }

    public string? WarehouseName { get; set; }

    public int CurrencyId { get; set; }

    public string? CurrencyCode { get; set; }

    public string? CurrencyName { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public int DocumentStatusId { get; set; }

    public string? StatusCode { get; set; }

    public string? StatusName { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }
}
