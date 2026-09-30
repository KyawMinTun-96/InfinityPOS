using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public AccountsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GET: api/accounts
    // GET: api/accounts?isActive=true
    // ============================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts(
        [FromQuery] bool? isActive = null)
    {
        IQueryable<Account> query = _context.Accounts
            .AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var accounts = await query
            .OrderBy(x => x.AccountCode)
            .ToListAsync();

        return Ok(accounts);
    }

    // ============================================================
    // GET: api/accounts/5
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AccountId == id);

        if (account == null)
        {
            return NotFound(new
            {
                message = "Account not found."
            });
        }

        return Ok(account);
    }
}
