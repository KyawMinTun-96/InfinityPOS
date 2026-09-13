using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    public string MethodCode { get; set; } = null!;

    public string MethodName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<PurchasePayment> PurchasePayments { get; set; } = new List<PurchasePayment>();

    public virtual ICollection<SalesPayment> SalesPayments { get; set; } = new List<SalesPayment>();
}
