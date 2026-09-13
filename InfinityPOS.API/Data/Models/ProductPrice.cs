using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class ProductPrice
{
    public long ProductPriceId { get; set; }

    public int ProductId { get; set; }

    public int PriceTypeId { get; set; }

    public decimal Price { get; set; }

    public int CurrencyId { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual PriceType PriceType { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<SalesInvoiceItem> SalesInvoiceItems { get; set; } = new List<SalesInvoiceItem>();
}
