using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/expenseitems")]
public class ExpenseItemsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public ExpenseItemsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/expenseitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseItemDto>>> GetAll()
    {
        var items = await _context.ExpenseItems
            .AsNoTracking()
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

        return Ok(items);
    }

    // GET: api/expenseitems/5
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ExpenseItemDto>> GetById(long id)
    {
        var item = await _context.ExpenseItems
            .AsNoTracking()
            .Where(x => x.ExpenseItemId == id)
            .Select(x => new ExpenseItemDto
            {
                ExpenseItemId = x.ExpenseItemId,
                ExpenseId = x.ExpenseId,
                AccountId = x.AccountId,
                Description = x.Description,
                Amount = x.Amount
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return NotFound(new { message = "Expense item not found." });

        return Ok(item);
    }

    // GET: api/expenseitems/expense/1
    [HttpGet("expense/{expenseId:long}")]
    public async Task<ActionResult<IEnumerable<ExpenseItemDto>>> GetByExpense(long expenseId)
    {
        var expenseExists = await _context.Expenses
            .AnyAsync(x => x.ExpenseId == expenseId);

        if (!expenseExists)
            return NotFound(new { message = "Expense not found." });

        var items = await _context.ExpenseItems
            .AsNoTracking()
            .Where(x => x.ExpenseId == expenseId)
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

        return Ok(items);
    }

    // POST: api/expenseitems
    [HttpPost]
    public async Task<ActionResult<ExpenseItemDto>> Create(
        CreateExpenseItemDto dto)
    {
        if (dto.ExpenseId <= 0)
            return BadRequest(new { message = "ExpenseId is required." });

        if (dto.AccountId <= 0)
            return BadRequest(new { message = "AccountId is required." });

        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x => x.ExpenseId == dto.ExpenseId);

        if (expense == null)
            return NotFound(new { message = "Expense not found." });

        // Expense must be DRAFT.
        if (expense.DocumentStatusId != 1 &&
            expense.DocumentStatusId != 4 &&
            expense.DocumentStatusId != 7 &&
            expense.DocumentStatusId != 10)
        {
            return BadRequest(new
            {
                message = "Expense item can only be added while the expense is in DRAFT status."
            });
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(x =>
                x.AccountId == dto.AccountId &&
                x.IsActive);

        if (account == null)
        {
            return BadRequest(new
            {
                message = "Account not found or inactive."
            });
        }

        var item = new Data.Models.ExpenseItem
        {
            ExpenseId = dto.ExpenseId,
            AccountId = dto.AccountId,
            Description = dto.Description,
            Amount = dto.Amount
        };

        _context.ExpenseItems.Add(item);

        expense.TotalAmount = await CalculateTotalAsync(dto.ExpenseId) + dto.Amount;

        await _context.SaveChangesAsync();

        var result = new ExpenseItemDto
        {
            ExpenseItemId = item.ExpenseItemId,
            ExpenseId = item.ExpenseId,
            AccountId = item.AccountId,
            Description = item.Description,
            Amount = item.Amount
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.ExpenseItemId },
            result);
    }

    // PUT: api/expenseitems/1
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ExpenseItemDto>> Update(
        long id,
        UpdateExpenseItemDto dto)
    {
        if (dto.AccountId <= 0)
            return BadRequest(new { message = "AccountId is required." });

        if (dto.Amount <= 0)
            return BadRequest(new { message = "Amount must be greater than zero." });

        var item = await _context.ExpenseItems
            .FirstOrDefaultAsync(x => x.ExpenseItemId == id);

        if (item == null)
            return NotFound(new { message = "Expense item not found." });

        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x => x.ExpenseId == item.ExpenseId);

        if (expense == null)
            return NotFound(new { message = "Expense not found." });

        if (expense.DocumentStatusId != 1 &&
            expense.DocumentStatusId != 4 &&
            expense.DocumentStatusId != 7 &&
            expense.DocumentStatusId != 10)
        {
            return BadRequest(new
            {
                message = "Expense item can only be updated while the expense is in DRAFT status."
            });
        }

        var account = await _context.Accounts
            .FirstOrDefaultAsync(x =>
                x.AccountId == dto.AccountId &&
                x.IsActive);

        if (account == null)
        {
            return BadRequest(new
            {
                message = "Account not found or inactive."
            });
        }

        item.AccountId = dto.AccountId;
        item.Description = dto.Description;
        item.Amount = dto.Amount;

        await _context.SaveChangesAsync();

        await RecalculateExpenseTotalAsync(expense.ExpenseId);

        var result = new ExpenseItemDto
        {
            ExpenseItemId = item.ExpenseItemId,
            ExpenseId = item.ExpenseId,
            AccountId = item.AccountId,
            Description = item.Description,
            Amount = item.Amount
        };

        return Ok(result);
    }

    // DELETE: api/expenseitems/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var item = await _context.ExpenseItems
            .FirstOrDefaultAsync(x => x.ExpenseItemId == id);

        if (item == null)
            return NotFound(new { message = "Expense item not found." });

        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x => x.ExpenseId == item.ExpenseId);

        if (expense == null)
            return NotFound(new { message = "Expense not found." });

        if (expense.DocumentStatusId != 1 &&
            expense.DocumentStatusId != 4 &&
            expense.DocumentStatusId != 7 &&
            expense.DocumentStatusId != 10)
        {
            return BadRequest(new
            {
                message = "Expense item can only be deleted while the expense is in DRAFT status."
            });
        }

        var expenseId = item.ExpenseId;

        _context.ExpenseItems.Remove(item);

        await _context.SaveChangesAsync();

        await RecalculateExpenseTotalAsync(expenseId);

        return NoContent();
    }

    private async Task<decimal> CalculateTotalAsync(long expenseId)
    {
        return await _context.ExpenseItems
            .Where(x => x.ExpenseId == expenseId)
            .SumAsync(x => x.Amount);
    }

    private async Task RecalculateExpenseTotalAsync(long expenseId)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(x => x.ExpenseId == expenseId);

        if (expense == null)
            return;

        expense.TotalAmount = await CalculateTotalAsync(expenseId);

        await _context.SaveChangesAsync();
    }
}