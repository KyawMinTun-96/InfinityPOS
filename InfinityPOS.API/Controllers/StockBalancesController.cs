using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockBalancesController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public StockBalancesController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/stockbalances
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockBalanceDto>>> GetStockBalances()
    {
        var stockBalances = await _context.StockBalances
            .AsNoTracking()
            .Select(x => new StockBalanceDto
            {
                StockBalanceId = x.StockBalanceId,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                Quantity = x.Quantity,
                AverageCost = x.AverageCost,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(stockBalances);
    }

    // GET: api/stockbalances/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<StockBalanceDto>> GetStockBalance(long id)
    {
        var stockBalance = await _context.StockBalances
            .AsNoTracking()
            .Where(x => x.StockBalanceId == id)
            .Select(x => new StockBalanceDto
            {
                StockBalanceId = x.StockBalanceId,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                Quantity = x.Quantity,
                AverageCost = x.AverageCost,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (stockBalance == null)
        {
            return NotFound(new
            {
                message = "Stock balance not found."
            });
        }

        return Ok(stockBalance);
    }

    // GET: api/stockbalances/product/1
    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<IEnumerable<StockBalanceDto>>> GetByProduct(
        int productId)
    {
        var productExists = await _context.Products
            .AnyAsync(x => x.ProductId == productId);

        if (!productExists)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        var stockBalances = await _context.StockBalances
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Select(x => new StockBalanceDto
            {
                StockBalanceId = x.StockBalanceId,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                Quantity = x.Quantity,
                AverageCost = x.AverageCost,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(stockBalances);
    }

    // GET: api/stockbalances/warehouse/1
    [HttpGet("warehouse/{warehouseId:int}")]
    public async Task<ActionResult<IEnumerable<StockBalanceDto>>> GetByWarehouse(
        int warehouseId)
    {
        var warehouseExists = await _context.Warehouses
            .AnyAsync(x => x.WarehouseId == warehouseId);

        if (!warehouseExists)
        {
            return NotFound(new
            {
                message = "Warehouse not found."
            });
        }

        var stockBalances = await _context.StockBalances
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId)
            .Select(x => new StockBalanceDto
            {
                StockBalanceId = x.StockBalanceId,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                Quantity = x.Quantity,
                AverageCost = x.AverageCost,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(stockBalances);
    }
}