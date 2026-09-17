using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockAdjustmentItemsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public StockAdjustmentItemsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/stockadjustmentitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockAdjustmentItemDto>>> GetItems()
    {
        var items = await _context.StockAdjustmentItems
            .AsNoTracking()
            .Select(x => new StockAdjustmentItemDto
            {
                StockAdjustmentItemId = x.StockAdjustmentItemId,
                StockAdjustmentId = x.StockAdjustmentId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                IsIncrease = x.IsIncrease,
                Notes = x.Notes
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: api/stockadjustmentitems/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<StockAdjustmentItemDto>> GetItem(long id)
    {
        var item = await _context.StockAdjustmentItems
            .AsNoTracking()
            .Where(x => x.StockAdjustmentItemId == id)
            .Select(x => new StockAdjustmentItemDto
            {
                StockAdjustmentItemId = x.StockAdjustmentItemId,
                StockAdjustmentId = x.StockAdjustmentId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                IsIncrease = x.IsIncrease,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound(new
            {
                message = "Stock adjustment item not found."
            });
        }

        return Ok(item);
    }

    // GET: api/stockadjustmentitems/adjustment/1
    [HttpGet("adjustment/{adjustmentId:long}")]
    public async Task<ActionResult<IEnumerable<StockAdjustmentItemDto>>> GetByAdjustment(
        long adjustmentId)
    {
        var adjustmentExists = await _context.StockAdjustments
            .AnyAsync(x => x.StockAdjustmentId == adjustmentId);

        if (!adjustmentExists)
        {
            return NotFound(new
            {
                message = "Stock adjustment not found."
            });
        }

        var items = await _context.StockAdjustmentItems
            .AsNoTracking()
            .Where(x => x.StockAdjustmentId == adjustmentId)
            .Select(x => new StockAdjustmentItemDto
            {
                StockAdjustmentItemId = x.StockAdjustmentItemId,
                StockAdjustmentId = x.StockAdjustmentId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                IsIncrease = x.IsIncrease,
                Notes = x.Notes
            })
            .ToListAsync();

        return Ok(items);
    }

    // POST: api/stockadjustmentitems
    [HttpPost]
    public async Task<ActionResult<StockAdjustmentItemDto>> CreateItem(
        CreateStockAdjustmentItemDto dto)
    {
        if (dto.StockAdjustmentId <= 0)
        {
            return BadRequest(new
            {
                message = "StockAdjustmentId must be greater than 0."
            });
        }

        if (dto.ProductId <= 0)
        {
            return BadRequest(new
            {
                message = "ProductId must be greater than 0."
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

        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(x =>
                x.StockAdjustmentId == dto.StockAdjustmentId);

        if (adjustment == null)
        {
            return BadRequest(new
            {
                message = "Stock adjustment not found."
            });
        }

        // Only DRAFT adjustment can accept new items.
        if (adjustment.DocumentStatusId != 7)
        {
            return BadRequest(new
            {
                message = "Only DRAFT stock adjustments can accept items."
            });
        }

        var product = await _context.Products
            .AsNoTracking()
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

        var item = new Data.Models.StockAdjustmentItem
        {
            StockAdjustmentId = dto.StockAdjustmentId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            IsIncrease = dto.IsIncrease,
            Notes = dto.Notes
        };

        _context.StockAdjustmentItems.Add(item);

        await _context.SaveChangesAsync();

        var result = new StockAdjustmentItemDto
        {
            StockAdjustmentItemId = item.StockAdjustmentItemId,
            StockAdjustmentId = item.StockAdjustmentId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitCost = item.UnitCost,
            IsIncrease = item.IsIncrease,
            Notes = item.Notes
        };

        return CreatedAtAction(
            nameof(GetItem),
            new { id = item.StockAdjustmentItemId },
            result);
    }
}