using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class AccountType
{
    public int AccountTypeId { get; set; }

    public string TypeCode { get; set; } = null!;

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
