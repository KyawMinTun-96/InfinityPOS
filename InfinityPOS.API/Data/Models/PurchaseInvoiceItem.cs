using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class PurchaseInvoiceItem
{
    public long PurchaseInvoiceItemId { get; set; }

    public long PurchaseInvoiceId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual PurchaseInvoice PurchaseInvoice { get; set; } = null!;
}
