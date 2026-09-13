using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class SalesInvoice
{
    public long SalesInvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public DateTime InvoiceDate { get; set; }

    public int? CustomerId { get; set; }

    public int WarehouseId { get; set; }

    public int CurrencyId { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public int DocumentStatusId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual Customer? Customer { get; set; }

    public virtual DocumentStatus DocumentStatus { get; set; } = null!;

    public virtual ICollection<SalesInvoiceItem> SalesInvoiceItems { get; set; } = new List<SalesInvoiceItem>();

    public virtual ICollection<SalesPayment> SalesPayments { get; set; } = new List<SalesPayment>();

    public virtual Warehouse Warehouse { get; set; } = null!;
}
