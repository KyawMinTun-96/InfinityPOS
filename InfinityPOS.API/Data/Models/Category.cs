using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
