using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class ExpenseItem
{
    public long ExpenseItemId { get; set; }

    public long ExpenseId { get; set; }

    public int AccountId { get; set; }

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Expense Expense { get; set; } = null!;
}
