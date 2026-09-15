using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseInvoiceItemsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public PurchaseInvoiceItemsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/purchaseinvoiceitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseInvoiceItemDto>>> GetPurchaseInvoiceItems()
    {
        var items = await _context.PurchaseInvoiceItems
            .OrderByDescending(x => x.PurchaseInvoiceItemId)
            .Select(x => new PurchaseInvoiceItemDto
            {
                PurchaseInvoiceItemId = x.PurchaseInvoiceItemId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: api/purchaseinvoiceitems/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PurchaseInvoiceItemDto>> GetPurchaseInvoiceItem(long id)
    {
        var item = await _context.PurchaseInvoiceItems
            .Where(x => x.PurchaseInvoiceItemId == id)
            .Select(x => new PurchaseInvoiceItemDto
            {
                PurchaseInvoiceItemId = x.PurchaseInvoiceItemId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount
            })
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound(new
            {
                message = "Purchase invoice item not found."
            });
        }

        return Ok(item);
    }

    // GET: api/purchaseinvoiceitems/invoice/1
    [HttpGet("invoice/{purchaseInvoiceId:long}")]
    public async Task<ActionResult<IEnumerable<PurchaseInvoiceItemDto>>> GetItemsByInvoice(
        long purchaseInvoiceId)
    {
        var invoiceExists = await _context.PurchaseInvoices
            .AnyAsync(x => x.PurchaseInvoiceId == purchaseInvoiceId);

        if (!invoiceExists)
        {
            return NotFound(new
            {
                message = "Purchase invoice not found."
            });
        }

        var items = await _context.PurchaseInvoiceItems
            .Where(x => x.PurchaseInvoiceId == purchaseInvoiceId)
            .OrderBy(x => x.PurchaseInvoiceItemId)
            .Select(x => new PurchaseInvoiceItemDto
            {
                PurchaseInvoiceItemId = x.PurchaseInvoiceItemId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount
            })
            .ToListAsync();

        return Ok(items);
    }

    // POST: api/purchaseinvoiceitems
    [HttpPost]
    public async Task<ActionResult<PurchaseInvoiceItemDto>> CreatePurchaseInvoiceItem(
        CreatePurchaseInvoiceItemDto dto)
    {
        if (dto.PurchaseInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "PurchaseInvoiceId is required."
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

        if (dto.UnitCost < 0)
        {
            return BadRequest(new
            {
                message = "UnitCost cannot be negative."
            });
        }

        if (dto.DiscountAmount < 0 || dto.TaxAmount < 0)
        {
            return BadRequest(new
            {
                message = "DiscountAmount and TaxAmount cannot be negative."
            });
        }

        // Purchase Invoice validation
        var invoiceExists = await _context.PurchaseInvoices
            .AnyAsync(x => x.PurchaseInvoiceId == dto.PurchaseInvoiceId);

        if (!invoiceExists)
        {
            return BadRequest(new
            {
                message = "Purchase invoice not found."
            });
        }

        // Product validation
        var productExists = await _context.Products
            .AnyAsync(x =>
                x.ProductId == dto.ProductId &&
                x.IsActive);

        if (!productExists)
        {
            return BadRequest(new
            {
                message = "Product not found or inactive."
            });
        }

        var totalAmount =
            (dto.Quantity * dto.UnitCost)
            - dto.DiscountAmount
            + dto.TaxAmount;

        if (totalAmount < 0)
        {
            return BadRequest(new
            {
                message = "TotalAmount cannot be negative."
            });
        }

        var item = new Data.Models.PurchaseInvoiceItem
        {
            PurchaseInvoiceId = dto.PurchaseInvoiceId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            DiscountAmount = dto.DiscountAmount,
            TaxAmount = dto.TaxAmount,
            TotalAmount = totalAmount
        };

        _context.PurchaseInvoiceItems.Add(item);

        await _context.SaveChangesAsync();

        var result = new PurchaseInvoiceItemDto
        {
            PurchaseInvoiceItemId = item.PurchaseInvoiceItemId,
            PurchaseInvoiceId = item.PurchaseInvoiceId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitCost = item.UnitCost,
            DiscountAmount = item.DiscountAmount,
            TaxAmount = item.TaxAmount,
            TotalAmount = item.TotalAmount
        };

        return CreatedAtAction(
            nameof(GetPurchaseInvoiceItem),
            new { id = item.PurchaseInvoiceItemId },
            result
        );
    }

    // PUT: api/purchaseinvoiceitems/1
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdatePurchaseInvoiceItem(
        long id,
        UpdatePurchaseInvoiceItemDto dto)
    {
        if (dto.PurchaseInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "PurchaseInvoiceId is required."
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

        if (dto.UnitCost < 0)
        {
            return BadRequest(new
            {
                message = "UnitCost cannot be negative."
            });
        }

        if (dto.DiscountAmount < 0 || dto.TaxAmount < 0)
        {
            return BadRequest(new
            {
                message = "DiscountAmount and TaxAmount cannot be negative."
            });
        }

        var item = await _context.PurchaseInvoiceItems
            .FirstOrDefaultAsync(x => x.PurchaseInvoiceItemId == id);

        if (item == null)
        {
            return NotFound(new
            {
                message = "Purchase invoice item not found."
            });
        }

        var invoiceExists = await _context.PurchaseInvoices
            .AnyAsync(x => x.PurchaseInvoiceId == dto.PurchaseInvoiceId);

        if (!invoiceExists)
        {
            return BadRequest(new
            {
                message = "Purchase invoice not found."
            });
        }

        var productExists = await _context.Products
            .AnyAsync(x =>
                x.ProductId == dto.ProductId &&
                x.IsActive);

        if (!productExists)
        {
            return BadRequest(new
            {
                message = "Product not found or inactive."
            });
        }

        var totalAmount =
            (dto.Quantity * dto.UnitCost)
            - dto.DiscountAmount
            + dto.TaxAmount;

        if (totalAmount < 0)
        {
            return BadRequest(new
            {
                message = "TotalAmount cannot be negative."
            });
        }

        item.PurchaseInvoiceId = dto.PurchaseInvoiceId;
        item.ProductId = dto.ProductId;
        item.Quantity = dto.Quantity;
        item.UnitCost = dto.UnitCost;
        item.DiscountAmount = dto.DiscountAmount;
        item.TaxAmount = dto.TaxAmount;
        item.TotalAmount = totalAmount;

        await _context.SaveChangesAsync();

        return Ok(new PurchaseInvoiceItemDto
        {
            PurchaseInvoiceItemId = item.PurchaseInvoiceItemId,
            PurchaseInvoiceId = item.PurchaseInvoiceId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitCost = item.UnitCost,
            DiscountAmount = item.DiscountAmount,
            TaxAmount = item.TaxAmount,
            TotalAmount = item.TotalAmount
        });
    }

    // DELETE: api/purchaseinvoiceitems/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeletePurchaseInvoiceItem(long id)
    {
        var item = await _context.PurchaseInvoiceItems
            .FirstOrDefaultAsync(x => x.PurchaseInvoiceItemId == id);

        if (item == null)
        {
            return NotFound(new
            {
                message = "Purchase invoice item not found."
            });
        }

        _context.PurchaseInvoiceItems.Remove(item);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Purchase invoice item deleted successfully."
        });
    }
}