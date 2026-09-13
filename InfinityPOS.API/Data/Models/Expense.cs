using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class Expense
{
    public long ExpenseId { get; set; }

    public string ExpenseNumber { get; set; } = null!;

    public DateTime ExpenseDate { get; set; }

    public string? PayeeName { get; set; }

    public int CurrencyId { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal TotalAmount { get; set; }

    public int DocumentStatusId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual DocumentStatus DocumentStatus { get; set; } = null!;

    public virtual ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();
}
