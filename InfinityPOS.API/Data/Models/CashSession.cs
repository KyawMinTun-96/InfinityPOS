using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class CashSession
{
    public long CashSessionId { get; set; }

    public DateTime BusinessDate { get; set; }

    public decimal OpeningCash { get; set; }

    public decimal SystemCash { get; set; }

    public decimal? ClosingCash { get; set; }

    public decimal? Difference { get; set; }

    public string Status { get; set; } = "OPEN";

    public DateTime OpenedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int? OpenedByUserId { get; set; }

    public int? ClosedByUserId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? OpenedByUser { get; set; }

    public virtual User? ClosedByUser { get; set; }

    public virtual ICollection<CashAdjustment> CashAdjustments { get; set; }
        = new List<CashAdjustment>();
}
