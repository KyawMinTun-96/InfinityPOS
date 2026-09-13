using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class SalesInvoiceItem
{
    public long SalesInvoiceItemId { get; set; }

    public long SalesInvoiceId { get; set; }

    public int ProductId { get; set; }

    public long? ProductPriceId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal UnitCost { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? Cogsamount { get; set; }

    public decimal? GrossProfit { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductPrice? ProductPrice { get; set; }

    public virtual SalesInvoice SalesInvoice { get; set; } = null!;
}
