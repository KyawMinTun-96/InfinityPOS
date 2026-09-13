using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class StockMovement
{
    public long StockMovementId { get; set; }

    public int ProductId { get; set; }

    public int WarehouseId { get; set; }

    public int StockMovementTypeId { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public DateTime MovementDate { get; set; }

    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual StockMovementType StockMovementType { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
