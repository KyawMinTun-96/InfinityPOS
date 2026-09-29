using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesInvoicesController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public SalesInvoicesController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GENERATE SALES VOUCHER NUMBER
    // ============================================================
    //
    // Format:
    //
    // SV + yy + MM + dd + HH + mm + daily sequence
    //
    // Example:
    //
    // SV260930011701
    //
    // SV = Sales Voucher
    // 26 = Year
    // 09 = Month
    // 30 = Day
    // 01 = Hour
    // 17 = Minute
    // 01 = Daily Sequence
    //
    // NO DASH (-)
    // ============================================================

    private async Task<string> GenerateSalesVoucherNumberAsync(
        DateTime invoiceDate)
    {
        var dayStart = invoiceDate.Date;
        var dayEnd = dayStart.AddDays(1);

        var existingNumbers = await _context.SalesInvoices
            .AsNoTracking()
            .Where(x =>
                x.InvoiceDate >= dayStart &&
                x.InvoiceDate < dayEnd &&
                x.InvoiceNumber.StartsWith("SV"))
            .Select(x => x.InvoiceNumber)
            .ToListAsync();

        var maxSequence = 0;

        foreach (var number in existingNumbers)
        {
            if (string.IsNullOrWhiteSpace(number))
                continue;

            if (number.Length < 2)
                continue;

            var lastTwoDigits = number[^2..];

            if (int.TryParse(
                lastTwoDigits,
                out var sequence))
            {
                if (sequence > maxSequence)
                {
                    maxSequence = sequence;
                }
            }
        }

        var nextSequence = maxSequence + 1;

        if (nextSequence > 99)
        {
            throw new InvalidOperationException(
                "Daily sales voucher sequence has reached 99."
            );
        }

        return $"SV{invoiceDate:yyMMddHHmm}{nextSequence:00}";
    }

    // ============================================================
    // GET ALL SALES INVOICES
    // ============================================================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesInvoiceDto>>> GetAll()
    {
        var invoices = await _context.SalesInvoices
            .AsNoTracking()
            .OrderByDescending(x => x.SalesInvoiceId)
            .Select(x => new SalesInvoiceDto
            {
                SalesInvoiceId = x.SalesInvoiceId,

                InvoiceNumber = x.InvoiceNumber,

                InvoiceDate = x.InvoiceDate,

                CustomerId = x.CustomerId,

                WarehouseId = x.WarehouseId,

                CurrencyId = x.CurrencyId,

                ExchangeRate = x.ExchangeRate,

                SubTotal = x.SubTotal,

                DiscountAmount = x.DiscountAmount,

                TaxAmount = x.TaxAmount,

                TotalAmount = x.TotalAmount,

                DocumentStatusId = x.DocumentStatusId,

                Notes = x.Notes,

                CreatedAt = x.CreatedAt,

                CreatedByUserId = x.CreatedByUserId
            })
            .ToListAsync();

        return Ok(invoices);
    }

    // ============================================================
    // GET SALES INVOICE BY ID
    // ============================================================

    [HttpGet("{id:long}")]
    public async Task<ActionResult<SalesInvoiceDto>> GetById(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid SalesInvoiceId."
            });
        }

        var invoice = await _context.SalesInvoices
            .AsNoTracking()
            .Where(x => x.SalesInvoiceId == id)
            .Select(x => new SalesInvoiceDto
            {
                SalesInvoiceId = x.SalesInvoiceId,

                InvoiceNumber = x.InvoiceNumber,

                InvoiceDate = x.InvoiceDate,

                CustomerId = x.CustomerId,

                WarehouseId = x.WarehouseId,

                CurrencyId = x.CurrencyId,

                ExchangeRate = x.ExchangeRate,

                SubTotal = x.SubTotal,

                DiscountAmount = x.DiscountAmount,

                TaxAmount = x.TaxAmount,

                TotalAmount = x.TotalAmount,

                DocumentStatusId = x.DocumentStatusId,

                Notes = x.Notes,

                CreatedAt = x.CreatedAt,

                CreatedByUserId = x.CreatedByUserId
            })
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Sales invoice not found."
            });
        }

        return Ok(invoice);
    }

    // ============================================================
    // CREATE SALES INVOICE
    // ============================================================

    [HttpPost]
    public async Task<ActionResult<SalesInvoiceDto>> Create(
        [FromBody] CreateSalesInvoiceDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new
            {
                message = "Request body is required."
            });
        }

        // --------------------------------------------------------
        // Invoice Date
        // --------------------------------------------------------

        var invoiceDate =
            dto.InvoiceDate ?? DateTime.Now;

        // --------------------------------------------------------
        // Basic Validation
        // --------------------------------------------------------

        if (dto.WarehouseId <= 0)
        {
            return BadRequest(new
            {
                message = "WarehouseId is required."
            });
        }

        if (dto.CurrencyId <= 0)
        {
            return BadRequest(new
            {
                message = "CurrencyId is required."
            });
        }

        if (dto.ExchangeRate <= 0)
        {
            return BadRequest(new
            {
                message =
                    "ExchangeRate must be greater than 0."
            });
        }

        if (dto.SubTotal < 0)
        {
            return BadRequest(new
            {
                message =
                    "SubTotal cannot be negative."
            });
        }

        if (dto.DiscountAmount < 0)
        {
            return BadRequest(new
            {
                message =
                    "DiscountAmount cannot be negative."
            });
        }

        if (dto.TaxAmount < 0)
        {
            return BadRequest(new
            {
                message =
                    "TaxAmount cannot be negative."
            });
        }

        if (dto.TotalAmount < 0)
        {
            return BadRequest(new
            {
                message =
                    "TotalAmount cannot be negative."
            });
        }

        // --------------------------------------------------------
        // Warehouse
        // --------------------------------------------------------

        var warehouse =
            await _context.Warehouses
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == dto.WarehouseId &&
                    x.IsActive);

        if (warehouse == null)
        {
            return BadRequest(new
            {
                message =
                    "Warehouse not found or inactive."
            });
        }

        // --------------------------------------------------------
        // Currency
        // --------------------------------------------------------

        var currency =
            await _context.Currencies
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CurrencyId == dto.CurrencyId &&
                    x.IsActive);

        if (currency == null)
        {
            return BadRequest(new
            {
                message =
                    "Currency not found or inactive."
            });
        }

        // --------------------------------------------------------
        // Customer
        // --------------------------------------------------------

        if (dto.CustomerId.HasValue)
        {
            var customer =
                await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.CustomerId ==
                            dto.CustomerId.Value &&
                        x.IsActive);

            if (customer == null)
            {
                return BadRequest(new
                {
                    message =
                        "Customer not found or inactive."
                });
            }
        }

        // --------------------------------------------------------
        // Created User
        // --------------------------------------------------------

        if (dto.CreatedByUserId.HasValue)
        {
            var user =
                await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                            dto.CreatedByUserId.Value &&
                        x.IsActive);

            if (user == null)
            {
                return BadRequest(new
                {
                    message =
                        "CreatedByUserId not found or inactive."
                });
            }
        }

        // --------------------------------------------------------
        // SALES DRAFT
        // --------------------------------------------------------

        const int draftStatusId = 1;

        var draftStatusExists =
            await _context.DocumentStatuses
                .AsNoTracking()
                .AnyAsync(x =>
                    x.DocumentStatusId ==
                        draftStatusId &&
                    x.StatusCode ==
                        "DRAFT");

        if (!draftStatusExists)
        {
            return BadRequest(new
            {
                message =
                    "SALES DRAFT document status not found."
            });
        }

        // ========================================================
        // TRANSACTION
        // ========================================================

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            // ====================================================
            // BACKEND GENERATES INVOICE NUMBER
            // ====================================================

            var invoiceNumber =
                await GenerateSalesVoucherNumberAsync(
                    invoiceDate);

            // ====================================================
            // DUPLICATE CHECK
            // ====================================================

            var duplicateNumber =
                await _context.SalesInvoices
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.InvoiceNumber ==
                        invoiceNumber);

            if (duplicateNumber)
            {
                await transaction.RollbackAsync();

                return Conflict(new
                {
                    message =
                        "Generated Sales Voucher Number already exists.",
                    invoiceNumber
                });
            }

            // ====================================================
            // CREATE INVOICE
            // ====================================================

            var invoice =
                new Data.Models.SalesInvoice
                {
                    // IMPORTANT:
                    // Frontend InvoiceNumber is NOT used.
                    InvoiceNumber =
                        invoiceNumber,

                    InvoiceDate =
                        invoiceDate,

                    CustomerId =
                        dto.CustomerId,

                    WarehouseId =
                        dto.WarehouseId,

                    CurrencyId =
                        dto.CurrencyId,

                    ExchangeRate =
                        dto.ExchangeRate,

                    SubTotal =
                        dto.SubTotal,

                    DiscountAmount =
                        dto.DiscountAmount,

                    TaxAmount =
                        dto.TaxAmount,

                    TotalAmount =
                        dto.TotalAmount,

                    DocumentStatusId =
                        draftStatusId,

                    Notes =
                        dto.Notes,

                    CreatedAt =
                        DateTime.Now,

                    CreatedByUserId =
                        dto.CreatedByUserId
                };

            _context.SalesInvoices.Add(invoice);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // ====================================================
            // RETURN BACKEND GENERATED INVOICE NUMBER
            // ====================================================

            var result =
                new SalesInvoiceDto
                {
                    SalesInvoiceId =
                        invoice.SalesInvoiceId,

                    InvoiceNumber =
                        invoice.InvoiceNumber,

                    InvoiceDate =
                        invoice.InvoiceDate,

                    CustomerId =
                        invoice.CustomerId,

                    WarehouseId =
                        invoice.WarehouseId,

                    CurrencyId =
                        invoice.CurrencyId,

                    ExchangeRate =
                        invoice.ExchangeRate,

                    SubTotal =
                        invoice.SubTotal,

                    DiscountAmount =
                        invoice.DiscountAmount,

                    TaxAmount =
                        invoice.TaxAmount,

                    TotalAmount =
                        invoice.TotalAmount,

                    DocumentStatusId =
                        invoice.DocumentStatusId,

                    Notes =
                        invoice.Notes,

                    CreatedAt =
                        invoice.CreatedAt,

                    CreatedByUserId =
                        invoice.CreatedByUserId
                };

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id =
                        invoice.SalesInvoiceId
                },
                result
            );
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();

            return Conflict(new
            {
                message =
                    "Could not create sales invoice. " +
                    "The generated voucher number may already exist.",

                error =
                    ex.InnerException?.Message ??
                    ex.Message
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Failed to create sales invoice.",

                    error =
                        ex.Message
                });
        }
    }

    // ============================================================
    // RECALCULATE
    // ============================================================

    [HttpPost("recalculate/{id:long}")]
    public async Task<ActionResult<SalesInvoiceDto>> Recalculate(
        long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message =
                    "Invalid SalesInvoiceId."
            });
        }

        var invoice =
            await _context.SalesInvoices
                .FirstOrDefaultAsync(x =>
                    x.SalesInvoiceId == id);

        if (invoice == null)
        {
            return NotFound(new
            {
                message =
                    "Sales invoice not found."
            });
        }

        const int draftStatusId = 1;

        if (invoice.DocumentStatusId != draftStatusId)
        {
            return BadRequest(new
            {
                message =
                    "Only draft sales invoices can be recalculated."
            });
        }

        var items =
            await _context.SalesInvoiceItems
                .Where(x =>
                    x.SalesInvoiceId == id)
                .ToListAsync();

        invoice.SubTotal =
            items.Sum(x =>
                x.Quantity * x.UnitPrice);

        invoice.DiscountAmount =
            items.Sum(x =>
                x.DiscountAmount);

        invoice.TaxAmount =
            items.Sum(x =>
                x.TaxAmount);

        invoice.TotalAmount =
            invoice.SubTotal
            - invoice.DiscountAmount
            + invoice.TaxAmount;

        if (invoice.TotalAmount < 0)
        {
            return BadRequest(new
            {
                message =
                    "Calculated TotalAmount cannot be negative."
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new SalesInvoiceDto
        {
            SalesInvoiceId =
                invoice.SalesInvoiceId,

            InvoiceNumber =
                invoice.InvoiceNumber,

            InvoiceDate =
                invoice.InvoiceDate,

            CustomerId =
                invoice.CustomerId,

            WarehouseId =
                invoice.WarehouseId,

            CurrencyId =
                invoice.CurrencyId,

            ExchangeRate =
                invoice.ExchangeRate,

            SubTotal =
                invoice.SubTotal,

            DiscountAmount =
                invoice.DiscountAmount,

            TaxAmount =
                invoice.TaxAmount,

            TotalAmount =
                invoice.TotalAmount,

            DocumentStatusId =
                invoice.DocumentStatusId,

            Notes =
                invoice.Notes,

            CreatedAt =
                invoice.CreatedAt,

            CreatedByUserId =
                invoice.CreatedByUserId
        });
    }

    // ============================================================
    // POST SALES INVOICE
    // ============================================================

    [HttpPost("{id:long}/post")]
    public async Task<ActionResult<SalesInvoiceDto>> PostInvoice(
        long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message =
                    "Invalid SalesInvoiceId."
            });
        }

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            const int draftStatusId = 1;
            const int postedStatusId = 2;

            // ----------------------------------------------------
            // Invoice
            // ----------------------------------------------------

            var invoice =
                await _context.SalesInvoices
                    .FirstOrDefaultAsync(x =>
                        x.SalesInvoiceId == id);

            if (invoice == null)
            {
                return NotFound(new
                {
                    message =
                        "Sales invoice not found."
                });
            }

            if (invoice.DocumentStatusId != draftStatusId)
            {
                return BadRequest(new
                {
                    message =
                        "Only draft sales invoices can be posted."
                });
            }

            // ----------------------------------------------------
            // Posted Status
            // ----------------------------------------------------

            var postedStatusExists =
                await _context.DocumentStatuses
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.DocumentStatusId ==
                            postedStatusId &&
                        x.StatusCode ==
                            "POSTED");

            if (!postedStatusExists)
            {
                return BadRequest(new
                {
                    message =
                        "SALES POSTED document status not found."
                });
            }

            // ----------------------------------------------------
            // Items
            // ----------------------------------------------------

            var items =
                await _context.SalesInvoiceItems
                    .Where(x =>
                        x.SalesInvoiceId == id)
                    .ToListAsync();

            if (items.Count == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Sales invoice must contain at least one item."
                });
            }

            // ----------------------------------------------------
            // Warehouse
            // ----------------------------------------------------

            var warehouse =
                await _context.Warehouses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.WarehouseId ==
                            invoice.WarehouseId &&
                        x.IsActive);

            if (warehouse == null)
            {
                return BadRequest(new
                {
                    message =
                        "Warehouse not found or inactive."
                });
            }

            // ----------------------------------------------------
            // Customer
            // ----------------------------------------------------

            if (invoice.CustomerId.HasValue)
            {
                var customer =
                    await _context.Customers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.CustomerId ==
                                invoice.CustomerId.Value &&
                            x.IsActive);

                if (customer == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "Customer not found or inactive."
                    });
                }
            }

            // ====================================================
            // VALIDATE ALL ITEMS FIRST
            // ====================================================

            foreach (var item in items)
            {
                var product =
                    await _context.Products
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x =>
                            x.ProductId ==
                                item.ProductId &&
                            x.IsActive);

                if (product == null)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Product ID {item.ProductId} not found or inactive."
                    });
                }

                if (item.Quantity <= 0)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Invalid quantity for Product ID {item.ProductId}."
                    });
                }

                if (item.UnitPrice < 0)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Invalid UnitPrice for Product ID {item.ProductId}."
                    });
                }

                if (item.DiscountAmount < 0 ||
                    item.TaxAmount < 0)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Discount or Tax cannot be negative for Product ID {item.ProductId}."
                    });
                }

                if (!item.ProductStockBatchId.HasValue ||
                    item.ProductStockBatchId.Value <= 0)
                {
                    return BadRequest(new
                    {
                        message =
                            $"ProductStockBatchId is required for Product ID {item.ProductId}."
                    });
                }

                var batch =
                    await _context.ProductStockBatches
                        .FirstOrDefaultAsync(x =>
                            x.ProductStockBatchId ==
                                item.ProductStockBatchId.Value &&
                            x.ProductId ==
                                item.ProductId &&
                            x.WarehouseId ==
                                invoice.WarehouseId &&
                            x.IsActive);

                if (batch == null)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Stock batch not found for Product ID {item.ProductId}."
                    });
                }

                if (batch.RemainingQuantity <= 0)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Stock batch has no remaining quantity for Product ID {item.ProductId}."
                    });
                }

                if (batch.RemainingQuantity < item.Quantity)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Insufficient stock in selected batch for Product ID {item.ProductId}. " +
                            $"Available: {batch.RemainingQuantity}, " +
                            $"Required: {item.Quantity}."
                    });
                }

                var stock =
                    await _context.StockBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId ==
                                item.ProductId &&
                            x.WarehouseId ==
                                invoice.WarehouseId);

                if (stock == null)
                {
                    return BadRequest(new
                    {
                        message =
                            $"No stock balance found for Product ID {item.ProductId}."
                    });
                }

                if (stock.Quantity < item.Quantity)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Insufficient total stock for Product ID {item.ProductId}. " +
                            $"Available: {stock.Quantity}, " +
                            $"Required: {item.Quantity}."
                    });
                }
            }

            // ====================================================
            // PROCESS SALE
            // ====================================================

            foreach (var item in items)
            {
                var batch =
                    await _context.ProductStockBatches
                        .FirstAsync(x =>
                            x.ProductStockBatchId ==
                                item.ProductStockBatchId!.Value &&
                            x.ProductId ==
                                item.ProductId &&
                            x.WarehouseId ==
                                invoice.WarehouseId);

                var stock =
                    await _context.StockBalances
                        .FirstAsync(x =>
                            x.ProductId ==
                                item.ProductId &&
                            x.WarehouseId ==
                                invoice.WarehouseId);

                // ------------------------------------------------
                // Actual Cost
                // ------------------------------------------------

                item.UnitCost =
                    batch.CostPrice;

                // ------------------------------------------------
                // COGS
                // ------------------------------------------------

                item.Cogsamount =
                    item.UnitCost *
                    item.Quantity;

                // ------------------------------------------------
                // Gross Profit
                // ------------------------------------------------

                var netSalesAmount =
                    (item.Quantity *
                     item.UnitPrice)
                    - item.DiscountAmount;

                item.GrossProfit =
                    netSalesAmount -
                    item.Cogsamount;

                // ------------------------------------------------
                // Decrease Batch
                // ------------------------------------------------

                batch.RemainingQuantity -=
                    item.Quantity;

                if (batch.RemainingQuantity <= 0)
                {
                    batch.RemainingQuantity = 0;
                    batch.IsActive = false;
                }

                // ------------------------------------------------
                // Decrease Stock Balance
                // ------------------------------------------------

                stock.Quantity -=
                    item.Quantity;

                stock.UpdatedAt =
                    DateTime.Now;

                // ------------------------------------------------
                // Stock Movement
                // ------------------------------------------------

                var movement =
                    new Data.Models.StockMovement
                    {
                        ProductId =
                            item.ProductId,

                        WarehouseId =
                            invoice.WarehouseId,

                        StockMovementTypeId =
                            2,

                        Quantity =
                            item.Quantity,

                        UnitCost =
                            item.UnitCost,

                        ReferenceType =
                            "SALES_INVOICE",

                        ReferenceId =
                            invoice.SalesInvoiceId,

                        MovementDate =
                            invoice.InvoiceDate,

                        Notes =
                            $"Sale OUT: {invoice.InvoiceNumber} | " +
                            $"Batch: {batch.ProductStockBatchId}",

                        CreatedByUserId =
                            invoice.CreatedByUserId
                    };

                _context.StockMovements.Add(
                    movement);
            }

            // ====================================================
            // RECALCULATE TOTALS
            // ====================================================

            invoice.SubTotal =
                items.Sum(x =>
                    x.Quantity *
                    x.UnitPrice);

            invoice.DiscountAmount =
                items.Sum(x =>
                    x.DiscountAmount);

            invoice.TaxAmount =
                items.Sum(x =>
                    x.TaxAmount);

            invoice.TotalAmount =
                invoice.SubTotal
                - invoice.DiscountAmount
                + invoice.TaxAmount;

            if (invoice.TotalAmount < 0)
            {
                return BadRequest(new
                {
                    message =
                        "Calculated TotalAmount cannot be negative."
                });
            }

            // ====================================================
            // DRAFT -> POSTED
            // ====================================================

            invoice.DocumentStatusId =
                postedStatusId;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // ====================================================
            // RESPONSE
            // ====================================================

            return Ok(new SalesInvoiceDto
            {
                SalesInvoiceId =
                    invoice.SalesInvoiceId,

                InvoiceNumber =
                    invoice.InvoiceNumber,

                InvoiceDate =
                    invoice.InvoiceDate,

                CustomerId =
                    invoice.CustomerId,

                WarehouseId =
                    invoice.WarehouseId,

                CurrencyId =
                    invoice.CurrencyId,

                ExchangeRate =
                    invoice.ExchangeRate,

                SubTotal =
                    invoice.SubTotal,

                DiscountAmount =
                    invoice.DiscountAmount,

                TaxAmount =
                    invoice.TaxAmount,

                TotalAmount =
                    invoice.TotalAmount,

                DocumentStatusId =
                    invoice.DocumentStatusId,

                Notes =
                    invoice.Notes,

                CreatedAt =
                    invoice.CreatedAt,

                CreatedByUserId =
                    invoice.CreatedByUserId
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Failed to post sales invoice.",

                    error =
                        ex.Message
                });
        }
    }
}