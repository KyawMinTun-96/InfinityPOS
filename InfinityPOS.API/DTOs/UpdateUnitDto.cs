namespace InfinityPOS.API.DTOs;

public class UpdateUnitDto
{
    public string UnitCode { get; set; } = null!;
    public string UnitName { get; set; } = null!;
    public bool IsActive { get; set; }
}