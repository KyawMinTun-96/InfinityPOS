namespace InfinityPOS.API.DTOs;

public class CreateExpenseItemDto
{
    public long ExpenseId { get; set; }
    public int AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}