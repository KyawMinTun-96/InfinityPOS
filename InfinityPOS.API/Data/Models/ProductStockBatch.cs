using System;

namespace InfinityPOS.API.Data.Models;

public partial class ProductStockBatch
{
    public long ProductStockBatchId { get; set; }

    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public long PurchaseInvoiceItemId { get; set; }

    public decimal CostPrice { get; set; }

    public decimal SalePrice { get; set; }

    public decimal OriginalQuantity { get; set; }

    public decimal RemainingQuantity { get; set; }

    public int CurrencyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;

    public virtual PurchaseInvoiceItem PurchaseInvoiceItem { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;
}