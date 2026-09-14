namespace ExcelManagement.Domain;

public class AuditLog
{
    public int Id { get; set; }
    public required string EntityName { get; set; }
    public int EntityId { get; set; }
    public AuditAction Action { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime Timestamp { get; set; }

    /// <summary>JSON representation of what changed (property -> value, or property -> {from, to} for updates).</summary>
    public required string Changes { get; set; }
}
