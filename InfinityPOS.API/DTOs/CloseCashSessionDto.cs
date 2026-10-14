namespace InfinityPOS.API.DTOs;

public class CloseCashSessionDto
{
    public decimal ActualClosingCash { get; set; }

    public int? ClosedByUserId { get; set; }

    public string? Notes { get; set; }
}
