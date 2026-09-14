namespace InfinityPOS.API.DTOs;

public class CustomerTypeDto
{
    public int CustomerTypeId { get; set; }
    public string TypeCode { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public bool IsActive { get; set; }
}