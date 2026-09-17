using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockTransferItemsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public StockTransferItemsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/stocktransferitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockTransferItemDto>>> GetAll()
    {
        var items = await _context.StockTransferItems
            .AsNoTracking()
            .OrderByDescending(x => x.StockTransferItemId)
            .Select(x => new StockTransferItemDto
            {
                StockTransferItemId = x.StockTransferItemId,
                StockTransferId = x.StockTransferId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost
            })
            .ToListAsync();

        return Ok(items);
    }

    // GET: api/stocktransferitems/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<StockTransferItemDto>> GetById(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid StockTransferItemId."
            });
        }

        var item = await _context.StockTransferItems
            .AsNoTracking()
            .Where(x => x.StockTransferItemId == id)
            .Select(x => new StockTransferItemDto
            {
                StockTransferItemId = x.StockTransferItemId,
                StockTransferId = x.StockTransferId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost
            })
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound(new
            {
                message = "Stock transfer item not found."
            });
        }

        return Ok(item);
    }

    // GET: api/stocktransferitems/transfer/1
    [HttpGet("transfer/{transferId:long}")]
    public async Task<ActionResult<IEnumerable<StockTransferItemDto>>> GetByTransfer(
        long transferId)
    {
        if (transferId <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid StockTransferId."
            });
        }

        var transferExists = await _context.StockTransfers
            .AnyAsync(x => x.StockTransferId == transferId);

        if (!transferExists)
        {
            return NotFound(new
            {
                message = "Stock transfer not found."
            });
        }

        var items = await _context.StockTransferItems
            .AsNoTracking()
            .Where(x => x.StockTransferId == transferId)
            .OrderBy(x => x.StockTransferItemId)
            .Select(x => new StockTransferItemDto
            {
                StockTransferItemId = x.StockTransferItemId,
                StockTransferId = x.StockTransferId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost
            })
            .ToListAsync();

        return Ok(items);
    }

    // POST: api/stocktransferitems
    [HttpPost]
    public async Task<ActionResult<StockTransferItemDto>> Create(
        CreateStockTransferItemDto dto)
    {
        if (dto.StockTransferId <= 0)
        {
            return BadRequest(new
            {
                message = "StockTransferId is required."
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

        var transfer = await _context.StockTransfers
            .FirstOrDefaultAsync(x =>
                x.StockTransferId == dto.StockTransferId);

        if (transfer == null)
        {
            return NotFound(new
            {
                message = "Stock transfer not found."
            });
        }

        // Only DRAFT transfer can receive items.
        if (transfer.DocumentStatusId != 10)
        {
            return BadRequest(new
            {
                message = "Items can only be added to a DRAFT stock transfer."
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

        var item = new Data.Models.StockTransferItem
        {
            StockTransferId = dto.StockTransferId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost
        };

        _context.StockTransferItems.Add(item);

        await _context.SaveChangesAsync();

        var result = new StockTransferItemDto
        {
            StockTransferItemId = item.StockTransferItemId,
            StockTransferId = item.StockTransferId,
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitCost = item.UnitCost
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.StockTransferItemId },
            result);
    }
}