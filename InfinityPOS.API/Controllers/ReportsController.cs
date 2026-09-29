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

        // GET: api/reports/purchases?fromDate=2026-09-01&toDate=2026-09-18
    [HttpGet("purchases")]
    public async Task<ActionResult<PurchaseReportDto>> GetPurchaseReport(
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

        var invoices = await _context.PurchaseInvoices
            .Where(x =>
                x.DocumentStatusId == 5 &&
                x.InvoiceDate >= startDate &&
                x.InvoiceDate < endExclusive)
            .ToListAsync();

        var items = await _context.PurchaseInvoiceItems
            .Where(x =>
                x.PurchaseInvoice.DocumentStatusId == 5 &&
                x.PurchaseInvoice.InvoiceDate >= startDate &&
                x.PurchaseInvoice.InvoiceDate < endExclusive)
            .ToListAsync();

        return Ok(new PurchaseReportDto
        {
            FromDate = startDate,
            ToDate = endDate,

            TotalPurchase = invoices.Sum(x => x.TotalAmount),

            TotalDiscount = invoices.Sum(x => x.DiscountAmount),

            TotalTax = invoices.Sum(x => x.TaxAmount),

            InvoiceCount = invoices.Count,

            TotalQuantity = items.Sum(x => x.Quantity)
        });
    }


        // GET: api/reports/inventory
    [HttpGet("inventory")]
    public async Task<ActionResult<InventoryReportDto>> GetInventoryReport()
    {
        var stockBalances = await _context.StockBalances
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .Where(x => x.Quantity != 0)
            .OrderBy(x => x.Product.ProductName)
            .ThenBy(x => x.Warehouse.WarehouseName)
            .ToListAsync();

        var items = stockBalances
            .Select(x => new InventoryReportItemDto
            {
                StockBalanceId = x.StockBalanceId,

                ProductId = x.ProductId,
                SKU = x.Product.Sku,
                ProductName = x.Product.ProductName,

                WarehouseId = x.WarehouseId,
                WarehouseName = x.Warehouse.WarehouseName,

                Quantity = x.Quantity,
                AverageCost = x.AverageCost,
                StockValue = x.Quantity * x.AverageCost
            })
            .ToList();

        return Ok(new InventoryReportDto
        {
            GeneratedAt = DateTime.Now,

            TotalStockValue = items.Sum(x => x.StockValue),

            Items = items
        });
    }

        // GET: api/reports/stock-movements?fromDate=2026-09-01&toDate=2026-09-18
    [HttpGet("stock-movements")]
    public async Task<ActionResult<StockMovementReportDto>> GetStockMovementReport(
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

        var movements = await _context.StockMovements
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .Include(x => x.StockMovementType)
            .Where(x =>
                x.MovementDate >= startDate &&
                x.MovementDate < endExclusive)
            .OrderByDescending(x => x.MovementDate)
            .ThenByDescending(x => x.StockMovementId)
            .ToListAsync();

        var items = movements
            .Select(x => new StockMovementReportItemDto
            {
                StockMovementId = x.StockMovementId,

                MovementDate = x.MovementDate,

                ProductId = x.ProductId,
                Sku = x.Product.Sku,
                ProductName = x.Product.ProductName,

                WarehouseId = x.WarehouseId,
                WarehouseName = x.Warehouse.WarehouseName,

                StockMovementTypeId = x.StockMovementTypeId,
                MovementTypeCode = x.StockMovementType.MovementCode,
                Direction = x.StockMovementType.Direction.Trim().ToUpperInvariant(),

                Quantity = x.Quantity,
                UnitCost = x.UnitCost,

                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,

                Notes = x.Notes
            })
            .ToList();

        return Ok(new StockMovementReportDto
        {
            FromDate = startDate,
            ToDate = endDate,

            MovementCount = items.Count(),

            TotalInQuantity = items
                .Where(x => x.Direction == "IN")
                .Sum(x => x.Quantity),

            TotalOutQuantity = items
                .Where(x => x.Direction == "OUT")
                .Sum(x => x.Quantity),

            Items = items
        });
    }

    // GET: api/reports/profit-loss?fromDate=2026-09-01&toDate=2026-09-18
[HttpGet("profit-loss")]
public async Task<ActionResult<ProfitLossReportDto>> GetProfitLossReport(
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

    // Posted Sales
    var salesInvoices = await _context.SalesInvoices
        .Where(x =>
            x.DocumentStatusId == 2 &&
            x.InvoiceDate >= startDate &&
            x.InvoiceDate < endExclusive)
        .ToListAsync();

    // Posted Sales Items
    var salesItems = await _context.SalesInvoiceItems
        .Where(x =>
            x.SalesInvoice.DocumentStatusId == 2 &&
            x.SalesInvoice.InvoiceDate >= startDate &&
            x.SalesInvoice.InvoiceDate < endExclusive)
        .ToListAsync();

    // Posted Operating Expenses
    var expenses = await _context.Expenses
        .Where(x =>
            x.DocumentStatusId == 2 &&
            x.ExpenseDate >= startDate &&
            x.ExpenseDate < endExclusive)
        .ToListAsync();

    var totalSales = salesInvoices.Sum(x => x.TotalAmount);
    var totalDiscount = salesInvoices.Sum(x => x.DiscountAmount);
    var totalTax = salesInvoices.Sum(x => x.TaxAmount);

    // Sales after tax
    var netSales = totalSales - totalTax;

    // COGS = Quantity × UnitCost
    var totalCOGS = salesItems.Sum(x => x.Quantity * x.UnitCost);

    var grossProfit = netSales - totalCOGS;

    var operatingExpenses = expenses.Sum(x => x.TotalAmount);

    var netProfit = grossProfit - operatingExpenses;

    var grossProfitMargin = netSales > 0
        ? (grossProfit / netSales) * 100
        : 0;

    var netProfitMargin = netSales > 0
        ? (netProfit / netSales) * 100
        : 0;

    return Ok(new ProfitLossReportDto
    {
        FromDate = startDate,
        ToDate = endDate,

        TotalSales = totalSales,
        TotalDiscount = totalDiscount,
        TotalTax = totalTax,
        NetSales = netSales,

        TotalCOGS = totalCOGS,
        GrossProfit = grossProfit,
        GrossProfitMargin = grossProfitMargin,

        OperatingExpenses = operatingExpenses,
        NetProfit = netProfit,
        NetProfitMargin = netProfitMargin,

        SalesInvoiceCount = salesInvoices.Count,
        ExpenseCount = expenses.Count
    });
}
}