namespace ExcelManagement.Application.Auth;

public interface IRefreshTokenRepository
{
    Task CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct);

    /// <summary>
    /// Looks up a refresh token by its hash. If it exists, is not already revoked, and is
    /// not expired (per the DB row), marks it revoked and returns true. Otherwise returns
    /// false (unknown, already-revoked, or expired token) without changing anything — the
    /// caller should reject the refresh/logout attempt in that case.
    /// </summary>
    Task<bool> RevokeIfActiveAsync(string tokenHash, CancellationToken ct);
}
