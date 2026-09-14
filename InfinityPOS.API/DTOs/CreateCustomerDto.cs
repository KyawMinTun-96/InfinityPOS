namespace InfinityPOS.API.DTOs;

public class CreateCustomerDto
{
    public string CustomerCode { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public int? CustomerTypeId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; } = 0;
}