using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class CustomerType
{
    public int CustomerTypeId { get; set; }

    public string TypeCode { get; set; } = null!;

    public string TypeName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
