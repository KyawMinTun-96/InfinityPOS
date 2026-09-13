using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class PurchasePayment
{
    public long PurchasePaymentId { get; set; }

    public long PurchaseInvoiceId { get; set; }

    public int PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual PurchaseInvoice PurchaseInvoice { get; set; } = null!;
}
