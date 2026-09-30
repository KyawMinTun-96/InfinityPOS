using System;
using System.Collections.Generic;

namespace InfinityPOS.API.DTOs;

public class UpdateExpenseDto
{
public DateTime? ExpenseDate { get; set; }


public string? PayeeName { get; set; }

public int CurrencyId { get; set; }

public decimal ExchangeRate { get; set; } = 1m;

public int PaymentAccountId { get; set; }

public decimal TotalAmount { get; set; }

public int? CreatedByUserId { get; set; }

public string? Notes { get; set; }

public List<CreateExpenseItemDto> Items { get; set; } = new();


}
