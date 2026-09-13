using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<FiscalPeriod> FiscalPeriods { get; set; } = new List<FiscalPeriod>();

    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();

    public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<StockTransfer> StockTransfers { get; set; } = new List<StockTransfer>();
}
