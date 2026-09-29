using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public DashboardController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/dashboard?date=2026-09-18
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard(
        [FromQuery] DateTime? date)
    {
        var dashboardDate = (date ?? DateTime.Today).Date;
        var nextDate = dashboardDate.AddDays(1);

        // =========================
        // Posted Sales
        // =========================
        var salesInvoices = await _context.SalesInvoices
            .Where(x =>
                x.DocumentStatusId == 2 &&
                x.InvoiceDate >= dashboardDate &&
                x.InvoiceDate < nextDate)
            .ToListAsync();

        // =========================
        // Posted Sales Items
        // =========================
        var salesItems = await _context.SalesInvoiceItems
            .Where(x =>
                x.SalesInvoice.DocumentStatusId == 2 &&
                x.SalesInvoice.InvoiceDate >= dashboardDate &&
                x.SalesInvoice.InvoiceDate < nextDate)
            .ToListAsync();

        // =========================
        // Posted Purchases
        // =========================
        var purchaseInvoices = await _context.PurchaseInvoices
            .Where(x =>
                x.DocumentStatusId == 5 &&
                x.InvoiceDate >= dashboardDate &&
                x.InvoiceDate < nextDate)
            .ToListAsync();

        // =========================
        // Posted Operating Expenses
        // =========================
        var expenses = await _context.Expenses
            .Where(x =>
                x.DocumentStatusId == 2 &&
                x.ExpenseDate >= dashboardDate &&
                x.ExpenseDate < nextDate)
            .ToListAsync();

        // =========================
        // Inventory
        // =========================
        var stockBalances = await _context.StockBalances
            .Where(x => x.Quantity != 0)
            .ToListAsync();

        // =========================
        // Recent Sales
        // =========================
        var recentSales = await _context.SalesInvoices
            .Include(x => x.Customer)
            .Where(x => x.DocumentStatusId == 2)
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.SalesInvoiceId)
            .Take(10)
            .ToListAsync();

        // =========================
        // Calculations
        // =========================

        var todaySales = salesInvoices.Sum(x => x.TotalAmount);

        var todayPurchase = purchaseInvoices.Sum(
            x => x.TotalAmount);

        var todayGrossProfit = salesItems.Sum(
            x => x.GrossProfit ?? 0);

        var todayOperatingExpenses = expenses.Sum(
            x => x.TotalAmount);

        var todayNetProfit =
            todayGrossProfit - todayOperatingExpenses;

        var totalStockValue = stockBalances.Sum(
            x => x.Quantity * x.AverageCost);

        return Ok(new DashboardDto
        {
            Date = dashboardDate,

            TodaySales = todaySales,
            TodayPurchase = todayPurchase,
            TodayGrossProfit = todayGrossProfit,
            TodayOperatingExpenses = todayOperatingExpenses,
            TodayNetProfit = todayNetProfit,

            TodaySalesInvoiceCount = salesInvoices.Count,
            TodayPurchaseInvoiceCount = purchaseInvoices.Count,

            TotalStockValue = totalStockValue,

            // Low stock will be implemented
            // when minimum/reorder stock setting is added.
            LowStockCount = 0,

            RecentSales = recentSales
                .Select(x => new DashboardRecentSaleDto
                {
                    SalesInvoiceId = x.SalesInvoiceId,
                    InvoiceNumber = x.InvoiceNumber,
                    InvoiceDate = x.InvoiceDate,

                    CustomerName =
                        x.Customer != null
                            ? x.Customer.CustomerName
                            : "Walk-in Customer",

                    TotalAmount = x.TotalAmount
                })
                .ToList()
        });
    }
}