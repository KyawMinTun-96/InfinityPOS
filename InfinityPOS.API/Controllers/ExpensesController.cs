using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public ExpensesController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/expenses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetAll()
    {
        var expenses = await _context.Expenses
            .AsNoTracking()
            .OrderByDescending(x => x.ExpenseId)
            .Select(x => new ExpenseDto
            {
                ExpenseId = x.ExpenseId,
                ExpenseNumber = x.ExpenseNumber,
                ExpenseDate = x.ExpenseDate,
                PayeeName = x.PayeeName,
                CurrencyId = x.CurrencyId,
                ExchangeRate = x.ExchangeRate,
                TotalAmount = x.TotalAmount,
                DocumentStatusId = x.DocumentStatusId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .ToListAsync();

        return Ok(expenses);
    }

    // GET: api/expenses/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ExpenseDto>> GetById(long id)
    {
        var expense = await _context.Expenses
            .AsNoTracking()
            .Where(x => x.ExpenseId == id)
            .Select(x => new ExpenseDto
            {
                ExpenseId = x.ExpenseId,
                ExpenseNumber = x.ExpenseNumber,
                ExpenseDate = x.ExpenseDate,
                PayeeName = x.PayeeName,
                CurrencyId = x.CurrencyId,
                ExchangeRate = x.ExchangeRate,
                TotalAmount = x.TotalAmount,
                DocumentStatusId = x.DocumentStatusId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .FirstOrDefaultAsync();

        if (expense == null)
            return NotFound(new { message = "Expense not found." });

        return Ok(expense);
    }

    // GET: api/expenses/1/details
    [HttpGet("{id:long}/details")]
    public async Task<IActionResult> GetDetails(long id)
    {
        var expense = await _context.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExpenseId == id);

        if (expense == null)
            return NotFound(new { message = "Expense not found." });

        var items = await _context.ExpenseItems
            .AsNoTracking()
            .Where(x => x.ExpenseId == id)
            .OrderBy(x => x.ExpenseItemId)
            .Select(x => new ExpenseItemDto
            {
                ExpenseItemId = x.ExpenseItemId,
                ExpenseId = x.ExpenseId,
                AccountId = x.AccountId,
                Description = x.Description,
                Amount = x.Amount
            })
            .ToListAsync();

        return Ok(new
        {
            expense = new ExpenseDto
            {
                ExpenseId = expense.ExpenseId,
                ExpenseNumber = expense.ExpenseNumber,
                ExpenseDate = expense.ExpenseDate,
                PayeeName = expense.PayeeName,
                CurrencyId = expense.CurrencyId,
                ExchangeRate = expense.ExchangeRate,
                TotalAmount = expense.TotalAmount,
                DocumentStatusId = expense.DocumentStatusId,
                Notes = expense.Notes,
                CreatedAt = expense.CreatedAt,
                CreatedByUserId = expense.CreatedByUserId
            },
            items
        });
    }

    // POST: api/expenses
    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(
        CreateExpenseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ExpenseNumber))
            return BadRequest(new { message = "ExpenseNumber is required." });

        if (dto.CurrencyId <= 0)
            return BadRequest(new { message = "CurrencyId is required." });

        if (dto.ExchangeRate <= 0)
            return BadRequest(new { message = "ExchangeRate must be greater than zero." });

        if (dto.TotalAmount < 0)
            return BadRequest(new { message = "TotalAmount cannot be negative." });

        var currencyExists = await _context.Currencies
            .AnyAsync(x =>
                x.CurrencyId == dto.CurrencyId &&
                x.IsActive);

        if (!currencyExists)
        {
            return BadRequest(new
            {
                message = "Currency not found or inactive."
            });
        }

        if (dto.CreatedByUserId.HasValue)
        {
            var userExists = await _context.Users
                .AnyAsync(x =>
                    x.UserId == dto.CreatedByUserId.Value &&
                    x.IsActive);

            if (!userExists)
            {
                return BadRequest(new
                {
                    message = "CreatedByUserId not found or inactive."
                });
            }
        }

        var duplicateNumber = await _context.Expenses
            .AnyAsync(x => x.ExpenseNumber == dto.ExpenseNumber);

        if (duplicateNumber)
        {
            return Conflict(new
            {
                message = "ExpenseNumber already exists."
            });
        }

        var expense = new Data.Models.Expense
        {
            ExpenseNumber = dto.ExpenseNumber,
            ExpenseDate = dto.ExpenseDate ?? DateTime.Now,
            PayeeName = dto.PayeeName,
            CurrencyId = dto.CurrencyId,
            ExchangeRate = dto.ExchangeRate,
            TotalAmount = dto.TotalAmount,
            DocumentStatusId = 1, // DRAFT
            Notes = dto.Notes,
            CreatedByUserId = dto.CreatedByUserId
        };

        _context.Expenses.Add(expense);

        await _context.SaveChangesAsync();

        var result = new ExpenseDto
        {
            ExpenseId = expense.ExpenseId,
            ExpenseNumber = expense.ExpenseNumber,
            ExpenseDate = expense.ExpenseDate,
            PayeeName = expense.PayeeName,
            CurrencyId = expense.CurrencyId,
            ExchangeRate = expense.ExchangeRate,
            TotalAmount = expense.TotalAmount,
            DocumentStatusId = expense.DocumentStatusId,
            Notes = expense.Notes,
            CreatedAt = expense.CreatedAt,
            CreatedByUserId = expense.CreatedByUserId
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = expense.ExpenseId },
            result);
    }

    // PUT: api/expenses/1
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ExpenseDto>> Update(
        long id,
        UpdateExpenseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ExpenseNumber))
            return BadRequest(new { message = "ExpenseNumber is required." });

        if (dto.CurrencyId <= 0)
            return BadRequest(new { message = "CurrencyId is required." });

        if (dto.ExchangeRate <= 0)
            return BadRequest(new { message = "ExchangeRate must be greater than zero." });

        if (dto.TotalAmount < 0)
            return BadRequest(new { message = "TotalAmount cannot be negative." });

        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x => x.ExpenseId == id);

        if (expense == null)
            return NotFound(new { message = "Expense not found." });

        if (expense.DocumentStatusId != 1)
        {
            return BadRequest(new
            {
                message = "Only DRAFT expenses can be updated."
            });
        }

        var currencyExists = await _context.Currencies
            .AnyAsync(x =>
                x.CurrencyId == dto.CurrencyId &&
                x.IsActive);

        if (!currencyExists)
        {
            return BadRequest(new
            {
                message = "Currency not found or inactive."
            });
        }

        if (dto.CreatedByUserId.HasValue)
        {
            var userExists = await _context.Users
                .AnyAsync(x =>
                    x.UserId == dto.CreatedByUserId.Value &&
                    x.IsActive);

            if (!userExists)
            {
                return BadRequest(new
                {
                    message = "CreatedByUserId not found or inactive."
                });
            }
        }

        var duplicateNumber = await _context.Expenses
            .AnyAsync(x =>
                x.ExpenseNumber == dto.ExpenseNumber &&
                x.ExpenseId != id);

        if (duplicateNumber)
        {
            return Conflict(new
            {
                message = "ExpenseNumber already exists."
            });
        }

        expense.ExpenseNumber = dto.ExpenseNumber;
        expense.ExpenseDate = dto.ExpenseDate;
        expense.PayeeName = dto.PayeeName;
        expense.CurrencyId = dto.CurrencyId;
        expense.ExchangeRate = dto.ExchangeRate;
        expense.TotalAmount = dto.TotalAmount;
        expense.Notes = dto.Notes;
        expense.CreatedByUserId = dto.CreatedByUserId;

        await _context.SaveChangesAsync();

        return Ok(new ExpenseDto
        {
            ExpenseId = expense.ExpenseId,
            ExpenseNumber = expense.ExpenseNumber,
            ExpenseDate = expense.ExpenseDate,
            PayeeName = expense.PayeeName,
            CurrencyId = expense.CurrencyId,
            ExchangeRate = expense.ExchangeRate,
            TotalAmount = expense.TotalAmount,
            DocumentStatusId = expense.DocumentStatusId,
            Notes = expense.Notes,
            CreatedAt = expense.CreatedAt,
            CreatedByUserId = expense.CreatedByUserId
        });
    }

    // POST: api/expenses/1/recalculate
    [HttpPost("{id:long}/recalculate")]
    public async Task<ActionResult<ExpenseDto>> Recalculate(long id)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x => x.ExpenseId == id);

        if (expense == null)
            return NotFound(new { message = "Expense not found." });

        if (expense.DocumentStatusId != 1)
        {
            return BadRequest(new
            {
                message = "Only DRAFT expenses can be recalculated."
            });
        }

        expense.TotalAmount = await _context.ExpenseItems
            .Where(x => x.ExpenseId == id)
            .SumAsync(x => x.Amount);

        await _context.SaveChangesAsync();

        return Ok(new ExpenseDto
        {
            ExpenseId = expense.ExpenseId,
            ExpenseNumber = expense.ExpenseNumber,
            ExpenseDate = expense.ExpenseDate,
            PayeeName = expense.PayeeName,
            CurrencyId = expense.CurrencyId,
            ExchangeRate = expense.ExchangeRate,
            TotalAmount = expense.TotalAmount,
            DocumentStatusId = expense.DocumentStatusId,
            Notes = expense.Notes,
            CreatedAt = expense.CreatedAt,
            CreatedByUserId = expense.CreatedByUserId
        });
    }

    // POST: api/expenses/1/post
    [HttpPost("{id:long}/post")]
    public async Task<ActionResult<ExpenseDto>> Post(long id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(x => x.ExpenseId == id);

            if (expense == null)
                return NotFound(new { message = "Expense not found." });

            if (expense.DocumentStatusId != 1)
            {
                return BadRequest(new
                {
                    message = "Only DRAFT expenses can be posted."
                });
            }

            var items = await _context.ExpenseItems
                .Where(x => x.ExpenseId == id)
                .ToListAsync();

            if (items.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Expense must have at least one item before posting."
                });
            }

            if (items.Any(x => x.Amount <= 0))
            {
                return BadRequest(new
                {
                    message = "All expense item amounts must be greater than zero."
                });
            }

            foreach (var item in items)
            {
                var accountExists = await _context.Accounts
                    .AnyAsync(x =>
                        x.AccountId == item.AccountId &&
                        x.IsActive);

                if (!accountExists)
                {
                    return BadRequest(new
                    {
                        message = $"Account ID {item.AccountId} not found or inactive."
                    });
                }
            }

            expense.TotalAmount = items.Sum(x => x.Amount);
            expense.DocumentStatusId = 2; // POSTED

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new ExpenseDto
            {
                ExpenseId = expense.ExpenseId,
                ExpenseNumber = expense.ExpenseNumber,
                ExpenseDate = expense.ExpenseDate,
                PayeeName = expense.PayeeName,
                CurrencyId = expense.CurrencyId,
                ExchangeRate = expense.ExchangeRate,
                TotalAmount = expense.TotalAmount,
                DocumentStatusId = expense.DocumentStatusId,
                Notes = expense.Notes,
                CreatedAt = expense.CreatedAt,
                CreatedByUserId = expense.CreatedByUserId
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                message = "Failed to post expense.",
                error = ex.Message
            });
        }
    }

    // DELETE: api/expenses/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Void(long id)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x => x.ExpenseId == id);

        if (expense == null)
            return NotFound(new { message = "Expense not found." });

        if (expense.DocumentStatusId == 3)
        {
            return BadRequest(new
            {
                message = "Expense is already VOID."
            });
        }

        expense.DocumentStatusId = 3; // VOID

        await _context.SaveChangesAsync();

        return NoContent();
    }
}