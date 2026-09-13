using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class JournalEntry
{
    public long JournalEntryId { get; set; }

    public string JournalNumber { get; set; } = null!;

    public DateTime JournalDate { get; set; }

    public int? FiscalPeriodId { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? Description { get; set; }

    public int DocumentStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual DocumentStatus DocumentStatus { get; set; } = null!;

    public virtual FiscalPeriod? FiscalPeriod { get; set; }

    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}
