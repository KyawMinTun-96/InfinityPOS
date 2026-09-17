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

    // GET: api/salesinvoices
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

    // GET: api/salesinvoices/1
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

    // POST: api/salesinvoices
    [HttpPost]
    public async Task<ActionResult<SalesInvoiceDto>> Create(
        CreateSalesInvoiceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.InvoiceNumber))
        {
            return BadRequest(new
            {
                message = "InvoiceNumber is required."
            });
        }

        if (dto.InvoiceNumber.Length > 50)
        {
            return BadRequest(new
            {
                message = "InvoiceNumber must not exceed 50 characters."
            });
        }

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
                message = "ExchangeRate must be greater than 0."
            });
        }

        if (dto.SubTotal < 0 ||
            dto.DiscountAmount < 0 ||
            dto.TaxAmount < 0 ||
            dto.TotalAmount < 0)
        {
            return BadRequest(new
            {
                message = "Amounts cannot be negative."
            });
        }

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == dto.WarehouseId &&
                x.IsActive);

        if (warehouse == null)
        {
            return BadRequest(new
            {
                message = "Warehouse not found or inactive."
            });
        }

        var currency = await _context.Currencies
            .FirstOrDefaultAsync(x =>
                x.CurrencyId == dto.CurrencyId &&
                x.IsActive);

        if (currency == null)
        {
            return BadRequest(new
            {
                message = "Currency not found or inactive."
            });
        }

        if (dto.CustomerId.HasValue)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(x =>
                    x.CustomerId == dto.CustomerId.Value &&
                    x.IsActive);

            if (customer == null)
            {
                return BadRequest(new
                {
                    message = "Customer not found or inactive."
                });
            }
        }

        if (dto.CreatedByUserId.HasValue)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.UserId == dto.CreatedByUserId.Value &&
                    x.IsActive);

            if (user == null)
            {
                return BadRequest(new
                {
                    message = "CreatedByUserId not found or inactive."
                });
            }
        }

        var duplicateNumber = await _context.SalesInvoices
            .AnyAsync(x =>
                x.InvoiceNumber == dto.InvoiceNumber);

        if (duplicateNumber)
        {
            return Conflict(new
            {
                message = "InvoiceNumber already exists."
            });
        }

        // SALES DRAFT = 1
        const int draftStatusId = 1;

        var statusExists = await _context.DocumentStatuses
            .AnyAsync(x =>
                x.DocumentStatusId == draftStatusId &&
                x.StatusCode == "DRAFT");

        if (!statusExists)
        {
            return BadRequest(new
            {
                message = "SALES DRAFT document status not found."
            });
        }

        var invoice = new Data.Models.SalesInvoice
        {
            InvoiceNumber = dto.InvoiceNumber.Trim(),
            InvoiceDate = dto.InvoiceDate ?? DateTime.Now,
            CustomerId = dto.CustomerId,
            WarehouseId = dto.WarehouseId,
            CurrencyId = dto.CurrencyId,
            ExchangeRate = dto.ExchangeRate,
            SubTotal = dto.SubTotal,
            DiscountAmount = dto.DiscountAmount,
            TaxAmount = dto.TaxAmount,
            TotalAmount = dto.TotalAmount,
            DocumentStatusId = draftStatusId,
            Notes = dto.Notes,
            CreatedAt = DateTime.Now,
            CreatedByUserId = dto.CreatedByUserId
        };

        _context.SalesInvoices.Add(invoice);

        await _context.SaveChangesAsync();

        var result = new SalesInvoiceDto
        {
            SalesInvoiceId = invoice.SalesInvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            CustomerId = invoice.CustomerId,
            WarehouseId = invoice.WarehouseId,
            CurrencyId = invoice.CurrencyId,
            ExchangeRate = invoice.ExchangeRate,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            DocumentStatusId = invoice.DocumentStatusId,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            CreatedByUserId = invoice.CreatedByUserId
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.SalesInvoiceId },
            result);
    }

// POST: api/salesinvoices/recalculate/1
[HttpPost("recalculate/{id:long}")]
public async Task<ActionResult<SalesInvoiceDto>> Recalculate(long id)
{
    if (id <= 0)
    {
        return BadRequest(new
        {
            message = "Invalid SalesInvoiceId."
        });
    }

    var invoice = await _context.SalesInvoices
        .FirstOrDefaultAsync(x => x.SalesInvoiceId == id);

    if (invoice == null)
    {
        return NotFound(new
        {
            message = "Sales invoice not found."
        });
    }

    // SALES DRAFT = 1
    const int draftStatusId = 1;

    if (invoice.DocumentStatusId != draftStatusId)
    {
        return BadRequest(new
        {
            message = "Only draft sales invoices can be recalculated."
        });
    }

    var items = await _context.SalesInvoiceItems
        .Where(x => x.SalesInvoiceId == id)
        .ToListAsync();

    invoice.SubTotal = items.Sum(x =>
        x.Quantity * x.UnitPrice);

    invoice.DiscountAmount = items.Sum(x =>
        x.DiscountAmount);

    invoice.TaxAmount = items.Sum(x =>
        x.TaxAmount);

    invoice.TotalAmount =
        invoice.SubTotal
        - invoice.DiscountAmount
        + invoice.TaxAmount;

    if (invoice.TotalAmount < 0)
    {
        return BadRequest(new
        {
            message = "Calculated TotalAmount cannot be negative."
        });
    }

    await _context.SaveChangesAsync();

    var result = new SalesInvoiceDto
    {
        SalesInvoiceId = invoice.SalesInvoiceId,
        InvoiceNumber = invoice.InvoiceNumber,
        InvoiceDate = invoice.InvoiceDate,
        CustomerId = invoice.CustomerId,
        WarehouseId = invoice.WarehouseId,
        CurrencyId = invoice.CurrencyId,
        ExchangeRate = invoice.ExchangeRate,
        SubTotal = invoice.SubTotal,
        DiscountAmount = invoice.DiscountAmount,
        TaxAmount = invoice.TaxAmount,
        TotalAmount = invoice.TotalAmount,
        DocumentStatusId = invoice.DocumentStatusId,
        Notes = invoice.Notes,
        CreatedAt = invoice.CreatedAt,
        CreatedByUserId = invoice.CreatedByUserId
    };

    return Ok(result);
}

