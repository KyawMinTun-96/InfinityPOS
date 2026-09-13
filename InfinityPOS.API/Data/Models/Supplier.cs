using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class Supplier
{
    public int SupplierId { get; set; }

    public string SupplierCode { get; set; } = null!;

    public string SupplierName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
}
