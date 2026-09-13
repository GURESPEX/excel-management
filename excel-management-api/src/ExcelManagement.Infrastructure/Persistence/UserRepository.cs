using ExcelManagement.Application.Auth;
using ExcelManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExcelManagement.Infrastructure.Persistence;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        return db.Users.SingleOrDefaultAsync(u => u.Username == username, ct);
    }
}
