using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class Currency
{
    public int CurrencyId { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string CurrencyName { get; set; } = null!;

    public string? Symbol { get; set; }

    public bool IsBaseCurrency { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();

    public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
}
