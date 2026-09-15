namespace InfinityPOS.API.DTOs;

public class UpdatePaymentMethodDto
{
    public string MethodCode { get; set; } = null!;
    public string MethodName { get; set; } = null!;
    public bool IsActive { get; set; }
}