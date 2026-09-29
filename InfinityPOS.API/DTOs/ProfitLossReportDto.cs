namespace InfinityPOS.API.DTOs;

public class ProfitLossReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public decimal TotalSales { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal NetSales { get; set; }

    public decimal TotalCOGS { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }

    public decimal OperatingExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public decimal NetProfitMargin { get; set; }

    public int SalesInvoiceCount { get; set; }
    public int ExpenseCount { get; set; }
}