using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PriceTypesController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public PriceTypesController(InfinityPosDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PriceType>>> GetPriceTypes()
    {
        var priceTypes = await _db.PriceTypes
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.PriceTypeId)
            .ToListAsync();

        return Ok(priceTypes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PriceType>> GetPriceType(int id)
    {
        var priceType = await _db.PriceTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.PriceTypeId == id
            );

        if (priceType == null)
        {
            return NotFound();
        }

        return Ok(priceType);
    }
}
