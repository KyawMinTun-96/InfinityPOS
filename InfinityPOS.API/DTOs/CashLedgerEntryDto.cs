using System;

namespace InfinityPOS.API.DTOs;

public class CashLedgerEntryDto
{
    public DateTime TransactionDate { get; set; }

    public string TransactionType { get; set; } = null!;

    public string? VoucherNumber { get; set; }

    public string? Description { get; set; }

    public decimal CashIn { get; set; }

    public decimal CashOut { get; set; }

    public decimal Balance { get; set; }

    public string Source { get; set; } = null!;

    public long? ReferenceId { get; set; }
}
