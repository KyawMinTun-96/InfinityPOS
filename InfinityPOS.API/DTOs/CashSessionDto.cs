using System;

namespace InfinityPOS.API.DTOs;

public class CashSessionDto
{
    public long CashSessionId { get; set; }

    public DateTime BusinessDate { get; set; }

    public decimal OpeningCash { get; set; }

    public decimal SystemCash { get; set; }

    public decimal? ClosingCash { get; set; }

    public decimal? Difference { get; set; }

    public string Status { get; set; } = null!;

    public DateTime OpenedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int? OpenedByUserId { get; set; }

    public int? ClosedByUserId { get; set; }

    public string? Notes { get; set; }

    public decimal CashSales { get; set; }

    public decimal CashPurchases { get; set; }

    public decimal CashExpenses { get; set; }

    public decimal ManualCashIn { get; set; }

    public decimal ManualCashOut { get; set; }
}
