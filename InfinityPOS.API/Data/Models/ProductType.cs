using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class ProductType
{
    public int ProductTypeId { get; set; }

    public string TypeCode { get; set; } = null!;

    public string TypeName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
