namespace InfinityPOS.API.DTOs;

public class UnitDto
{
    public int UnitId { get; set; }
    public string UnitCode { get; set; } = null!;
    public string UnitName { get; set; } = null!;
    public bool IsActive { get; set; }
}