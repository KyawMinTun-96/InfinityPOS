namespace InfinityPOS.API.DTOs;

public class UpdateExpenseItemDto
{
    public int AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}