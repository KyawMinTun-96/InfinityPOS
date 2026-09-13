using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class ProductComponent
{
    public long ProductComponentId { get; set; }

    public int ParentProductId { get; set; }

    public int ComponentProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal? CostAllocationPercent { get; set; }

    public bool IsActive { get; set; }

    public virtual Product ComponentProduct { get; set; } = null!;

    public virtual Product ParentProduct { get; set; } = null!;
}
