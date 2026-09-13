using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public CategoriesController(InfinityPosDbContext db)
    {
        _db = db;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryCode = c.CategoryCode,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/categories/1
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .Where(c => c.CategoryId == id)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryCode = c.CategoryCode,
                CategoryName = c.CategoryName,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    // POST: api/categories
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        CreateCategoryDto dto)
    {
        var category = new Category
        {
            CategoryCode = dto.CategoryCode,
            CategoryName = dto.CategoryName,
            Description = dto.Description,
            IsActive = true
        };

        _db.Categories.Add(category);

        await _db.SaveChangesAsync();

        var result = new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryCode = category.CategoryCode,
            CategoryName = category.CategoryName,
            Description = category.Description,
            IsActive = category.IsActive
        };

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.CategoryId },
            result
        );
    }

    // PUT: api/categories/1
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        int id,
        UpdateCategoryDto dto)
    {
        Console.WriteLine($"CategoryCode: [{dto.CategoryCode}]");
        Console.WriteLine($"CategoryName: [{dto.CategoryName}]");
        Console.WriteLine($"Description: [{dto.Description}]");
        Console.WriteLine($"IsActive: [{dto.IsActive}]");

        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound();
        }

        category.CategoryCode = dto.CategoryCode;
        category.CategoryName = dto.CategoryName;
        category.Description = dto.Description;
        category.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        var result = new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryCode = category.CategoryCode,
            CategoryName = category.CategoryName,
            Description = category.Description,
            IsActive = category.IsActive
        };

        return Ok(result);
    }
    // DELETE: api/categories/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound();
        }

        // Soft Delete
        category.IsActive = false;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}