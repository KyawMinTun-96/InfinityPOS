namespace InfinityPOS.API.DTOs;

public class CreateExpenseDto
{
    public string ExpenseNumber { get; set; } = null!;

    public DateTime? ExpenseDate { get; set; }

    public string? PayeeName { get; set; }

    public int CurrencyId { get; set; }

    public decimal ExchangeRate { get; set; } = 1;

    public decimal TotalAmount { get; set; } = 0;

    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }
}