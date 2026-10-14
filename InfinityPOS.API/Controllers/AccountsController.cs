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

    // GET: api/accounts
    // GET: api/accounts?isActive=true
    // GET: api/accounts?isActive=true&purpose=payment
    // GET: api/accounts?isActive=true&purpose=expense
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Account>>> GetAccounts(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? purpose = null)
    {
        try
        {
            IQueryable<Account> query =
                _context.Accounts.AsNoTracking();

            if (isActive.HasValue)
            {
                query = query.Where(
                    x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(purpose))
            {
                switch (purpose.Trim().ToLowerInvariant())
                {
                    case "payment":
                        query = query.Where(x =>
                            x.AccountCode == "1000" ||
                            x.AccountCode == "1010");
                        break;

                    case "expense":
                        query = query.Where(x =>
                            _context.AccountTypes.Any(t =>
                                t.AccountTypeId == x.AccountTypeId &&
                                t.TypeCode == "EXPENSE"));
                        break;

                    default:
                        return BadRequest(new
                        {
                            message =
                                "Invalid purpose. Use 'payment' or 'expense'."
                        });
                }
            }

            var accounts = await query
                .OrderBy(x => x.AccountCode)
                .ToListAsync();

            return Ok(accounts);
        }
        catch (Exception ex)
        {
            // Full details are logged in the backend terminal.
            Console.Error.WriteLine(
                $"GetAccounts error: {ex}");

            return StatusCode(500, new
            {
                message = "Failed to load accounts.",
                detail = ex.Message
            });
        }
    }

    // GET: api/accounts/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Account>> GetAccount(int id)
    {
        try
        {
            var account = await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.AccountId == id);

            if (account == null)
            {
                return NotFound(new
                {
                    message = "Account not found."
                });
            }

            return Ok(account);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"GetAccount error: {ex}");

            return StatusCode(500, new
            {
                message = "Failed to load account.",
                detail = ex.Message
            });
        }
    }
}
