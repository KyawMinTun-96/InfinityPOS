using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class PriceType
{
    public int PriceTypeId { get; set; }

    public string PriceTypeCode { get; set; } = null!;

    public string PriceTypeName { get; set; } = null!;

    public bool IsSalesPrice { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ProductPrice> ProductPrices { get; set; } = new List<ProductPrice>();
}
