using System;

namespace InfinityPOS.API.DTOs;

public class OpenCashSessionDto
{
    public DateTime? BusinessDate { get; set; }

    public decimal OpeningCash { get; set; }

    public int? OpenedByUserId { get; set; }

    public string? Notes { get; set; }
}
