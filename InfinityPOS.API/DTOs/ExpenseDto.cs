namespace InfinityPOS.API.DTOs;

public class ExpenseDto
{
    public long ExpenseId { get; set; }

    public string ExpenseNumber { get; set; } = null!;
    public DateTime ExpenseDate { get; set; }

    public string? PayeeName { get; set; }

    public int CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; }

    public decimal TotalAmount { get; set; }

    public int DocumentStatusId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
}