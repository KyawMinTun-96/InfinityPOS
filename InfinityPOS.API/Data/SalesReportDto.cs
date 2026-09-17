namespace InfinityPOS.API.DTOs;

public class SalesReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public decimal TotalSales { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalCOGS { get; set; }
    public decimal GrossProfit { get; set; }

    public int InvoiceCount { get; set; }
    public decimal TotalQuantity { get; set; }
}