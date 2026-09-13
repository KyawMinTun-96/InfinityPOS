using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class StockMovementType
{
    public int StockMovementTypeId { get; set; }

    public string MovementCode { get; set; } = null!;

    public string MovementName { get; set; } = null!;

    public string Direction { get; set; } = null!;

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
