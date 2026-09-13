using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class JournalEntryLine
{
    public long JournalEntryLineId { get; set; }

    public long JournalEntryId { get; set; }

    public int AccountId { get; set; }

    public string? Description { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public int? CurrencyId { get; set; }

    public decimal? ExchangeRate { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Currency? Currency { get; set; }

    public virtual JournalEntry JournalEntry { get; set; } = null!;
}
