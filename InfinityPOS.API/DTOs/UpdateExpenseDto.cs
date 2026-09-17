namespace InfinityPOS.API.DTOs;

public class UpdateExpenseDto
{
    public string ExpenseNumber { get; set; } = null!;

    public DateTime ExpenseDate { get; set; }

    public string? PayeeName { get; set; }

    public int CurrencyId { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public int? CreatedByUserId { get; set; }
}