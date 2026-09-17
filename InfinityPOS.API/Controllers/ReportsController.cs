using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public ReportsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/reports/daily-summary?date=2026-09-18
    [HttpGet("daily-summary")]
    public async Task<ActionResult<DailySummaryDto>> GetDailySummary(
        [FromQuery] DateTime? date)
    {
        var reportDate = (date ?? DateTime.Today).Date;
        var nextDate = reportDate.AddDays(1);

        var sales = await _context.SalesInvoices
            .Where(x =>
                x.DocumentStatusId == 2 &&
                x.InvoiceDate >= reportDate &&
                x.InvoiceDate < nextDate)
            .ToListAsync();

        var purchases = await _context.PurchaseInvoices
            .Where(x =>
                x.DocumentStatusId == 5 &&
                x.InvoiceDate >= reportDate &&
                x.InvoiceDate < nextDate)
            .ToListAsync();

        var expenses = await _context.Expenses
            .Where(x =>
                x.DocumentStatusId == 2 &&
                x.ExpenseDate >= reportDate &&
                x.ExpenseDate < nextDate)
            .ToListAsync();

            var grossProfit = await _context.SalesInvoiceItems
            .Where(item =>
                item.SalesInvoice.DocumentStatusId == 2 &&
                item.SalesInvoice.InvoiceDate >= reportDate &&
                item.SalesInvoice.InvoiceDate < nextDate)
            .SumAsync(item => item.GrossProfit ?? 0);

        return Ok(new DailySummaryDto
        {
            Date = reportDate,

            Sales = sales.Sum(x => x.TotalAmount),

            Purchase = purchases.Sum(x => x.TotalAmount),

            Expenses = expenses.Sum(x => x.TotalAmount),

            Profit = grossProfit - expenses.Sum(x => x.TotalAmount),

            SalesInvoiceCount = sales.Count,

            PurchaseInvoiceCount = purchases.Count
        });
    }


        // GET: api/reports/sales?fromDate=2026-09-01&toDate=2026-09-17
    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> GetSalesReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var startDate = (fromDate ?? DateTime.Today).Date;
        var endDate = (toDate ?? startDate).Date;

        if (endDate < startDate)
        {
            return BadRequest(new
            {
                message = "toDate must be greater than or equal to fromDate."
            });
        }

        var endExclusive = endDate.AddDays(1);

        var invoices = await _context.SalesInvoices
            .Where(x =>
                x.DocumentStatusId == 2 &&
                x.InvoiceDate >= startDate &&
                x.InvoiceDate < endExclusive)
            .ToListAsync();

        var items = await _context.SalesInvoiceItems
            .Where(x =>
                x.SalesInvoice.DocumentStatusId == 2 &&
                x.SalesInvoice.InvoiceDate >= startDate &&
                x.SalesInvoice.InvoiceDate < endExclusive)
            .ToListAsync();

        return Ok(new SalesReportDto
        {    FromDate = startDate,
            ToDate = endDate,

            TotalSales = invoices.Sum(x => x.TotalAmount),

            TotalDiscount = invoices.Sum(x => x.DiscountAmount),

            TotalTax = invoices.Sum(x => x.TaxAmount),

            TotalCOGS = items.Sum(x => x.Quantity * x.UnitCost),

            GrossProfit = items.Sum(x => x.GrossProfit ?? 0),

            InvoiceCount = invoices.Count,

            TotalQuantity = items.Sum(x => x.Quantity)
        });
    }
}