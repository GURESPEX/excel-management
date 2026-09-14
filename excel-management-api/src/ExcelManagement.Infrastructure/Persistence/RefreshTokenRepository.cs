using ExcelManagement.Application.Auth;
using ExcelManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExcelManagement.Infrastructure.Persistence;

public class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public async Task CreateAsync(int userId, string tokenHash, DateTime expiresAt, CancellationToken ct)
    {
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> RevokeIfActiveAsync(string tokenHash, CancellationToken ct)
    {
        var token = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
        if (token is null || token.RevokedAt is not null || token.ExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        token.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
