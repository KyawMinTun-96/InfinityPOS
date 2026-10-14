using System;

namespace InfinityPOS.API.DTOs;

public class CreateCashAdjustmentDto
{
    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Description { get; set; }

    public int? CreatedByUserId { get; set; }
}
