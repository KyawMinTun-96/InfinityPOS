using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductPricesController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public ProductPricesController(InfinityPosDbContext db)
    {
        _db = db;
    }

    // GET: api/productprices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductPriceDto>>> GetProductPrices()
    {
        var prices = await _db.ProductPrices
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new ProductPriceDto
            {
                ProductPriceId = p.ProductPriceId,
                ProductId = p.ProductId,
                PriceTypeId = p.PriceTypeId,
                Price = p.Price,
                CurrencyId = p.CurrencyId,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                CreatedByUserId = p.CreatedByUserId
            })
            .ToListAsync();

        return Ok(prices);
    }

    // GET: api/productprices/1
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductPriceDto>> GetProductPrice(long id)
    {
        var price = await _db.ProductPrices
            .AsNoTracking()
            .Where(p => p.ProductPriceId == id)
            .Select(p => new ProductPriceDto
            {
                ProductPriceId = p.ProductPriceId,
                ProductId = p.ProductId,
                PriceTypeId = p.PriceTypeId,
                Price = p.Price,
                CurrencyId = p.CurrencyId,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                CreatedByUserId = p.CreatedByUserId
            })
            .FirstOrDefaultAsync();

        if (price == null)
        {
            return NotFound();
        }

        return Ok(price);
    }

    // GET: api/productprices/product/1
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<ProductPriceDto>>> GetProductPricesByProduct(
        int productId)
    {
        var prices = await _db.ProductPrices
            .AsNoTracking()
            .Where(p => p.ProductId == productId && p.IsActive)
            .OrderByDescending(p => p.EffectiveFrom)
            .Select(p => new ProductPriceDto
            {
                ProductPriceId = p.ProductPriceId,
                ProductId = p.ProductId,
                PriceTypeId = p.PriceTypeId,
                Price = p.Price,
                CurrencyId = p.CurrencyId,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                CreatedByUserId = p.CreatedByUserId
            })
            .ToListAsync();

        return Ok(prices);
    }

    // POST: api/productprices
    [HttpPost]
    public async Task<ActionResult<ProductPriceDto>> CreateProductPrice(
        CreateProductPriceDto dto)
    {
        var productExists = await _db.Products
            .AnyAsync(p => p.ProductId == dto.ProductId);

        if (!productExists)
        {
            return BadRequest($"Product ID {dto.ProductId} not found.");
        }

        var priceTypeExists = await _db.PriceTypes
            .AnyAsync(p => p.PriceTypeId == dto.PriceTypeId);

        if (!priceTypeExists)
        {
            return BadRequest($"Price Type ID {dto.PriceTypeId} not found.");
        }

        var currencyExists = await _db.Currencies
            .AnyAsync(c => c.CurrencyId == dto.CurrencyId);

        if (!currencyExists)
        {
            return BadRequest($"Currency ID {dto.CurrencyId} not found.");
        }

        var productPrice = new ProductPrice
        {
            ProductId = dto.ProductId,
            PriceTypeId = dto.PriceTypeId,
            Price = dto.Price,
            CurrencyId = dto.CurrencyId,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            IsActive = true,
            CreatedAt = DateTime.Now,
            CreatedByUserId = dto.CreatedByUserId
        };

        _db.ProductPrices.Add(productPrice);

        await _db.SaveChangesAsync();

        var result = new ProductPriceDto
        {
            ProductPriceId = productPrice.ProductPriceId,
            ProductId = productPrice.ProductId,
            PriceTypeId = productPrice.PriceTypeId,
            Price = productPrice.Price,
            CurrencyId = productPrice.CurrencyId,
            EffectiveFrom = productPrice.EffectiveFrom,
            EffectiveTo = productPrice.EffectiveTo,
            IsActive = productPrice.IsActive,
            CreatedAt = productPrice.CreatedAt,
            CreatedByUserId = productPrice.CreatedByUserId
        };

        return CreatedAtAction(
            nameof(GetProductPrice),
            new { id = productPrice.ProductPriceId },
            result
        );
    }

    // PUT: api/productprices/1
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductPriceDto>> UpdateProductPrice(
        long id,
        UpdateProductPriceDto dto)
    {
        var productPrice = await _db.ProductPrices
            .FirstOrDefaultAsync(p => p.ProductPriceId == id);

        if (productPrice == null)
        {
            return NotFound();
        }

        var productExists = await _db.Products
            .AnyAsync(p => p.ProductId == dto.ProductId);

        if (!productExists)
        {
            return BadRequest($"Product ID {dto.ProductId} not found.");
        }

        var priceTypeExists = await _db.PriceTypes
            .AnyAsync(p => p.PriceTypeId == dto.PriceTypeId);

        if (!priceTypeExists)
        {
            return BadRequest($"Price Type ID {dto.PriceTypeId} not found.");
        }

        var currencyExists = await _db.Currencies
            .AnyAsync(c => c.CurrencyId == dto.CurrencyId);

        if (!currencyExists)
        {
            return BadRequest($"Currency ID {dto.CurrencyId} not found.");
        }

        productPrice.ProductId = dto.ProductId;
        productPrice.PriceTypeId = dto.PriceTypeId;
        productPrice.Price = dto.Price;
        productPrice.CurrencyId = dto.CurrencyId;
        productPrice.EffectiveFrom = dto.EffectiveFrom;
        productPrice.EffectiveTo = dto.EffectiveTo;
        productPrice.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        var result = new ProductPriceDto
        {
            ProductPriceId = productPrice.ProductPriceId,
            ProductId = productPrice.ProductId,
            PriceTypeId = productPrice.PriceTypeId,
            Price = productPrice.Price,
            CurrencyId = productPrice.CurrencyId,
            EffectiveFrom = productPrice.EffectiveFrom,
            EffectiveTo = productPrice.EffectiveTo,
            IsActive = productPrice.IsActive,
            CreatedAt = productPrice.CreatedAt,
            CreatedByUserId = productPrice.CreatedByUserId
        };

        return Ok(result);
    }

    // DELETE: api/productprices/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductPrice(long id)
    {
        var productPrice = await _db.ProductPrices
            .FirstOrDefaultAsync(p => p.ProductPriceId == id);

        if (productPrice == null)
        {
            return NotFound();
        }

        productPrice.IsActive = false;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}