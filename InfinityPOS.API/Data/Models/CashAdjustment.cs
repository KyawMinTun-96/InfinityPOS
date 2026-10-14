using System;

namespace InfinityPOS.API.Data.Models;

public partial class CashAdjustment
{
    public long CashAdjustmentId { get; set; }

    public long CashSessionId { get; set; }

    public DateTime TransactionDate { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }

    public int? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsVoided { get; set; }

    public virtual CashSession CashSession { get; set; } = null!;

    public virtual User? CreatedByUser { get; set; }
}
