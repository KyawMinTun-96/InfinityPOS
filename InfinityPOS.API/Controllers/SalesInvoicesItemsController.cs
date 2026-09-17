using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesInvoicesItemsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public SalesInvoicesItemsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/salesinvoiceitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesInvoiceItemDto>>> GetAll()
    {
        var items = await _context.SalesInvoiceItems
            .AsNoTracking()
            .OrderByDescending(x => x.SalesInvoiceItemId)
            .Select(x => new SalesInvoiceItemDto
            {
                SalesInvoiceItemId = x.SalesInvoiceItemId,
                SalesInvoiceId = x.SalesInvoiceId,
                ProductId = x.ProductId,
                ProductPriceId = x.ProductPriceId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                UnitCost = x.UnitCost,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,
                COGSAmount = x.Cogsamount,
                GrossProfit = x.GrossProfit
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: api/salesinvoiceitems/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<SalesInvoiceItemDto>> GetById(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid SalesInvoiceItemId."
            });
        }

        var item = await _context.SalesInvoiceItems
            .AsNoTracking()
            .Where(x => x.SalesInvoiceItemId == id)
            .Select(x => new SalesInvoiceItemDto
            {
                SalesInvoiceItemId = x.SalesInvoiceItemId,
                SalesInvoiceId = x.SalesInvoiceId,
                ProductId = x.ProductId,
                ProductPriceId = x.ProductPriceId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                UnitCost = x.UnitCost,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,
                COGSAmount = x.Cogsamount,
                GrossProfit = x.GrossProfit
            })
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound(new
            {
                message = "Sales invoice item not found."
            });
        }

        return Ok(item);
    }

    // GET: api/salesinvoiceitems/invoice/1
    [HttpGet("invoice/{salesInvoiceId:long}")]
    public async Task<ActionResult<IEnumerable<SalesInvoiceItemDto>>> GetByInvoice(
        long salesInvoiceId)
    {
        if (salesInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid SalesInvoiceId."
            });
        }

        var invoiceExists = await _context.SalesInvoices
            .AnyAsync(x => x.SalesInvoiceId == salesInvoiceId);

        if (!invoiceExists)
        {
            return NotFound(new
            {
                message = "Sales invoice not found."
            });
        }

        var items = await _context.SalesInvoiceItems
            .AsNoTracking()
            .Where(x => x.SalesInvoiceId == salesInvoiceId)
            .OrderBy(x => x.SalesInvoiceItemId)
            .Select(x => new SalesInvoiceItemDto
            {
                SalesInvoiceItemId = x.SalesInvoiceItemId,
                SalesInvoiceId = x.SalesInvoiceId,
                ProductId = x.ProductId,
                ProductPriceId = x.ProductPriceId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                UnitCost = x.UnitCost,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,
                COGSAmount = x.Cogsamount,
                GrossProfit = x.GrossProfit
            })
            .ToListAsync();

        return Ok(items);
    }

    // POST: api/salesinvoiceitems
    [HttpPost]
    public async Task<ActionResult<SalesInvoiceItemDto>> Create(
        CreateSalesInvoiceItemDto dto)
    {
        if (dto.SalesInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "SalesInvoiceId is required."
            });
        }

        if (dto.ProductId <= 0)
        {
            return BadRequest(new
            {
                message = "ProductId is required."
            });
        }

        if (dto.Quantity <= 0)
        {
            return BadRequest(new
            {
                message = "Quantity must be greater than 0."
            });
        }

        if (dto.UnitPrice < 0)
        {
            return BadRequest(new
            {
                message = "UnitPrice cannot be negative."
            });
        }

        if (dto.DiscountAmount < 0)
        {
            return BadRequest(new
            {
                message = "DiscountAmount cannot be negative."
            });
        }

        if (dto.TaxAmount < 0)
        {
            return BadRequest(new
            {
                message = "TaxAmount cannot be negative."
            });
        }

        var invoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(x =>
                x.SalesInvoiceId == dto.SalesInvoiceId);

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
                message = "Only draft sales invoices can have items added."
            });
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.ProductId == dto.ProductId &&
                x.IsActive);

        if (product == null)
        {
            return BadRequest(new
            {
                message = "Product not found or inactive."
            });
        }

        if (dto.ProductPriceId.HasValue)
        {
            var productPrice = await _context.ProductPrices
                .FirstOrDefaultAsync(x =>
                    x.ProductPriceId == dto.ProductPriceId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (productPrice == null)
            {
                return BadRequest(new
                {
                    message = "ProductPrice not found, inactive, or does not belong to the selected product."
                });
            }
        }

        var totalAmount =
            (dto.Quantity * dto.UnitPrice)
            - dto.DiscountAmount
            + dto.TaxAmount;

        if (totalAmount < 0)
        {
            return BadRequest(new
            {
                message = "Calculated TotalAmount cannot be negative."
            });
        }

        var item = new Data.Models.SalesInvoiceItem
        {
            SalesInvoiceId = dto.SalesInvoiceId,
            ProductId = dto.ProductId,
            ProductPriceId = dto.ProductPriceId,

            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,

            // Draft stage
            UnitCost = 0,

            DiscountAmount = dto.DiscountAmount,
            TaxAmount = dto.TaxAmount,

            TotalAmount = totalAmount,

            // Calculated when invoice is posted
            Cogsamount = 0,
            GrossProfit = 0
        };

        _context.SalesInvoiceItems.Add(item);

        await _context.SaveChangesAsync();

        var result = new SalesInvoiceItemDto
        {
            SalesInvoiceItemId = item.SalesInvoiceItemId,
            SalesInvoiceId = item.SalesInvoiceId,
            ProductId = item.ProductId,
            ProductPriceId = item.ProductPriceId,

            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            UnitCost = item.UnitCost,

            DiscountAmount = item.DiscountAmount,
            TaxAmount = item.TaxAmount,

            TotalAmount = item.TotalAmount,
            COGSAmount = item.Cogsamount,
            GrossProfit = item.GrossProfit
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.SalesInvoiceItemId },
            result);
    }

    // PUT: api/salesinvoiceitems/1
    [HttpPut("{id:long}")]
    public async Task<ActionResult<SalesInvoiceItemDto>> Update(
        long id,
        UpdateSalesInvoiceItemDto dto)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid SalesInvoiceItemId."
            });
        }

        if (dto.SalesInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "SalesInvoiceId is required."
            });
        }

        if (dto.ProductId <= 0)
        {
            return BadRequest(new
            {
                message = "ProductId is required."
            });
        }

        if (dto.Quantity <= 0)
        {
            return BadRequest(new
            {
                message = "Quantity must be greater than 0."
            });
        }

        if (dto.UnitPrice < 0)
        {
            return BadRequest(new
            {
                message = "UnitPrice cannot be negative."
            });
        }

        if (dto.DiscountAmount < 0)
        {
            return BadRequest(new
            {
                message = "DiscountAmount cannot be negative."
            });
        }

        if (dto.TaxAmount < 0)
        {
            return BadRequest(new
            {
                message = "TaxAmount cannot be negative."
            });
        }

        var item = await _context.SalesInvoiceItems
            .FirstOrDefaultAsync(x =>
                x.SalesInvoiceItemId == id);

        if (item == null)
        {
            return NotFound(new
            {
                message = "Sales invoice item not found."
            });
        }

        var invoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(x =>
                x.SalesInvoiceId == dto.SalesInvoiceId);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Sales invoice not found."
            });
        }

        const int draftStatusId = 1;

        if (invoice.DocumentStatusId != draftStatusId)
        {
            return BadRequest(new
            {
                message = "Only draft sales invoices can be updated."
            });
        }

        if (item.SalesInvoiceId != dto.SalesInvoiceId)
        {
            return BadRequest(new
            {
                message = "SalesInvoiceId cannot be changed."
            });
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.ProductId == dto.ProductId &&
                x.IsActive);

        if (product == null)
        {
            return BadRequest(new
            {
                message = "Product not found or inactive."
            });
        }

        if (dto.ProductPriceId.HasValue)
        {
            var productPrice = await _context.ProductPrices
                .FirstOrDefaultAsync(x =>
                    x.ProductPriceId == dto.ProductPriceId.Value &&
                    x.ProductId == dto.ProductId &&
                    x.IsActive);

            if (productPrice == null)
            {
                return BadRequest(new
                {
                    message = "ProductPrice not found, inactive, or does not belong to the selected product."
                });
            }
        }

        var totalAmount =
            (dto.Quantity * dto.UnitPrice)
            - dto.DiscountAmount
            + dto.TaxAmount;

        if (totalAmount < 0)
        {
            return BadRequest(new
            {
                message = "Calculated TotalAmount cannot be negative."
            });
        }

        item.ProductId = dto.ProductId;
        item.ProductPriceId = dto.ProductPriceId;

        item.Quantity = dto.Quantity;
        item.UnitPrice = dto.UnitPrice;

        item.DiscountAmount = dto.DiscountAmount;
        item.TaxAmount = dto.TaxAmount;

        item.TotalAmount = totalAmount;

        // Cost and profit will be recalculated when posted.
        item.UnitCost = 0;
        item.Cogsamount = 0;
        item.GrossProfit = 0;

        await _context.SaveChangesAsync();

        var result = new SalesInvoiceItemDto
        {
            SalesInvoiceItemId = item.SalesInvoiceItemId,
            SalesInvoiceId = item.SalesInvoiceId,
            ProductId = item.ProductId,
            ProductPriceId = item.ProductPriceId,

            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            UnitCost = item.UnitCost,

            DiscountAmount = item.DiscountAmount,
            TaxAmount = item.TaxAmount,

            TotalAmount = item.TotalAmount,
            COGSAmount = item.Cogsamount,
            GrossProfit = item.GrossProfit
        };

        return Ok(result);
    }

    // DELETE: api/salesinvoiceitems/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid SalesInvoiceItemId."
            });
        }

        var item = await _context.SalesInvoiceItems
            .FirstOrDefaultAsync(x =>
                x.SalesInvoiceItemId == id);

        if (item == null)
        {
            return NotFound(new
            {
                message = "Sales invoice item not found."
            });
        }

        var invoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(x =>
                x.SalesInvoiceId == item.SalesInvoiceId);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Sales invoice not found."
            });
        }

        const int draftStatusId = 1;

        if (invoice.DocumentStatusId != draftStatusId)
        {
            return BadRequest(new
            {
                message = "Only draft sales invoices can have items deleted."
            });
        }

        _context.SalesInvoiceItems.Remove(item);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/salesinvoices/1/recalculate
[HttpPost("{id:long}/recalculate")]
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

    if (items.Count == 0)
    {
        invoice.SubTotal = 0;
        invoice.DiscountAmount = 0;
        invoice.TaxAmount = 0;
        invoice.TotalAmount = 0;
    }
    else
    {
        invoice.SubTotal = items.Sum(x =>
            (x.Quantity * x.UnitPrice) - x.DiscountAmount);

        invoice.DiscountAmount = items.Sum(x =>
            x.DiscountAmount);

        invoice.TaxAmount = items.Sum(x =>
            x.TaxAmount);

        invoice.TotalAmount = items.Sum(x =>
            ((x.Quantity * x.UnitPrice) - x.DiscountAmount)
            + x.TaxAmount);
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
}