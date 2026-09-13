using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class Unit
{
    public int UnitId { get; set; }

    public string UnitCode { get; set; } = null!;

    public string UnitName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
