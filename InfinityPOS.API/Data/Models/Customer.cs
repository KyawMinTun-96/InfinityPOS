using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string CustomerCode { get; set; } = null!;

    public string CustomerName { get; set; } = null!;

    public int? CustomerTypeId { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public decimal CreditLimit { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CustomerType? CustomerType { get; set; }

    public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
}
