using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public BrandsController(InfinityPosDbContext db)
    {
        _db = db;
    }

    // GET: api/brands
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
    {
        var brands = await _db.Brands
            .AsNoTracking()
            .Where(b => b.IsActive)
            .Select(b => new BrandDto
            {
                BrandId = b.BrandId,
                BrandCode = b.BrandCode,
                BrandName = b.BrandName,
                Description = b.Description,
                IsActive = b.IsActive
            })
            .ToListAsync();

        return Ok(brands);
    }

    // GET: api/brands/1
    [HttpGet("{id}")]
    public async Task<ActionResult<BrandDto>> GetBrand(int id)
    {
        var brand = await _db.Brands
            .AsNoTracking()
            .Where(b => b.BrandId == id)
            .Select(b => new BrandDto
            {
                BrandId = b.BrandId,
                BrandCode = b.BrandCode,
                BrandName = b.BrandName,
                Description = b.Description,
                IsActive = b.IsActive
            })
            .FirstOrDefaultAsync();

        if (brand == null)
        {
            return NotFound();
        }

        return Ok(brand);
    }

    // POST: api/brands
    [HttpPost]
    public async Task<ActionResult<BrandDto>> CreateBrand(
        CreateBrandDto dto)
    {
        var brand = new Brand
        {
            BrandCode = dto.BrandCode,
            BrandName = dto.BrandName,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _db.Brands.Add(brand);

        await _db.SaveChangesAsync();

        var result = new BrandDto
        {
            BrandId = brand.BrandId,
            BrandCode = brand.BrandCode,
            BrandName = brand.BrandName,
            Description = brand.Description,
            IsActive = brand.IsActive
        };

        return CreatedAtAction(
            nameof(GetBrand),
            new { id = brand.BrandId },
            result
        );
    }

    // PUT: api/brands/1
    [HttpPut("{id}")]
    public async Task<ActionResult<BrandDto>> UpdateBrand(
        int id,
        UpdateBrandDto dto)
    {
        var brand = await _db.Brands
            .FirstOrDefaultAsync(b => b.BrandId == id);

        if (brand == null)
        {
            return NotFound();
        }

        brand.BrandCode = dto.BrandCode;
        brand.BrandName = dto.BrandName;
        brand.Description = dto.Description;
        brand.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        var result = new BrandDto
        {
            BrandId = brand.BrandId,
            BrandCode = brand.BrandCode,
            BrandName = brand.BrandName,
            Description = brand.Description,
            IsActive = brand.IsActive
        };

        return Ok(result);
    }

    // DELETE: api/brands/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        var brand = await _db.Brands
            .FirstOrDefaultAsync(b => b.BrandId == id);

        if (brand == null)
        {
            return NotFound();
        }

        // Soft Delete
        brand.IsActive = false;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}