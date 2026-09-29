using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CurrenciesController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public CurrenciesController(InfinityPosDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Currency>>> GetCurrencies()
    {
        var currencies = await _db.Currencies
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CurrencyId)
            .ToListAsync();

        return Ok(currencies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Currency>> GetCurrency(int id)
    {
        var currency = await _db.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.CurrencyId == id
            );

        if (currency == null)
        {
            return NotFound();
        }

        return Ok(currency);
    }
}
