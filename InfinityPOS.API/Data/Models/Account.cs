using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string AccountCode { get; set; } = null!;

    public string AccountName { get; set; } = null!;

    public int AccountTypeId { get; set; }

    public int? ParentAccountId { get; set; }

    public bool IsControlAccount { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AccountType AccountType { get; set; } = null!;

    public virtual ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();

    public virtual ICollection<Account> InverseParentAccount { get; set; } = new List<Account>();

    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();

    public virtual Account? ParentAccount { get; set; }
}
