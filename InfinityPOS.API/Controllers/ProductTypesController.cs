using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductTypesController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public ProductTypesController(InfinityPosDbContext db)
    {
        _db = db;
    }

    // GET: api/producttypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductType>>> GetProductTypes()
    {
        var productTypes = await _db.ProductTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.TypeName)
            .ToListAsync();

        return Ok(productTypes);
    }

    // GET: api/producttypes/1
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductType>> GetProductType(int id)
    {
        var productType = await _db.ProductTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductTypeId == id);

        if (productType == null)
        {
            return NotFound();
        }

        return Ok(productType);
    }
}