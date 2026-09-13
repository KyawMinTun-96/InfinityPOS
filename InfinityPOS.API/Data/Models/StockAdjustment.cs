using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class StockAdjustment
{
    public long StockAdjustmentId { get; set; }

    public string AdjustmentNumber { get; set; } = null!;

    public DateTime AdjustmentDate { get; set; }

    public int WarehouseId { get; set; }

    public int StockAdjustmentReasonId { get; set; }

    public int DocumentStatusId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual DocumentStatus DocumentStatus { get; set; } = null!;

    public virtual ICollection<StockAdjustmentItem> StockAdjustmentItems { get; set; } = new List<StockAdjustmentItem>();

    public virtual StockAdjustmentReason StockAdjustmentReason { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
