namespace ExcelManagement.Domain;

public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>SHA256 hex hash of the JWT string — the raw token is never stored.</summary>
    public required string TokenHash { get; set; }

    public int UserId { get; set; }

    public DateTime IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }
}
