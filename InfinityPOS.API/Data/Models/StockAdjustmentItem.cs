using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class StockAdjustmentItem
{
    public long StockAdjustmentItemId { get; set; }

    public long StockAdjustmentId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public bool IsIncrease { get; set; }

    public string? Notes { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual StockAdjustment StockAdjustment { get; set; } = null!;
}
