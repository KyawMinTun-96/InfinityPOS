namespace InfinityPOS.API.DTOs;

public class PaymentMethodDto
{
    public int PaymentMethodId { get; set; }
    public string MethodCode { get; set; } = null!;
    public string MethodName { get; set; } = null!;
    public bool IsActive { get; set; }
}