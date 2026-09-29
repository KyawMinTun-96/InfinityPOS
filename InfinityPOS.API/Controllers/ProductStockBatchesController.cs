using InfinityPOS.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductStockBatchesController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public ProductStockBatchesController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/productstockbatches/product/1/warehouse/1
    [HttpGet("product/{productId:int}/warehouse/{warehouseId:int}")]
    public async Task<IActionResult> GetAvailableBatches(
        int productId,
        int warehouseId)
    {
        if (productId <= 0)
        {
            return BadRequest(new
            {
                message = "ProductId is required."
            });
        }

        if (warehouseId <= 0)
        {
            return BadRequest(new
            {
                message = "WarehouseId is required."
            });
        }

        var productExists = await _context.Products
            .AsNoTracking()
            .AnyAsync(x =>
                x.ProductId == productId &&
                x.IsActive);

        if (!productExists)
        {
            return NotFound(new
            {
                message = "Product not found or inactive."
            });
        }

        var warehouseExists = await _context.Warehouses
            .AsNoTracking()
            .AnyAsync(x =>
                x.WarehouseId == warehouseId &&
                x.IsActive);

        if (!warehouseExists)
        {
            return NotFound(new
            {
                message = "Warehouse not found or inactive."
            });
        }

        var batches = await _context.ProductStockBatches
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId &&
                x.WarehouseId == warehouseId &&
                x.IsActive &&
                x.RemainingQuantity > 0)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                productStockBatchId =
                    x.ProductStockBatchId,

                productId =
                    x.ProductId,

                warehouseId =
                    x.WarehouseId,

                purchaseInvoiceItemId =
                    x.PurchaseInvoiceItemId,

                costPrice =
                    x.CostPrice,

                salePrice =
                    x.SalePrice,

                originalQuantity =
                    x.OriginalQuantity,

                remainingQuantity =
                    x.RemainingQuantity,

                currencyId =
                    x.CurrencyId,

                createdAt =
                    x.CreatedAt,

                isActive =
                    x.IsActive
            })
            .ToListAsync();

        return Ok(batches);
    }
}