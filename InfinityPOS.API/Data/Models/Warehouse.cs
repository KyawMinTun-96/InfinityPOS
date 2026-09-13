using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class Warehouse
{
    public int WarehouseId { get; set; }

    public string WarehouseCode { get; set; } = null!;

    public string WarehouseName { get; set; } = null!;

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();

    public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();

    public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();

    public virtual ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<StockTransfer> StockTransferFromWarehouses { get; set; } = new List<StockTransfer>();

    public virtual ICollection<StockTransfer> StockTransferToWarehouses { get; set; } = new List<StockTransfer>();
}
