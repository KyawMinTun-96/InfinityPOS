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
    // Stock Balance is shown by stock batch.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductStockBatchBalanceDto>>> GetStockBalances()
    {
        var stockBalances = await _context.ProductStockBatches
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.RemainingQuantity > 0)
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

    // GET: api/stockbalances/1
    // Here id means ProductStockBatchId.
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductStockBatchBalanceDto>> GetStockBalance(long id)
    {
        var stockBalance = await _context.ProductStockBatches
            .AsNoTracking()
            .Where(x =>
                x.ProductStockBatchId == id &&
                x.IsActive &&
                x.RemainingQuantity > 0)
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

    // GET: api/stockbalances/product/1
    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<IEnumerable<ProductStockBatchBalanceDto>>> GetByProduct(
        int productId)
    {
        var productExists = await _context.Products
            .AsNoTracking()
            .AnyAsync(x => x.ProductId == productId);

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
                x.IsActive &&
                x.RemainingQuantity > 0)
            .OrderBy(x => x.CreatedAt)
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

    // GET: api/stockbalances/warehouse/1
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
                x.IsActive &&
                x.RemainingQuantity > 0)
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