
using InfinityPOS.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesReportsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    private const int PostedStatusId = 2;

    public SalesReportsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    private bool IsValidDateRange(DateTime fromDate, DateTime toDate)
    {
        return fromDate.Date <= toDate.Date;
    }

    // ============================================================
    // SALES SUMMARY
    // GET /api/SalesReports/summary
    // ============================================================

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        if (!IsValidDateRange(fromDate, toDate))
        {
            return BadRequest(new
            {
                message = "fromDate cannot be later than toDate."
            });
        }

        var start = fromDate.Date;
        var endExclusive = toDate.Date.AddDays(1);

        var result = await _context.SalesInvoices
            .AsNoTracking()
            .Where(x =>
                x.DocumentStatusId == PostedStatusId &&
                x.InvoiceDate >= start &&
                x.InvoiceDate < endExclusive)
            .GroupBy(x => x.CurrencyId)
            .Select(g => new
            {
                CurrencyId = g.Key,
                InvoiceCount = g.Count(),
                SubTotal = g.Sum(x => x.SubTotal),
                DiscountAmount = g.Sum(x => x.DiscountAmount),
                TaxAmount = g.Sum(x => x.TaxAmount),
                NetSales = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.CurrencyId)
            .ToListAsync();

        return Ok(new
        {
            FromDate = start,
            ToDate = toDate.Date,
            InvoiceStatus = "POSTED",
            CurrencySummaries = result
        });
    }

    // ============================================================
    // DAILY SALES - INVOICE AND PRODUCT DETAILS
    // GET /api/SalesReports/daily
    // ============================================================

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailySales(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        if (!IsValidDateRange(fromDate, toDate))
        {
            return BadRequest(new
            {
                message = "fromDate cannot be later than toDate."
            });
        }

        var start = fromDate.Date;
        var endExclusive = toDate.Date.AddDays(1);

        // Materialize invoices and their item details before
        // constructing the nested response.
        var invoiceEntities = await _context.SalesInvoices
            .AsNoTracking()
            .Where(x =>
                x.DocumentStatusId == PostedStatusId &&
                x.InvoiceDate >= start &&
                x.InvoiceDate < endExclusive)
            .Include(x => x.SalesInvoiceItems)
                .ThenInclude(i => i.Product)
            .OrderBy(x => x.InvoiceDate)
            .ThenBy(x => x.SalesInvoiceId)
            .ToListAsync();

        var invoices = invoiceEntities.Select(x => new
        {
            x.SalesInvoiceId,
            x.InvoiceNumber,
            x.InvoiceDate,
            x.CurrencyId,
            x.SubTotal,
            x.DiscountAmount,
            x.TaxAmount,
            x.TotalAmount,

            Products = x.SalesInvoiceItems
                .OrderBy(i => i.SalesInvoiceItemId)
                .Select(i => new
                {
                    i.SalesInvoiceItemId,
                    i.ProductId,
                    ProductName = i.Product.ProductName,
                    SKU = i.Product.Sku,
                    i.Quantity,
                    i.UnitPrice,
                    i.DiscountAmount,
                    i.TaxAmount,
                    LineTotal = i.TotalAmount ??
                        (i.Quantity * i.UnitPrice
                         - i.DiscountAmount
                         + i.TaxAmount)
                })
                .ToList()
        }).ToList();

        var dailyTotals = invoices
            .GroupBy(x => x.CurrencyId)
            .Select(g => new
            {
                CurrencyId = g.Key,
                InvoiceCount = g.Count(),
                ProductLineCount = g.Sum(x => x.Products.Count),
                TotalQuantity = g.Sum(x =>
                    x.Products.Sum(p => p.Quantity)),
                SubTotal = g.Sum(x => x.SubTotal),
                DiscountAmount = g.Sum(x => x.DiscountAmount),
                TaxAmount = g.Sum(x => x.TaxAmount),
                NetSales = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.CurrencyId)
            .ToList();

        return Ok(new
        {
            FromDate = start,
            ToDate = toDate.Date,
            InvoiceStatus = "POSTED",
            InvoiceCount = invoices.Count,
            InvoiceDetails = invoices,
            DailyTotals = dailyTotals
        });
    }

    // ============================================================
    // SALES ITEM REPORT - ALL POSTED VOUCHER PRODUCT LINES
    // GET /api/SalesReports/items
    // ============================================================

    [HttpGet("items")]
    public async Task<IActionResult> GetSalesItems(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        if (!IsValidDateRange(fromDate, toDate))
        {
            return BadRequest(new
            {
                message = "fromDate cannot be later than toDate."
            });
        }

        var start = fromDate.Date;
        var endExclusive = toDate.Date.AddDays(1);

        var items = await _context.SalesInvoices
            .AsNoTracking()
            .Where(invoice =>
                invoice.DocumentStatusId == PostedStatusId &&
                invoice.InvoiceDate >= start &&
                invoice.InvoiceDate < endExclusive)
            .SelectMany(invoice => invoice.SalesInvoiceItems.Select(item => new
            {
                SalesInvoiceItemId = item.SalesInvoiceItemId,
                SalesInvoiceId = invoice.SalesInvoiceId,
                InvoiceDate = invoice.InvoiceDate,
                InvoiceNumber = invoice.InvoiceNumber,
                CurrencyId = invoice.CurrencyId,

                ProductId = item.ProductId,
                SKU = item.Product.Sku,
                ProductName = item.Product.ProductName,

                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TaxAmount = item.TaxAmount,

                LineTotal = item.TotalAmount ??
                    (item.Quantity * item.UnitPrice
                     - item.DiscountAmount
                     + item.TaxAmount)
            }))
            .OrderBy(item => item.InvoiceDate)
            .ThenBy(item => item.InvoiceNumber)
            .ThenBy(item => item.SalesInvoiceItemId)
            .ToListAsync();

        var totals = items
            .GroupBy(item => item.CurrencyId)
            .Select(group => new
            {
                CurrencyId = group.Key,
                InvoiceCount = group
                    .Select(item => item.SalesInvoiceId)
                    .Distinct()
                    .Count(),
                ProductLineCount = group.Count(),
                TotalQuantity = group.Sum(item => item.Quantity),
                TotalDiscount = group.Sum(item => item.DiscountAmount),
                TotalTax = group.Sum(item => item.TaxAmount),
                NetSales = group.Sum(item => item.LineTotal)
            })
            .OrderBy(total => total.CurrencyId)
            .ToList();

        return Ok(new
        {
            FromDate = start,
            ToDate = toDate.Date,
            InvoiceStatus = "POSTED",
            ItemCount = items.Count,
            SalesItems = items,
            Totals = totals
        });
    }

    // ============================================================
    // PAYMENT SUMMARY
    // GET /api/SalesReports/payments
    // ============================================================

    [HttpGet("payments")]
    public async Task<IActionResult> GetPaymentSummary(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        if (!IsValidDateRange(fromDate, toDate))
        {
            return BadRequest(new
            {
                message = "fromDate cannot be later than toDate."
            });
        }

        var start = fromDate.Date;
        var endExclusive = toDate.Date.AddDays(1);

        var result = await (
            from payment in _context.SalesPayments.AsNoTracking()

            join invoice in _context.SalesInvoices.AsNoTracking()
                on payment.SalesInvoiceId
                equals invoice.SalesInvoiceId

            join method in _context.PaymentMethods.AsNoTracking()
                on payment.PaymentMethodId
                equals method.PaymentMethodId

            where
                invoice.DocumentStatusId == PostedStatusId &&
                payment.PaymentDate >= start &&
                payment.PaymentDate < endExclusive

            group payment by new
            {
                PaymentDate = payment.PaymentDate.Date,
                payment.PaymentMethodId,
                method.MethodCode,
                method.MethodName,
                invoice.CurrencyId
            }
            into g

            select new
            {
                PaymentDate = g.Key.PaymentDate,
                PaymentMethodId = g.Key.PaymentMethodId,
                PaymentMethodCode = g.Key.MethodCode,
                PaymentMethodName = g.Key.MethodName,
                CurrencyId = g.Key.CurrencyId,
                PaymentCount = g.Count(),
                ReceivedAmount = g.Sum(x => x.Amount)
            }
        )
        .OrderBy(x => x.PaymentDate)
        .ThenBy(x => x.PaymentMethodId)
        .ThenBy(x => x.CurrencyId)
        .ToListAsync();

        return Ok(new
        {
            FromDate = start,
            ToDate = toDate.Date,
            PaymentSummaries = result
        });
    }
}
