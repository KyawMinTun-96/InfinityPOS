namespace InfinityPOS.API.DTOs;

public class DailySummaryDto
{
    public DateTime Date { get; set; }

    public decimal Sales { get; set; }

    public decimal Purchase { get; set; }

    public decimal Expenses { get; set; }

    public decimal Profit { get; set; }

    public int SalesInvoiceCount { get; set; }

    public int PurchaseInvoiceCount { get; set; }
}