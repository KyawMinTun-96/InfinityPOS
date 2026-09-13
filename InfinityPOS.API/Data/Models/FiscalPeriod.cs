using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class FiscalPeriod
{
    public int FiscalPeriodId { get; set; }

    public string PeriodName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool IsClosed { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int? ClosedByUserId { get; set; }

    public virtual User? ClosedByUser { get; set; }

    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}
