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

    // ============================================================
    // GET: api/stockbalances
    //
    // Returns ALL active stock batches.
    // Balance = 0 is intentionally included.
    // ============================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductStockBatchBalanceDto>>> GetStockBalances()
    {
        var stockBalances = await _context.ProductStockBatches
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.WarehouseId)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new ProductStockBatchBalanceDto
            {
                ProductStockBatchId = x.ProductStockBatchId,

                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,

                PurchaseInvoiceItemId = x.PurchaseInvoiceItemId,

                CostPrice = x.CostPrice,
                SalePrice = x.SalePrice,

                OriginalQuantity = x.OriginalQuantity,
                RemainingQuantity = x.RemainingQuantity,

                CurrencyId = x.CurrencyId,

                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(stockBalances);
    }

    // ============================================================
    // GET: api/stockbalances/{id}
    // ============================================================
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductStockBatchBalanceDto>> GetStockBalance(long id)
    {
        var stockBalance = await _context.ProductStockBatches
            .AsNoTracking()
            .Where(x =>
                x.ProductStockBatchId == id &&
                x.IsActive)
            .Select(x => new ProductStockBatchBalanceDto
            {
                ProductStockBatchId = x.ProductStockBatchId,

                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,

                PurchaseInvoiceItemId = x.PurchaseInvoiceItemId,

                CostPrice = x.CostPrice,
                SalePrice = x.SalePrice,

                OriginalQuantity = x.OriginalQuantity,
                RemainingQuantity = x.RemainingQuantity,

                CurrencyId = x.CurrencyId,

                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (stockBalance == null)
        {
            return NotFound(new
            {
                message = "Stock batch balance not found."
            });
        }

        return Ok(stockBalance);
    }

    // ============================================================
    // GET: api/stockbalances/product/{productId}
    //
    // Balance = 0 is included.
    // ============================================================
    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<IEnumerable<ProductStockBatchBalanceDto>>> GetByProduct(
        int productId)
    {
        var productExists = await _context.Products
            .AsNoTracking()
            .AnyAsync(x =>
                x.ProductId == productId &&
                x.IsActive);

        if (!productExists)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        var stockBalances = await _context.ProductStockBatches
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.IsActive)
            .OrderBy(x => x.WarehouseId)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new ProductStockBatchBalanceDto
            {
                ProductStockBatchId = x.ProductStockBatchId,

                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,

                PurchaseInvoiceItemId = x.PurchaseInvoiceItemId,

                CostPrice = x.CostPrice,
                SalePrice = x.SalePrice,

                OriginalQuantity = x.OriginalQuantity,
                RemainingQuantity = x.RemainingQuantity,

                CurrencyId = x.CurrencyId,

                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(stockBalances);
    }

    // ============================================================
    // GET: api/stockbalances/warehouse/{warehouseId}
    //
    // Balance = 0 is included.
    // ============================================================
    [HttpGet("warehouse/{warehouseId:int}")]
    public async Task<ActionResult<IEnumerable<ProductStockBatchBalanceDto>>> GetByWarehouse(
        int warehouseId)
    {
        var warehouseExists = await _context.Warehouses
            .AsNoTracking()
            .AnyAsync(x => x.WarehouseId == warehouseId);

        if (!warehouseExists)
        {
            return NotFound(new
            {
                message = "Warehouse not found."
            });
        }

        var stockBalances = await _context.ProductStockBatches
            .AsNoTracking()
            .Where(x =>
                x.WarehouseId == warehouseId &&
                x.IsActive)
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new ProductStockBatchBalanceDto
            {
                ProductStockBatchId = x.ProductStockBatchId,

                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,

                PurchaseInvoiceItemId = x.PurchaseInvoiceItemId,

                CostPrice = x.CostPrice,
                SalePrice = x.SalePrice,

                OriginalQuantity = x.OriginalQuantity,
                RemainingQuantity = x.RemainingQuantity,

                CurrencyId = x.CurrencyId,

                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(stockBalances);
    }
}