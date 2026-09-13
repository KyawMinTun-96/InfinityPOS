using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class StockTransfer
{
    public long StockTransferId { get; set; }

    public string TransferNumber { get; set; } = null!;

    public DateTime TransferDate { get; set; }

    public int FromWarehouseId { get; set; }

    public int ToWarehouseId { get; set; }

    public int DocumentStatusId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual DocumentStatus DocumentStatus { get; set; } = null!;

    public virtual Warehouse FromWarehouse { get; set; } = null!;

    public virtual ICollection<StockTransferItem> StockTransferItems { get; set; } = new List<StockTransferItem>();

    public virtual Warehouse ToWarehouse { get; set; } = null!;
}
