using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class PurchaseInvoice
{
    public long PurchaseInvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public DateTime InvoiceDate { get; set; }

    public int? SupplierId { get; set; }

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

    public virtual DocumentStatus DocumentStatus { get; set; } = null!;

    public virtual ICollection<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = new List<PurchaseInvoiceItem>();

    public virtual ICollection<PurchasePayment> PurchasePayments { get; set; } = new List<PurchasePayment>();

    public virtual Supplier? Supplier { get; set; }

    public virtual Warehouse Warehouse { get; set; } = null!;
}
