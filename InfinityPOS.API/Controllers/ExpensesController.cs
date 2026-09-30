using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
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

// ============================================================
// GET: api/expenses
// ============================================================
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
            StatusCode = x.DocumentStatus.StatusCode,
            StatusName = x.DocumentStatus.StatusName,
            PaymentAccountId = x.PaymentAccountId,
            PaymentAccountCode = x.PaymentAccount != null
                ? x.PaymentAccount.AccountCode
                : null,
            PaymentAccountName = x.PaymentAccount != null
                ? x.PaymentAccount.AccountName
                : null,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            CreatedByUserId = x.CreatedByUserId
        })
        .ToListAsync();

    return Ok(expenses);
}


// ============================================================
// GET: api/expenses/1
// ============================================================
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
            StatusCode = x.DocumentStatus.StatusCode,
            StatusName = x.DocumentStatus.StatusName,
            PaymentAccountId = x.PaymentAccountId,
            PaymentAccountCode = x.PaymentAccount != null
                ? x.PaymentAccount.AccountCode
                : null,
            PaymentAccountName = x.PaymentAccount != null
                ? x.PaymentAccount.AccountName
                : null,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            CreatedByUserId = x.CreatedByUserId
        })
        .FirstOrDefaultAsync();

    if (expense == null)
    {
        return NotFound(new
        {
            message = "Expense not found."
        });
    }

    return Ok(expense);
}


// ============================================================
// GET: api/expenses/1/details
// ============================================================
[HttpGet("{id:long}/details")]
public async Task<IActionResult> GetDetails(long id)
{
    var expense = await _context.Expenses
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.ExpenseId == id);

    if (expense == null)
    {
        return NotFound(new
        {
            message = "Expense not found."
        });
    }

    var items = await _context.ExpenseItems
        .AsNoTracking()
        .Where(x => x.ExpenseId == id)
        .OrderBy(x => x.ExpenseItemId)
        .Select(x => new ExpenseItemDto
        {
            ExpenseItemId = x.ExpenseItemId,
            ExpenseId = x.ExpenseId,
            AccountId = x.AccountId,
            AccountCode = x.Account.AccountCode,
            AccountName = x.Account.AccountName,
            Description = x.Description,
            Amount = x.Amount
        })
        .ToListAsync();

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
        StatusCode = await GetStatusCode(expense.DocumentStatusId),
        StatusName = await GetStatusName(expense.DocumentStatusId),
        PaymentAccountId = expense.PaymentAccountId,
        PaymentAccountCode = expense.PaymentAccount?.AccountCode,
        PaymentAccountName = expense.PaymentAccount?.AccountName,
        Notes = expense.Notes,
        CreatedAt = expense.CreatedAt,
        CreatedByUserId = expense.CreatedByUserId
    };

    return Ok(new
    {
        expense = result,
        items
    });
}


