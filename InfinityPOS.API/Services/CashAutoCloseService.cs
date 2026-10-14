
using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Services;

public sealed class CashAutoCloseService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CashAutoCloseService> _logger;

    private static readonly TimeSpan CloseTime =
        new(23, 59, 0);

    private const int CashAccountId = 1;

    public CashAutoCloseService(
        IServiceScopeFactory scopeFactory,
        ILogger<CashAutoCloseService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Cash Auto Close Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSessionsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Cash Auto Close failed.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessSessionsAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<InfinityPosDbContext>();

        var now = DateTime.Now;
        var today = now.Date;

        // 1. Close any overdue OPEN sessions.
        var overdueSessions = await db.CashSessions
            .Where(x =>
                x.BusinessDate < today &&
                x.Status == "OPEN")
            .OrderBy(x => x.BusinessDate)
            .ToListAsync(cancellationToken);

        foreach (var session in overdueSessions)
        {
            await CloseSessionAsync(
                db,
                session,
                now,
                cancellationToken);
        }

        // 2. Ensure at least today's session exists.
        var latestSession = await db.CashSessions
            .OrderByDescending(x => x.BusinessDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestSession == null)
        {
            var firstSession = new CashSession
            {
                BusinessDate = today,
                OpeningCash = 0m,
                SystemCash = 0m,
                Status = "OPEN",
                OpenedAt = now,
                CreatedAt = now,
                Notes = "Automatically opened by system."
            };

            db.CashSessions.Add(firstSession);
            await db.SaveChangesAsync(cancellationToken);

            latestSession = firstSession;
        }

        // 3. Backfill missing business dates after the
        // latest existing session, carrying cash forward.
        var nextDate = latestSession.BusinessDate.Date.AddDays(1);

        while (nextDate <= today)
        {
            var previousSession = await db.CashSessions
                .Where(x => x.BusinessDate < nextDate)
                .OrderByDescending(x => x.BusinessDate)
                .FirstAsync(cancellationToken);

            var openingCash =
                previousSession.ClosingCash
                ?? previousSession.SystemCash;

            var session = await db.CashSessions
                .SingleOrDefaultAsync(
                    x => x.BusinessDate == nextDate,
                    cancellationToken);

            if (session == null)
            {
                session = new CashSession
                {
                    BusinessDate = nextDate,
                    OpeningCash = openingCash,
                    SystemCash = openingCash,
                    Status = "OPEN",
                    OpenedAt = now,
                    CreatedAt = now,
                    Notes = "Automatically opened by system."
                };

                db.CashSessions.Add(session);
                await db.SaveChangesAsync(cancellationToken);
            }

            // Past dates must be closed. Today's session
            // remains open until 23:59.
            if (nextDate < today)
            {
                if (session.Status == "OPEN")
                {
                    await CloseSessionAsync(
                        db,
                        session,
                        now,
                        cancellationToken);
                }
            }

            nextDate = nextDate.AddDays(1);
        }

        // 4. Get today's session.
        var todaySession = await db.CashSessions
            .SingleOrDefaultAsync(
                x => x.BusinessDate == today,
                cancellationToken);

        if (todaySession == null)
        {
            // This can occur if the database had no session
            // for today and no earlier session was available.
            var previous = await db.CashSessions
                .Where(x => x.BusinessDate < today)
                .OrderByDescending(x => x.BusinessDate)
                .FirstOrDefaultAsync(cancellationToken);

            var openingCash = previous == null
                ? 0m
                : previous.ClosingCash ?? previous.SystemCash;

            todaySession = new CashSession
            {
                BusinessDate = today,
                OpeningCash = openingCash,
                SystemCash = openingCash,
                Status = "OPEN",
                OpenedAt = now,
                CreatedAt = now,
                Notes = "Automatically opened by system."
            };

            db.CashSessions.Add(todaySession);
            await db.SaveChangesAsync(cancellationToken);
        }

        // 5. Close today at 23:59.
        if (now.TimeOfDay >= CloseTime &&
            todaySession.Status == "OPEN")
        {
            await CloseSessionAsync(
                db,
                todaySession,
                now,
                cancellationToken);
        }

        // 6. After closing, create tomorrow's session.
        if (now.TimeOfDay >= CloseTime)
        {
            var tomorrow = today.AddDays(1);

            var tomorrowSession = await db.CashSessions
                .SingleOrDefaultAsync(
                    x => x.BusinessDate == tomorrow,
                    cancellationToken);

            if (tomorrowSession == null)
            {
                var closingCash =
                    todaySession.ClosingCash
                    ?? todaySession.SystemCash;

                db.CashSessions.Add(new CashSession
                {
                    BusinessDate = tomorrow,
                    OpeningCash = closingCash,
                    SystemCash = closingCash,
                    Status = "OPEN",
                    OpenedAt = now,
                    CreatedAt = now,
                    Notes = "Automatically opened by system."
                });

                await db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Next-day session created for {Date} with opening cash {Cash}.",
                    tomorrow,
                    closingCash);
            }
        }
    }

    private  async Task CloseSessionAsync(
        InfinityPosDbContext db,
        CashSession session,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (session.Status != "OPEN")
            return;

        var from = session.BusinessDate.Date;
        var to = from.AddDays(1);

        var cashSales = await db.SalesPayments
            .Where(p =>
                p.PaymentMethodId == 1 &&
                p.SalesInvoice.InvoiceDate >= from &&
                p.SalesInvoice.InvoiceDate < to &&
                p.SalesInvoice.DocumentStatus.StatusCode == "POSTED")
            .SumAsync(
                p => (decimal?)p.Amount,
                cancellationToken) ?? 0m;

        var cashPurchases = await db.PurchasePayments
            .Where(p =>
                p.PaymentMethodId == 1 &&
                p.PurchaseInvoice.InvoiceDate >= from &&
                p.PurchaseInvoice.InvoiceDate < to &&
                p.PurchaseInvoice.DocumentStatus.StatusCode == "POSTED")
            .SumAsync(
                p => (decimal?)p.Amount,
                cancellationToken) ?? 0m;

        var cashExpenses = await db.Expenses
            .Where(e =>
                e.PaymentAccountId == CashAccountId &&
                e.ExpenseDate >= from &&
                e.ExpenseDate < to &&
                e.DocumentStatus.StatusCode == "POSTED")
            .SumAsync(
                e => (decimal?)e.TotalAmount,
                cancellationToken) ?? 0m;

        var cashIn = await db.CashAdjustments
            .Where(a =>
                a.CashSessionId == session.CashSessionId &&
                !a.IsVoided &&
                a.TransactionType == "CASH_IN" &&
                a.TransactionDate >= from &&
                a.TransactionDate < to)
            .SumAsync(
                a => (decimal?)a.Amount,
                cancellationToken) ?? 0m;

        var cashOut = await db.CashAdjustments
            .Where(a =>
                a.CashSessionId == session.CashSessionId &&
                !a.IsVoided &&
                a.TransactionType == "CASH_OUT" &&
                a.TransactionDate >= from &&
                a.TransactionDate < to)
            .SumAsync(
                a => (decimal?)a.Amount,
                cancellationToken) ?? 0m;

        var systemCash =
            session.OpeningCash
            + cashSales
            - cashPurchases
            - cashExpenses
            + cashIn
            - cashOut;

        session.SystemCash = systemCash;
        session.ClosingCash = systemCash;
        session.Difference = 0m;
        session.Status = "CLOSED";
        session.ClosedAt = now;
        session.ClosedByUserId = null;

        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cash session {Id} closed for {Date}. Closing cash: {Cash}.",
            session.CashSessionId,
            session.BusinessDate,
            systemCash);
    }
}
