namespace InfinityPOS.API.DTOs;

public class PurchaseReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public decimal TotalPurchase { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }

    public int InvoiceCount { get; set; }
    public decimal TotalQuantity { get; set; }
}