// ============================================================
// POST: api/expenses
// ============================================================
[HttpPost]
public async Task<ActionResult<ExpenseDto>> Create(
    [FromBody] CreateExpenseDto dto)
{
    if (dto.CurrencyId <= 0)
    {
        return BadRequest(new
        {
            message = "CurrencyId is required."
        });
    }

    if (dto.ExchangeRate <= 0)
    {
        return BadRequest(new
        {
            message = "ExchangeRate must be greater than zero."
        });
    }

    if (dto.PaymentAccountId <= 0)
    {
        return BadRequest(new
        {
            message = "PaymentAccountId is required."
        });
    }

    if (dto.Items == null || dto.Items.Count == 0)
    {
        return BadRequest(new
        {
            message = "At least one expense item is required."
        });
    }

    if (dto.Items.Any(x => x.AccountId <= 0))
    {
        return BadRequest(new
        {
            message = "Every expense item must have a valid AccountId."
        });
    }

    if (dto.Items.Any(x => x.Amount <= 0))
    {
        return BadRequest(new
        {
            message = "All expense item amounts must be greater than zero."
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

    var paymentAccount = await _context.Accounts
        .FirstOrDefaultAsync(x =>
            x.AccountId == dto.PaymentAccountId &&
            x.IsActive);

    if (paymentAccount == null)
    {
        return BadRequest(new
        {
            message = "Payment account not found or inactive."
        });
    }

    var accountIds = dto.Items
        .Select(x => x.AccountId)
        .Distinct()
        .ToList();

    var validAccountIds = await _context.Accounts
        .Where(x =>
            accountIds.Contains(x.AccountId) &&
            x.IsActive)
        .Select(x => x.AccountId)
        .ToListAsync();

    var invalidAccountId = accountIds
        .FirstOrDefault(x => !validAccountIds.Contains(x));

    if (invalidAccountId > 0)
    {
        return BadRequest(new
        {
            message =
                $"Account ID {invalidAccountId} not found or inactive."
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

    var draftStatus = await GetExpenseStatus("DRAFT");

    if (draftStatus == null)
    {
        return StatusCode(500, new
        {
            message =
                "EXPENSE DRAFT document status is not configured."
        });
    }

    var expenseDate = dto.ExpenseDate ?? DateTime.Now;

    var totalAmount = dto.Items.Sum(x => x.Amount);

    if (totalAmount <= 0)
    {
        return BadRequest(new
        {
            message = "Expense total amount must be greater than zero."
        });
    }

    var expenseNumber = await GenerateExpenseNumber(expenseDate);

    var expense = new Expense
    {
        ExpenseNumber = expenseNumber,
        ExpenseDate = expenseDate,
        PayeeName = dto.PayeeName,
        CurrencyId = dto.CurrencyId,
        ExchangeRate = dto.ExchangeRate,
        TotalAmount = totalAmount,
        PaymentAccountId = dto.PaymentAccountId,
        DocumentStatusId = draftStatus.DocumentStatusId,
        Notes = dto.Notes,
        CreatedByUserId = dto.CreatedByUserId,
        CreatedAt = DateTime.Now
    };

    _context.Expenses.Add(expense);

    await _context.SaveChangesAsync();

    foreach (var dtoItem in dto.Items)
    {
        _context.ExpenseItems.Add(new ExpenseItem
        {
            ExpenseId = expense.ExpenseId,
            AccountId = dtoItem.AccountId,
            Description = dtoItem.Description,
            Amount = dtoItem.Amount
        });
    }

    await _context.SaveChangesAsync();

    var result = await BuildExpenseDto(expense.ExpenseId);

    return CreatedAtAction(
        nameof(GetById),
        new { id = expense.ExpenseId },
        result);
}


// ============================================================
// PUT: api/expenses/1
// ============================================================
[HttpPut("{id:long}")]
public async Task<ActionResult<ExpenseDto>> Update(
    long id,
    [FromBody] UpdateExpenseDto dto)
{
    if (dto.CurrencyId <= 0)
    {
        return BadRequest(new
        {
            message = "CurrencyId is required."
        });
    }

    if (dto.ExchangeRate <= 0)
    {
        return BadRequest(new
        {
            message = "ExchangeRate must be greater than zero."
        });
    }

    if (dto.PaymentAccountId <= 0)
    {
        return BadRequest(new
        {
            message = "PaymentAccountId is required."
        });
    }

    if (dto.Items == null || dto.Items.Count == 0)
    {
        return BadRequest(new
        {
            message = "At least one expense item is required."
        });
    }

    if (dto.Items.Any(x => x.AccountId <= 0))
    {
        return BadRequest(new
        {
            message = "Every expense item must have a valid AccountId."
        });
    }

    if (dto.Items.Any(x => x.Amount <= 0))
    {
        return BadRequest(new
        {
            message = "All expense item amounts must be greater than zero."
        });
    }

    var expense = await _context.Expenses
        .FirstOrDefaultAsync(x => x.ExpenseId == id);

    if (expense == null)
    {
        return NotFound(new
        {
            message = "Expense not found."
        });
    }

    var draftStatus = await GetExpenseStatus("DRAFT");

    if (draftStatus == null)
    {
        return StatusCode(500, new
        {
            message =
                "EXPENSE DRAFT document status is not configured."
        });
    }

    if (expense.DocumentStatusId != draftStatus.DocumentStatusId)
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

    var paymentAccountExists = await _context.Accounts
        .AnyAsync(x =>
            x.AccountId == dto.PaymentAccountId &&
            x.IsActive);

    if (!paymentAccountExists)
    {
        return BadRequest(new
        {
            message = "Payment account not found or inactive."
        });
    }

    var accountIds = dto.Items
        .Select(x => x.AccountId)
        .Distinct()
        .ToList();

    var validAccountIds = await _context.Accounts
        .Where(x =>
            accountIds.Contains(x.AccountId) &&
            x.IsActive)
        .Select(x => x.AccountId)
        .ToListAsync();

    var invalidAccountId = accountIds
        .FirstOrDefault(x => !validAccountIds.Contains(x));

    if (invalidAccountId > 0)
    {
        return BadRequest(new
        {
            message =
                $"Account ID {invalidAccountId} not found or inactive."
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

    var totalAmount = dto.Items.Sum(x => x.Amount);

    if (totalAmount <= 0)
    {
        return BadRequest(new
        {
            message = "Expense total amount must be greater than zero."
        });
    }

    // Expense number must not change during update.
    // It remains the original system-generated number.
    expense.ExpenseDate = dto.ExpenseDate ?? expense.ExpenseDate;
    expense.PayeeName = dto.PayeeName;
    expense.CurrencyId = dto.CurrencyId;
    expense.ExchangeRate = dto.ExchangeRate;
    expense.PaymentAccountId = dto.PaymentAccountId;
    expense.TotalAmount = totalAmount;
    expense.Notes = dto.Notes;

    if (dto.CreatedByUserId.HasValue)
    {
        expense.CreatedByUserId = dto.CreatedByUserId;
    }

    var oldItems = await _context.ExpenseItems
        .Where(x => x.ExpenseId == id)
        .ToListAsync();

    _context.ExpenseItems.RemoveRange(oldItems);

    foreach (var dtoItem in dto.Items)
    {
        _context.ExpenseItems.Add(new ExpenseItem
        {
            ExpenseId = id,
            AccountId = dtoItem.AccountId,
            Description = dtoItem.Description,
            Amount = dtoItem.Amount
        });
    }

    await _context.SaveChangesAsync();

    var result = await BuildExpenseDto(id);

    return Ok(result);
}


// ============================================================
// POST: api/expenses/1/recalculate
// ============================================================
[HttpPost("{id:long}/recalculate")]
public async Task<ActionResult<ExpenseDto>> Recalculate(long id)
{
    var expense = await _context.Expenses
        .FirstOrDefaultAsync(x => x.ExpenseId == id);

    if (expense == null)
    {
        return NotFound(new
        {
            message = "Expense not found."
        });
    }

    var draftStatus = await GetExpenseStatus("DRAFT");

    if (draftStatus == null)
    {
        return StatusCode(500, new
        {
            message =
                "EXPENSE DRAFT document status is not configured."
        });
    }

    if (expense.DocumentStatusId != draftStatus.DocumentStatusId)
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

    return Ok(await BuildExpenseDto(id));
}


// ============================================================
// POST: api/expenses/1/post
// ============================================================
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
        {
            return NotFound(new
            {
                message = "Expense not found."
            });
        }

        var draftStatus = await GetExpenseStatus("DRAFT");
        var postedStatus = await GetExpenseStatus("POSTED");

        if (draftStatus == null || postedStatus == null)
        {
            return StatusCode(500, new
            {
                message =
                    "EXPENSE DRAFT/POSTED document statuses are not configured."
            });
        }

        if (expense.DocumentStatusId != draftStatus.DocumentStatusId)
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
                message =
                    "Expense must have at least one item before posting."
            });
        }

        if (items.Any(x => x.Amount <= 0))
        {
            return BadRequest(new
            {
                message =
                    "All expense item amounts must be greater than zero."
            });
        }

        var paymentAccountExists = await _context.Accounts
            .AnyAsync(x =>
                x.AccountId == expense.PaymentAccountId &&
                x.IsActive);

        if (!paymentAccountExists)
        {
            return BadRequest(new
            {
                message =
                    "Payment account not found or inactive."
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
                    message =
                        $"Account ID {item.AccountId} not found or inactive."
                });
            }
        }

        expense.TotalAmount = items.Sum(x => x.Amount);
        expense.DocumentStatusId = postedStatus.DocumentStatusId;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(await BuildExpenseDto(id));
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


// ============================================================
// DELETE: api/expenses/1
// ============================================================
[HttpDelete("{id:long}")]
public async Task<IActionResult> Void(long id)
{
    var expense = await _context.Expenses
        .FirstOrDefaultAsync(x => x.ExpenseId == id);

    if (expense == null)
    {
        return NotFound(new
        {
            message = "Expense not found."
        });
    }

    var cancelledStatus = await GetExpenseStatus("CANCELLED");

    if (cancelledStatus == null)
    {
        return StatusCode(500, new
        {
            message =
                "EXPENSE CANCELLED document status is not configured."
        });
    }

    if (expense.DocumentStatusId ==
        cancelledStatus.DocumentStatusId)
    {
        return BadRequest(new
        {
            message = "Expense is already CANCELLED."
        });
    }

    expense.DocumentStatusId =
        cancelledStatus.DocumentStatusId;

    await _context.SaveChangesAsync();

    return NoContent();
}


// ============================================================
// PRIVATE: BUILD EXPENSE DTO
// ============================================================
private async Task<ExpenseDto?> BuildExpenseDto(long expenseId)
{
    return await _context.Expenses
        .AsNoTracking()
        .Where(x => x.ExpenseId == expenseId)
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
            StatusCode = x.DocumentStatus.StatusCode,
            StatusName = x.DocumentStatus.StatusName,
            PaymentAccountId = x.PaymentAccountId,
            PaymentAccountCode = x.PaymentAccount != null
                ? x.PaymentAccount.AccountCode
                : null,
            PaymentAccountName = x.PaymentAccount != null
                ? x.PaymentAccount.AccountName
                : null,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            CreatedByUserId = x.CreatedByUserId
        })
        .FirstOrDefaultAsync();
}


// ============================================================
// PRIVATE: GET EXPENSE STATUS
// ============================================================
private async Task<DocumentStatus?> GetExpenseStatus(
    string statusCode)
{
    return await _context.DocumentStatuses
        .AsNoTracking()
        .FirstOrDefaultAsync(x =>
            x.DocumentType == "EXPENSE" &&
            x.StatusCode == statusCode);
}


private async Task<string?> GetStatusCode(int statusId)
{
    return await _context.DocumentStatuses
        .AsNoTracking()
        .Where(x => x.DocumentStatusId == statusId)
        .Select(x => x.StatusCode)
        .FirstOrDefaultAsync();
}


private async Task<string?> GetStatusName(int statusId)
{
    return await _context.DocumentStatuses
        .AsNoTracking()
        .Where(x => x.DocumentStatusId == statusId)
        .Select(x => x.StatusName)
        .FirstOrDefaultAsync();
}


// ============================================================
// PRIVATE: EXPENSE NUMBER GENERATOR
// Format: EV2609300001
// ============================================================
private async Task<string> GenerateExpenseNumber(
    DateTime expenseDate)
{
    var prefix = $"EV{expenseDate:yyMMdd}";

    var existingNumbers = await _context.Expenses
        .AsNoTracking()
        .Where(x => x.ExpenseNumber.StartsWith(prefix))
        .Select(x => x.ExpenseNumber)
        .ToListAsync();

    var maxSequence = 0;

    foreach (var number in existingNumbers)
    {
        if (number.Length <= prefix.Length)
            continue;

        var sequenceText = number[prefix.Length..];

        if (int.TryParse(sequenceText, out var sequence))
        {
            if (sequence > maxSequence)
            {
                maxSequence = sequence;
            }
        }
    }

    return $"{prefix}{maxSequence + 1:0000}";
}

}
