using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class DocumentStatus
{
    public int DocumentStatusId { get; set; }

    public string DocumentType { get; set; } = null!;

    public string StatusCode { get; set; } = null!;

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();

    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();

    public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();

    public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();

    public virtual ICollection<StockTransfer> StockTransfers { get; set; } = new List<StockTransfer>();
}
