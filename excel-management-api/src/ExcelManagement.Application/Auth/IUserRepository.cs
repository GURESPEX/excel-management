using ExcelManagement.Domain;

namespace ExcelManagement.Application.Auth;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
}
