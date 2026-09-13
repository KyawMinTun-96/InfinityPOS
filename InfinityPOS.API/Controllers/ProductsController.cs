using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public ProductsController(InfinityPosDbContext db)
    {
        _db = db;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                SKU = p.Sku,
                Barcode = p.Barcode,
                ProductName = p.ProductName,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                ProductTypeId = p.ProductTypeId,
                UnitId = p.UnitId,
                Description = p.Description,
                TrackInventory = p.TrackInventory,
                AllowNegativeStock = p.AllowNegativeStock,
                IsActive = p.IsActive
            })
            .ToListAsync();

        return Ok(products);
    }

    // GET: api/products/1
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductId == id)
            .Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                SKU = p.Sku,
                Barcode = p.Barcode,
                ProductName = p.ProductName,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                ProductTypeId = p.ProductTypeId,
                UnitId = p.UnitId,
                Description = p.Description,
                TrackInventory = p.TrackInventory,
                AllowNegativeStock = p.AllowNegativeStock,
                IsActive = p.IsActive
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
    {
        var product = new Product
        {
            Sku = dto.SKU,
            Barcode = dto.Barcode,
            ProductName = dto.ProductName,
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId,
            ProductTypeId = dto.ProductTypeId,
            UnitId = dto.UnitId,
            Description = dto.Description,
            TrackInventory = dto.TrackInventory,
            AllowNegativeStock = dto.AllowNegativeStock,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _db.Products.Add(product);

        await _db.SaveChangesAsync();

        var result = new ProductDto
        {
            ProductId = product.ProductId,
            SKU = product.Sku,
            Barcode = product.Barcode,
            ProductName = product.ProductName,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            ProductTypeId = product.ProductTypeId,
            UnitId = product.UnitId,
            Description = product.Description,
            TrackInventory = product.TrackInventory,
            AllowNegativeStock = product.AllowNegativeStock,
            IsActive = product.IsActive
        };

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.ProductId },
            result
        );
    }


    // PUT: api/products/1
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        int id,
        UpdateProductDto dto)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }

        product.Sku = dto.SKU;
        product.Barcode = dto.Barcode;
        product.ProductName = dto.ProductName;
        product.CategoryId = dto.CategoryId;
        product.BrandId = dto.BrandId;
        product.ProductTypeId = dto.ProductTypeId;
        product.UnitId = dto.UnitId;
        product.Description = dto.Description;
        product.TrackInventory = dto.TrackInventory;
        product.AllowNegativeStock = dto.AllowNegativeStock;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        var result = new ProductDto
        {
            ProductId = product.ProductId,
            SKU = product.Sku,
            Barcode = product.Barcode,
            ProductName = product.ProductName,
            CategoryId = product.CategoryId,
            BrandId = product.BrandId,
            ProductTypeId = product.ProductTypeId,
            UnitId = product.UnitId,
            Description = product.Description,
            TrackInventory = product.TrackInventory,
            AllowNegativeStock = product.AllowNegativeStock,
            IsActive = product.IsActive
        };

        return Ok(result);
    }


    // DELETE: api/products/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }

        product.IsActive = false;
        product.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        return NoContent();
    }

}