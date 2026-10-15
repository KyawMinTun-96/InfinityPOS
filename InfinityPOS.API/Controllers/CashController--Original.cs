/*
using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CashController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    // ============================================================
    // CASH ACCOUNT
    //
    // Accounts:
    // AccountId = 1
    // AccountCode = 1000
    // AccountName = Cash
    // AccountType.TypeCode = ASSET
    //
    // Expense.PaymentAccountId points to Accounts.AccountId.
    // It is NOT PaymentMethodId.
    // ============================================================

    private const int CashAccountId = 1;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public CashController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GET SESSION BY BUSINESS DATE
    //
    // GET /api/cash/session?date=2026-09-30
    // ============================================================

    [HttpGet("session")]
    public async Task<ActionResult<CashSessionDto>> GetSession(
        [FromQuery] DateTime? date = null)
    {
        var businessDate =
            (date ?? DateTime.Today).Date;

        var session =
            await _context.CashSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.BusinessDate == businessDate);

        if (session == null)
        {
            return NotFound(new
            {
                message = "Cash session not found.",
                businessDate
            });
        }

        var dto =
            await BuildSessionDtoAsync(session);

        return Ok(dto);
    }

    // ============================================================
    // GET OPEN/CURRENT SESSION
    //
    // GET /api/cash/current
    // ============================================================

    [HttpGet("current")]
    public async Task<ActionResult<CashSessionDto>>
        GetCurrentSession()
    {
        var session =
            await _context.CashSessions
                .AsNoTracking()
                .Where(x =>
                    x.Status == "OPEN")
                .OrderByDescending(x =>
                    x.BusinessDate)
                .FirstOrDefaultAsync();

        if (session == null)
        {
            return NotFound(new
            {
                message =
                    "No open cash session found."
            });
        }

        var dto =
            await BuildSessionDtoAsync(session);

        return Ok(dto);
    }

    // ============================================================
    // GET SESSION BY ID
    //
    // GET /api/cash/1
    // ============================================================

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CashSessionDto>>
        GetSessionById(long id)
    {
        var session =
            await _context.CashSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CashSessionId == id);

        if (session == null)
        {
            return NotFound(new
            {
                message =
                    "Cash session not found."
            });
        }

        var dto =
            await BuildSessionDtoAsync(session);

        return Ok(dto);
    }

    // ============================================================
    // OPEN CASH SESSION
    //
    // POST /api/cash/open
    // ============================================================

    [HttpPost("open")]
    public async Task<ActionResult<CashSessionDto>>
        OpenSession(
            [FromBody] OpenCashSessionDto dto)
    {
        var businessDate =
            (dto.BusinessDate ?? DateTime.Today).Date;

        if (dto.OpeningCash < 0)
        {
            return BadRequest(new
            {
                message =
                    "Opening cash cannot be negative."
            });
        }

        // --------------------------------------------------------
        // SAME BUSINESS DATE ALREADY EXISTS
        // --------------------------------------------------------

        var existingSession =
            await _context.CashSessions
                .FirstOrDefaultAsync(x =>
                    x.BusinessDate == businessDate);

        if (existingSession != null)
        {
            return Conflict(new
            {
                message =
                    $"Cash session already exists for {businessDate:yyyy-MM-dd}.",

                cashSessionId =
                    existingSession.CashSessionId,

                status =
                    existingSession.Status
            });
        }

        // --------------------------------------------------------
        // ONLY ONE OPEN SESSION AT A TIME
        // --------------------------------------------------------

        var openSession =
            await _context.CashSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Status == "OPEN");

        if (openSession != null)
        {
            return Conflict(new
            {
                message =
                    $"There is already an open cash session for {openSession.BusinessDate:yyyy-MM-dd}.",

                cashSessionId =
                    openSession.CashSessionId
            });
        }

        // --------------------------------------------------------
        // CREATE SESSION
        // --------------------------------------------------------

        var session = new CashSession
        {
            BusinessDate =
                businessDate,

            OpeningCash =
                dto.OpeningCash,

            SystemCash =
                dto.OpeningCash,

            ClosingCash =
                null,

            Difference =
                null,

            Status =
                "OPEN",

            OpenedAt =
                DateTime.Now,

            OpenedByUserId =
                dto.OpenedByUserId,

            Notes =
                dto.Notes,

            CreatedAt =
                DateTime.Now
        };

        _context.CashSessions.Add(session);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message =
                    "Cash session could not be opened because a session already exists for this business date."
            });
        }

        var result =
            await BuildSessionDtoAsync(session);

        return Ok(result);
    }

    // ============================================================
    // CLOSE CASH SESSION
    //
    // POST /api/cash/{id}/close
    // ============================================================

    [HttpPost("{id:long}/close")]
    public async Task<ActionResult<CashSessionDto>>
        CloseSession(
            long id,
            [FromBody] CloseCashSessionDto dto)
    {
        if (dto.ActualClosingCash < 0)
        {
            return BadRequest(new
            {
                message =
                    "Actual closing cash cannot be negative."
            });
        }

        var session =
            await _context.CashSessions
                .FirstOrDefaultAsync(x =>
                    x.CashSessionId == id);

        if (session == null)
        {
            return NotFound(new
            {
                message =
                    "Cash session not found."
            });
        }

        if (session.Status != "OPEN")
        {
            return BadRequest(new
            {
                message =
                    "This cash session is already closed."
            });
        }

        // --------------------------------------------------------
        // RECALCULATE SYSTEM CASH BEFORE CLOSING
        // --------------------------------------------------------

        var summary =
            await CalculateCashSummaryAsync(session);

        var systemCash =
            summary.SystemCash;

        var difference =
            dto.ActualClosingCash -
            systemCash;

        session.SystemCash =
            systemCash;

        session.ClosingCash =
            dto.ActualClosingCash;

        session.Difference =
            difference;

        session.Status =
            "CLOSED";

        session.ClosedAt =
            DateTime.Now;

        session.ClosedByUserId =
            dto.ClosedByUserId;

        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            session.Notes =
                dto.Notes;
        }

        await _context.SaveChangesAsync();

        var result =
            await BuildSessionDtoAsync(session);

        return Ok(result);
    }

    // ============================================================
    // CREATE CASH ADJUSTMENT
    //
    // POST /api/cash/{id}/adjustment
    //
    // CASH_IN
    // CASH_OUT
    // ============================================================

    [HttpPost("{id:long}/adjustment")]
    public async Task<ActionResult<CashAdjustment>>
        CreateAdjustment(
            long id,
            [FromBody] CreateCashAdjustmentDto dto)
    {
        var session =
            await _context.CashSessions
                .FirstOrDefaultAsync(x =>
                    x.CashSessionId == id);

        if (session == null)
        {
            return NotFound(new
            {
                message =
                    "Cash session not found."
            });
        }

        if (session.Status != "OPEN")
        {
            return BadRequest(new
            {
                message =
                    "Cash adjustment cannot be added to a closed session."
            });
        }

        var transactionType =
            dto.TransactionType
                ?.Trim()
                .ToUpperInvariant();

        if (
            transactionType != "CASH_IN" &&
            transactionType != "CASH_OUT"
        )
        {
            return BadRequest(new
            {
                message =
                    "TransactionType must be CASH_IN or CASH_OUT."
            });
        }

        if (dto.Amount <= 0)
        {
            return BadRequest(new
            {
                message =
                    "Cash adjustment amount must be greater than zero."
            });
        }

        var transactionDate =
            dto.TransactionDate ??
            DateTime.Now;

        // --------------------------------------------------------
        // ADJUSTMENT MUST MATCH SESSION BUSINESS DATE
        // --------------------------------------------------------

        if (transactionDate.Date !=
            session.BusinessDate.Date)
        {
            return BadRequest(new
            {
                message =
                    "Transaction date must match the cash session business date."
            });
        }

        var adjustment = new CashAdjustment
        {
            CashSessionId =
                session.CashSessionId,

            TransactionDate =
                transactionDate,

            TransactionType =
                transactionType,

            Amount =
                dto.Amount,

            ReferenceNumber =
                dto.ReferenceNumber,

            Description =
                dto.Description,

            CreatedByUserId =
                dto.CreatedByUserId,

            CreatedAt =
                DateTime.Now,

            IsVoided =
                false
        };

        _context.CashAdjustments.Add(adjustment);

        await _context.SaveChangesAsync();

        // --------------------------------------------------------
        // UPDATE SYSTEM CASH
        // --------------------------------------------------------

        var summary =
            await CalculateCashSummaryAsync(session);

        session.SystemCash =
            summary.SystemCash;

        await _context.SaveChangesAsync();

        return Ok(adjustment);
    }

    // ============================================================
    // GET DAILY LEDGER
    //
    // GET /api/cash/{id}/ledger
    // ============================================================

    [HttpGet("{id:long}/ledger")]
    public async Task<
        ActionResult<IEnumerable<CashLedgerEntryDto>>>
        GetLedger(long id)
    {
        var session =
            await _context.CashSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.CashSessionId == id);

        if (session == null)
        {
            return NotFound(new
            {
                message =
                    "Cash session not found."
            });
        }

        var from =
            session.BusinessDate.Date;

        var to =
            from.AddDays(1);

        var ledger =
            await BuildLedgerAsync(
                session,
                from,
                to);

        return Ok(ledger);
    }

    // ============================================================
    // GET CASH SESSION HISTORY
    //
    // GET /api/cash/history
    // GET /api/cash/history?from=2026-09-01&to=2026-09-30
    // ============================================================

    [HttpGet("history")]
    public async Task<
        ActionResult<IEnumerable<CashSessionDto>>>
        GetHistory(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
    {
        var fromDate =
            (from ?? DateTime.Today.AddDays(-30)).Date;

        var toDate =
            (to ?? DateTime.Today).Date;

        if (toDate < fromDate)
        {
            return BadRequest(new
            {
                message =
                    "The 'to' date cannot be earlier than the 'from' date."
            });
        }

        var sessions =
            await _context.CashSessions
                .AsNoTracking()
                .Where(x =>
                    x.BusinessDate >= fromDate &&
                    x.BusinessDate <= toDate)
                .OrderByDescending(x =>
                    x.BusinessDate)
                .ToListAsync();

        var result =
            new List<CashSessionDto>();

        foreach (var session in sessions)
        {
            result.Add(
                await BuildSessionDtoAsync(session));
        }

        return Ok(result);
    }

    // ============================================================
    // VOID CASH ADJUSTMENT
    //
    // POST /api/cash/adjustment/{adjustmentId}/void
    // ============================================================

    [HttpPost("adjustment/{adjustmentId:long}/void")]
    public async Task<ActionResult>
        VoidAdjustment(long adjustmentId)
    {
        var adjustment =
            await _context.CashAdjustments
                .Include(x => x.CashSession)
                .FirstOrDefaultAsync(x =>
                    x.CashAdjustmentId == adjustmentId);

        if (adjustment == null)
        {
            return NotFound(new
            {
                message =
                    "Cash adjustment not found."
            });
        }

        if (adjustment.IsVoided)
        {
            return BadRequest(new
            {
                message =
                    "Cash adjustment is already voided."
            });
        }

        if (adjustment.CashSession.Status != "OPEN")
        {
            return BadRequest(new
            {
                message =
                    "Only adjustments from an open session can be voided."
            });
        }

        adjustment.IsVoided = true;

        await _context.SaveChangesAsync();

        // --------------------------------------------------------
        // UPDATE SYSTEM CASH
        // --------------------------------------------------------

        var summary =
            await CalculateCashSummaryAsync(
                adjustment.CashSession);

        adjustment.CashSession.SystemCash =
            summary.SystemCash;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Cash adjustment voided successfully."
        });
    }

    // ============================================================
    // PRIVATE: BUILD SESSION DTO
    // ============================================================

    private async Task<CashSessionDto>
        BuildSessionDtoAsync(CashSession session)
    {
        var summary =
            await CalculateCashSummaryAsync(session);

        return new CashSessionDto
        {
            CashSessionId =
                session.CashSessionId,

            BusinessDate =
                session.BusinessDate,

            OpeningCash =
                session.OpeningCash,

            SystemCash =
                summary.SystemCash,

            ClosingCash =
                session.ClosingCash,

            Difference =
                session.Difference,

            Status =
                session.Status,

            OpenedAt =
                session.OpenedAt,

            ClosedAt =
                session.ClosedAt,

            OpenedByUserId =
                session.OpenedByUserId,

            ClosedByUserId =
                session.ClosedByUserId,

            CashSales =
                summary.CashSales,

            CashPurchases =
                summary.CashPurchases,

            CashExpenses =
                summary.CashExpenses,

            ManualCashIn =
                summary.ManualCashIn,

            ManualCashOut =
                summary.ManualCashOut
        };
    }

    // ============================================================
    // PRIVATE: CALCULATE CASH SUMMARY
    //
    // BUSINESS DATE RULE
    //
    // SALE     -> SalesInvoice.InvoiceDate
    // PURCHASE -> PurchaseInvoice.InvoiceDate
    // EXPENSE  -> Expense.ExpenseDate
    //
    // Opening
    // + Cash Sales
    // - Cash Purchases
    // - Cash Expenses
    // + Manual Cash In
    // - Manual Cash Out
    // = System Cash
    // ============================================================

    private async Task<CashSummary>
        CalculateCashSummaryAsync(CashSession session)
    {
        var from =
            session.BusinessDate.Date;

        var to =
            from.AddDays(1);

        // --------------------------------------------------------
        // CASH SALES
        //
        // SalesPayments.PaymentMethodId = 1 means CASH.
        // --------------------------------------------------------

        var cashSales =
            await _context.SalesPayments
                .AsNoTracking()
                .Where(payment =>
                    payment.PaymentMethodId == 1 &&
                    payment.SalesInvoice.InvoiceDate >= from &&
                    payment.SalesInvoice.InvoiceDate < to &&
                    payment.SalesInvoice
                        .DocumentStatus.StatusCode == "POSTED")
                .SumAsync(payment =>
                    (decimal?)payment.Amount) ?? 0m;

        // --------------------------------------------------------
        // CASH PURCHASES
        //
        // PurchasePayments.PaymentMethodId = 1 means CASH.
        // --------------------------------------------------------

        var cashPurchases =
            await _context.PurchasePayments
                .AsNoTracking()
                .Where(payment =>
                    payment.PaymentMethodId == 1 &&
                    payment.PurchaseInvoice.InvoiceDate >= from &&
                    payment.PurchaseInvoice.InvoiceDate < to &&
                    payment.PurchaseInvoice
                        .DocumentStatus.StatusCode == "POSTED")
                .SumAsync(payment =>
                    (decimal?)payment.Amount) ?? 0m;

        // --------------------------------------------------------
        // CASH EXPENSES
        //
        // PaymentAccountId references Accounts.AccountId.
        //
        // AccountId = 1 is the Cash account.
        // Only posted expenses paid from this Cash account
        // are included in Cash Management.
        // --------------------------------------------------------

        var cashExpenses =
            await _context.Expenses
                .AsNoTracking()
                .Where(expense =>
                    expense.PaymentAccountId == CashAccountId &&
                    expense.ExpenseDate >= from &&
                    expense.ExpenseDate < to &&
                    expense.DocumentStatus.StatusCode == "POSTED")
                .SumAsync(expense =>
                    (decimal?)expense.TotalAmount) ?? 0m;

        // --------------------------------------------------------
        // MANUAL CASH IN
        // --------------------------------------------------------

        var manualCashIn =
            await _context.CashAdjustments
                .AsNoTracking()
                .Where(x =>
                    x.CashSessionId == session.CashSessionId &&
                    !x.IsVoided &&
                    x.TransactionType == "CASH_IN" &&
                    x.TransactionDate >= from &&
                    x.TransactionDate < to)
                .SumAsync(x =>
                    (decimal?)x.Amount) ?? 0m;

        // --------------------------------------------------------
        // MANUAL CASH OUT
        // --------------------------------------------------------

        var manualCashOut =
            await _context.CashAdjustments
                .AsNoTracking()
                .Where(x =>
                    x.CashSessionId == session.CashSessionId &&
                    !x.IsVoided &&
                    x.TransactionType == "CASH_OUT" &&
                    x.TransactionDate >= from &&
                    x.TransactionDate < to)
                .SumAsync(x =>
                    (decimal?)x.Amount) ?? 0m;

        // --------------------------------------------------------
        // SYSTEM CASH
        // --------------------------------------------------------

        var systemCash =
            session.OpeningCash
            + cashSales
            - cashPurchases
            - cashExpenses
            + manualCashIn
            - manualCashOut;

        return new CashSummary
        {
            CashSales =
                cashSales,

            CashPurchases =
                cashPurchases,

            CashExpenses =
                cashExpenses,

            ManualCashIn =
                manualCashIn,

            ManualCashOut =
                manualCashOut,

            SystemCash =
                systemCash
        };
    }

    // ============================================================
    // PRIVATE: BUILD LEDGER
    // ============================================================

    private async Task<List<CashLedgerEntryDto>>
        BuildLedgerAsync(
            CashSession session,
            DateTime from,
            DateTime to)
    {
        var entries =
            new List<CashLedgerEntryDto>();

        // --------------------------------------------------------
        // OPENING BALANCE
        // --------------------------------------------------------

        entries.Add(
            new CashLedgerEntryDto
            {
                TransactionDate =
                    session.OpenedAt,

                TransactionType =
                    "OPENING",

                VoucherNumber =
                    null,

                Description =
                    "Opening Cash",

                CashIn =
                    session.OpeningCash,

                CashOut =
                    0m,

                Balance =
                    session.OpeningCash,

                Source =
                    "CASH_SESSION",

                ReferenceId =
                    session.CashSessionId
            });

        // --------------------------------------------------------
        // SALES
        //
        // BUSINESS DATE = SalesInvoice.InvoiceDate
        // DISPLAY DATE = SalesPayment.PaymentDate
        // --------------------------------------------------------

        var sales =
            await _context.SalesPayments
                .AsNoTracking()
                .Where(payment =>
                    payment.PaymentMethodId == 1 &&
                    payment.SalesInvoice.InvoiceDate >= from &&
                    payment.SalesInvoice.InvoiceDate < to &&
                    payment.SalesInvoice
                        .DocumentStatus.StatusCode == "POSTED")
                .Select(payment => new
                {
                    payment.SalesPaymentId,

                    PaymentDate =
                        payment.PaymentDate,

                    Amount =
                        payment.Amount,

                    VoucherNumber =
                        payment.SalesInvoice.InvoiceNumber
                })
                .ToListAsync();

        foreach (var sale in sales)
        {
            entries.Add(
                new CashLedgerEntryDto
                {
                    TransactionDate =
                        sale.PaymentDate,

                    TransactionType =
                        "SALE",

                    VoucherNumber =
                        sale.VoucherNumber,

                    Description =
                        "Cash Sale",

                    CashIn =
                        Math.Abs(sale.Amount),

                    CashOut =
                        0m,

                    Balance =
                        0m,

                    Source =
                        "SALES_PAYMENT",

                    ReferenceId =
                        sale.SalesPaymentId
                });
        }

        // --------------------------------------------------------
        // PURCHASES
        //
        // BUSINESS DATE = PurchaseInvoice.InvoiceDate
        // DISPLAY DATE = PurchasePayment.PaymentDate
        // --------------------------------------------------------

        var purchases =
            await _context.PurchasePayments
                .AsNoTracking()
                .Where(payment =>
                    payment.PaymentMethodId == 1 &&
                    payment.PurchaseInvoice.InvoiceDate >= from &&
                    payment.PurchaseInvoice.InvoiceDate < to &&
                    payment.PurchaseInvoice
                        .DocumentStatus.StatusCode == "POSTED")
                .Select(payment => new
                {
                    payment.PurchasePaymentId,

                    PaymentDate =
                        payment.PaymentDate,

                    Amount =
                        payment.Amount,

                    VoucherNumber =
                        payment.PurchaseInvoice.InvoiceNumber
                })
                .ToListAsync();

        foreach (var purchase in purchases)
        {
            entries.Add(
                new CashLedgerEntryDto
                {
                    TransactionDate =
                        purchase.PaymentDate,

                    TransactionType =
                        "PURCHASE",

                    VoucherNumber =
                        purchase.VoucherNumber,

                    Description =
                        "Cash Purchase",

                    CashIn =
                        0m,

                    CashOut =
                        Math.Abs(purchase.Amount),

                    Balance =
                        0m,

                    Source =
                        "PURCHASE_PAYMENT",

                    ReferenceId =
                        purchase.PurchasePaymentId
                });
        }

        // --------------------------------------------------------
        // EXPENSES
        //
        // PaymentAccountId references Accounts.AccountId.
        //
        // AccountId = 1 is the Cash account.
        // Only posted expenses paid from this Cash account
        // are included in Cash Management.
        // --------------------------------------------------------

        var expenses =
            await _context.Expenses
                .AsNoTracking()
                .Where(expense =>
                    expense.PaymentAccountId == CashAccountId &&
                    expense.ExpenseDate >= from &&
                    expense.ExpenseDate < to &&
                    expense.DocumentStatus.StatusCode == "POSTED")
                .Select(expense => new
                {
                    expense.ExpenseId,

                    TransactionDate =
                        expense.ExpenseDate,

                    Amount =
                        expense.TotalAmount,

                    VoucherNumber =
                        expense.ExpenseNumber,

                    PayeeName =
                        expense.PayeeName
                })
                .ToListAsync();

        foreach (var expense in expenses)
        {
            entries.Add(
                new CashLedgerEntryDto
                {
                    TransactionDate =
                        expense.TransactionDate,

                    TransactionType =
                        "EXPENSE",

                    VoucherNumber =
                        expense.VoucherNumber,

                    Description =
                        string.IsNullOrWhiteSpace(expense.PayeeName)
                            ? "Cash Expense"
                            : $"Cash Expense - {expense.PayeeName}",

                    CashIn =
                        0m,

                    CashOut =
                        Math.Abs(expense.Amount),

                    Balance =
                        0m,

                    Source =
                        "EXPENSE",

                    ReferenceId =
                        expense.ExpenseId
                });
        }

        // --------------------------------------------------------
        // CASH ADJUSTMENTS
        // --------------------------------------------------------

        var adjustments =
            await _context.CashAdjustments
                .AsNoTracking()
                .Where(adjustment =>
                    adjustment.CashSessionId == session.CashSessionId &&
                    !adjustment.IsVoided &&
                    adjustment.TransactionDate >= from &&
                    adjustment.TransactionDate < to)
                .Select(adjustment => new
                {
                    adjustment.CashAdjustmentId,

                    adjustment.TransactionDate,

                    adjustment.TransactionType,

                    adjustment.Amount,

                    adjustment.ReferenceNumber,

                    adjustment.Description
                })
                .ToListAsync();

        foreach (var adjustment in adjustments)
        {
            var isCashIn =
                adjustment.TransactionType == "CASH_IN";

            entries.Add(
                new CashLedgerEntryDto
                {
                    TransactionDate =
                        adjustment.TransactionDate,

                    TransactionType =
                        adjustment.TransactionType,

                    VoucherNumber =
                        adjustment.ReferenceNumber,

                    Description =
                        adjustment.Description ??
                        adjustment.TransactionType,

                    CashIn =
                        isCashIn
                            ? adjustment.Amount
                            : 0m,

                    CashOut =
                        isCashIn
                            ? 0m
                            : adjustment.Amount,

                    Balance =
                        0m,

                    Source =
                        "CASH_ADJUSTMENT",

                    ReferenceId =
                        adjustment.CashAdjustmentId
                });
        }

        // --------------------------------------------------------
        // SORT LEDGER
        // --------------------------------------------------------

        entries =
            entries
                .OrderBy(x =>
                    x.TransactionDate)
                .ThenBy(x =>
                    GetLedgerPriority(x.TransactionType))
                .ToList();

        // --------------------------------------------------------
        // CALCULATE RUNNING BALANCE
        // --------------------------------------------------------

        decimal runningBalance = 0m;

        foreach (var entry in entries)
        {
            runningBalance += entry.CashIn;
            runningBalance -= entry.CashOut;

            entry.Balance =
                runningBalance;
        }

        return entries;
    }

    // ============================================================
    // PRIVATE: LEDGER SORT PRIORITY
    // ============================================================

    private static int GetLedgerPriority(
        string transactionType)
    {
        return transactionType switch
        {
            "OPENING" => 0,
            "SALE" => 1,
            "CASH_IN" => 2,
            "PURCHASE" => 3,
            "EXPENSE" => 4,
            "CASH_OUT" => 5,
            _ => 99
        };
    }

    // ============================================================
    // PRIVATE: CASH SUMMARY CLASS
    // ============================================================

    private sealed class CashSummary
    {
        public decimal CashSales { get; set; }

        public decimal CashPurchases { get; set; }

        public decimal CashExpenses { get; set; }

        public decimal ManualCashIn { get; set; }

        public decimal ManualCashOut { get; set; }

        public decimal SystemCash { get; set; }
    }
}
*/