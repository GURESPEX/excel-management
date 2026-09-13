using Microsoft.EntityFrameworkCore;
using ExcelManagement.Api.Models;

namespace ExcelManagement.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
}
