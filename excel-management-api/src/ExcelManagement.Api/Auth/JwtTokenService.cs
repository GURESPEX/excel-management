using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ExcelManagement.Domain;
using Microsoft.IdentityModel.Tokens;

namespace ExcelManagement.Api.Auth;

public record AuthPrincipal(int Id, string Username, UserRole Role);

public class JwtTokenService(IConfiguration configuration)
{
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(configuration["Jwt:SigningKey"]!));
    private readonly int _accessMinutes = configuration.GetValue("Jwt:AccessTokenMinutes", 15);
    private readonly int _refreshDays = configuration.GetValue("Jwt:RefreshTokenDays", 7);

    public (string Token, DateTime ExpiresAt) CreateAccessToken(AuthPrincipal principal) =>
        CreateToken(principal, TimeSpan.FromMinutes(_accessMinutes), "access");

    public (string Token, DateTime ExpiresAt) CreateRefreshToken(AuthPrincipal principal) =>
        CreateToken(principal, TimeSpan.FromDays(_refreshDays), "refresh");

    /// <summary>SHA256 hex hash of a refresh token string, for server-side lookup/revocation without storing the raw JWT.</summary>
    public static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private (string, DateTime) CreateToken(AuthPrincipal principal, TimeSpan lifetime, string tokenType)
    {
        var expires = DateTime.UtcNow.Add(lifetime);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, principal.Id.ToString()),
            new Claim(ClaimTypes.Name, principal.Username),
            new Claim(ClaimTypes.Role, principal.Role.ToString()),
            new Claim("typ", tokenType),
            // Without a nonce, two tokens minted for the same user within the same
            // second-precision `exp` window are byte-identical JWTs — harmless when
            // refresh tokens were purely stateless, but it collides against the
            // RefreshTokens.TokenHash unique index now that tokens are persisted.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(claims: claims, expires: expires, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public AuthPrincipal? ValidateRefreshToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var claimsPrincipal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
            }, out _);

            if (claimsPrincipal.FindFirstValue("typ") != "refresh")
            {
                return null;
            }

            return new AuthPrincipal(
                int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)!),
                claimsPrincipal.FindFirstValue(ClaimTypes.Name)!,
                Enum.Parse<UserRole>(claimsPrincipal.FindFirstValue(ClaimTypes.Role)!));
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}
