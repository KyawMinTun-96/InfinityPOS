namespace InfinityPOS.API.DTOs;

public class CreatePaymentMethodDto
{
    public string MethodCode { get; set; } = null!;
    public string MethodName { get; set; } = null!;
}