namespace InfinityPOS.API.DTOs;

public class UpdateCustomerTypeDto
{
    public string TypeCode { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public bool IsActive { get; set; }
}