// POST: api/salesinvoices/1/post
[HttpPost("{id:long}/post")]
public async Task<ActionResult<SalesInvoiceDto>> PostInvoice(long id)
{
    if (id <= 0)
    {
        return BadRequest(new
        {
            message = "Invalid SalesInvoiceId."
        });
    }

    await using var transaction =
        await _context.Database.BeginTransactionAsync();

    try
    {
        // SALES DRAFT = 1
        // SALES POSTED = 2
        const int draftStatusId = 1;
        const int postedStatusId = 2;

        var invoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(x =>
                x.SalesInvoiceId == id);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Sales invoice not found."
            });
        }

        if (invoice.DocumentStatusId != draftStatusId)
        {
            return BadRequest(new
            {
                message = "Only draft sales invoices can be posted."
            });
        }

        // Check POSTED status
        var postedStatusExists = await _context.DocumentStatuses
            .AnyAsync(x =>
                x.DocumentStatusId == postedStatusId &&
                x.StatusCode == "POSTED");

        if (!postedStatusExists)
        {
            return BadRequest(new
            {
                message = "SALES POSTED document status not found."
            });
        }

        // Get invoice items
        var items = await _context.SalesInvoiceItems
            .Where(x => x.SalesInvoiceId == id)
            .ToListAsync();

        if (items.Count == 0)
        {
            return BadRequest(new
            {
                message = "Sales invoice must contain at least one item."
            });
        }

        // Validate warehouse
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == invoice.WarehouseId &&
                x.IsActive);

        if (warehouse == null)
        {
            return BadRequest(new
            {
                message = "Warehouse not found or inactive."
            });
        }

        // Validate customer if specified
        if (invoice.CustomerId.HasValue)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(x =>
                    x.CustomerId == invoice.CustomerId.Value &&
                    x.IsActive);

            if (customer == null)
            {
                return BadRequest(new
                {
                    message = "Customer not found or inactive."
                });
            }
        }

        // Validate all products and stock first
        foreach (var item in items)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductId == item.ProductId &&
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

            var stock = await _context.StockBalances
                .FirstOrDefaultAsync(x =>
                    x.ProductId == item.ProductId &&
                    x.WarehouseId == invoice.WarehouseId);

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
                        $"Insufficient stock for Product ID {item.ProductId}. " +
                        $"Available: {stock.Quantity}, Required: {item.Quantity}."
                });
            }
        }

        // Process stock + UnitCost + StockMovement
        foreach (var item in items)
        {
            var stock = await _context.StockBalances
                .FirstAsync(x =>
                    x.ProductId == item.ProductId &&
                    x.WarehouseId == invoice.WarehouseId);

            // Use current weighted average cost
            item.UnitCost = stock.AverageCost;

            // Decrease stock
            stock.Quantity -= item.Quantity;
            stock.UpdatedAt = DateTime.Now;

            // SALE_OUT = StockMovementTypeId 2
            var movement = new Data.Models.StockMovement
            {
                ProductId = item.ProductId,
                WarehouseId = invoice.WarehouseId,
                StockMovementTypeId = 2,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                ReferenceType = "SALES_INVOICE",
                ReferenceId = invoice.SalesInvoiceId,
                MovementDate = invoice.InvoiceDate,
                Notes = $"Sale OUT: {invoice.InvoiceNumber}",
                CreatedByUserId = invoice.CreatedByUserId
            };

            _context.StockMovements.Add(movement);
        }

        // Recalculate invoice totals
        invoice.SubTotal = items.Sum(x =>
            x.Quantity * x.UnitPrice);

        invoice.DiscountAmount = items.Sum(x =>
            x.DiscountAmount);

        invoice.TaxAmount = items.Sum(x =>
            x.TaxAmount);

        invoice.TotalAmount =
            invoice.SubTotal
            - invoice.DiscountAmount
            + invoice.TaxAmount;

        if (invoice.TotalAmount < 0)
        {
            return BadRequest(new
            {
                message = "Calculated TotalAmount cannot be negative."
            });
        }

        // Change DRAFT -> POSTED
        invoice.DocumentStatusId = postedStatusId;

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        // Reload computed columns:
        // TotalAmount, COGSAmount, GrossProfit
        await _context.Entry(invoice).ReloadAsync();

        var result = new SalesInvoiceDto
        {
            SalesInvoiceId = invoice.SalesInvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            CustomerId = invoice.CustomerId,
            WarehouseId = invoice.WarehouseId,
            CurrencyId = invoice.CurrencyId,
            ExchangeRate = invoice.ExchangeRate,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            DocumentStatusId = invoice.DocumentStatusId,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            CreatedByUserId = invoice.CreatedByUserId
        };

        return Ok(result);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();

        return StatusCode(500, new
        {
            message = "Failed to post sales invoice.",
            error = ex.Message
        });
    }
}
}