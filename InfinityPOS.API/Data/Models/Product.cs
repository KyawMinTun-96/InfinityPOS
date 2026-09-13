using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Sku { get; set; } = null!;

    public string? Barcode { get; set; }

    public string ProductName { get; set; } = null!;

    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int ProductTypeId { get; set; }

    public int UnitId { get; set; }

    public string? Description { get; set; }

    public bool TrackInventory { get; set; }

    public bool AllowNegativeStock { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductComponent> ProductComponentComponentProducts { get; set; } = new List<ProductComponent>();

    public virtual ICollection<ProductComponent> ProductComponentParentProducts { get; set; } = new List<ProductComponent>();

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();

    public virtual ProductType ProductType { get; set; } = null!;

    public virtual ICollection<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; } = new List<PurchaseInvoiceItem>();

    public virtual ICollection<SalesInvoiceItem> SalesInvoiceItems { get; set; } = new List<SalesInvoiceItem>();

    public virtual ICollection<StockAdjustmentItem> StockAdjustmentItems { get; set; } = new List<StockAdjustmentItem>();

    public virtual ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<StockTransferItem> StockTransferItems { get; set; } = new List<StockTransferItem>();

    public virtual Unit Unit { get; set; } = null!;
}
