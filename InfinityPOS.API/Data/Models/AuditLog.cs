using System;
using System.Collections.Generic;

namespace InfinityPOS.API.Data.Models;

public partial class AuditLog
{
    public long AuditLogId { get; set; }

    public int? UserId { get; set; }

    public string ActionType { get; set; } = null!;

    public string? TableName { get; set; }

    public long? RecordId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
