using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class StockAdjustmentReason
{
    public int StockAdjustmentReasonId { get; set; }

    public string ReasonCode { get; set; } = null!;

    public string ReasonName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
}
