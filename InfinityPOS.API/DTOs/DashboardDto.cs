namespace InfinityPOS.API.DTOs;

public class DashboardRecentSaleDto
{
    public long SalesInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
}

public class DashboardDto
{
    public DateTime Date { get; set; }

    public decimal TodaySales { get; set; }
    public decimal TodayPurchase { get; set; }
    public decimal TodayGrossProfit { get; set; }
    public decimal TodayOperatingExpenses { get; set; }
    public decimal TodayNetProfit { get; set; }

    public int TodaySalesInvoiceCount { get; set; }
    public int TodayPurchaseInvoiceCount { get; set; }

    public decimal TotalStockValue { get; set; }
    public int LowStockCount { get; set; }

    public List<DashboardRecentSaleDto> RecentSales { get; set; } = new();
}