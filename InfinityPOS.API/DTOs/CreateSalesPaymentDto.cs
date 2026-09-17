namespace InfinityPOS.API.DTOs;

public class CreateSalesPaymentDto
{
    public long SalesInvoiceId { get; set; }
    public int PaymentMethodId { get; set; }

    public decimal Amount { get; set; }
    public DateTime? PaymentDate { get; set; }

    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}