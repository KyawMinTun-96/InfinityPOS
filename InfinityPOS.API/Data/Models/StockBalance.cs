using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class StockBalance
{
    public long StockBalanceId { get; set; }

    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public decimal Quantity { get; set; }

    public decimal AverageCost { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
