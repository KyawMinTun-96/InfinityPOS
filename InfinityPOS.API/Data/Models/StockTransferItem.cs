using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class StockTransferItem
{
    public long StockTransferItemId { get; set; }

    public long StockTransferId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual StockTransfer StockTransfer { get; set; } = null!;
}
