using ExcelManagement.Application.Employees;
using ExcelManagement.Infrastructure;
using ExcelManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db);
}

app.MapGet("/employees", async (IEmployeeRepository repository, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var result = await repository.GetPagedAsync(page, pageSize, ct);
    return Results.Ok(result);
})
.WithName("GetEmployees")
.Produces<PagedResult<EmployeeListItemDto>>(StatusCodes.Status200OK);

app.Run();

public partial class Program;
