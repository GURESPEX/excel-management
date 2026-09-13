using ExcelManagement.Application.Departments;
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

app.MapGet("/employees/{id:int}", async (int id, IEmployeeRepository repository, CancellationToken ct) =>
{
    var employee = await repository.GetByIdAsync(id, ct);
    return employee is not null ? Results.Ok(employee) : Results.NotFound();
})
.WithName("GetEmployeeById")
.Produces<EmployeeDetailDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

app.MapPost("/employees", async (UpsertEmployeeRequest request, IEmployeeRepository employees, IDepartmentRepository departments, CancellationToken ct) =>
{
    var departmentExists = await departments.ExistsAsync(request.DepartmentId, ct);
    var errors = EmployeeValidator.Validate(request, departmentExists);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var created = await employees.CreateAsync(request, ct);
    return Results.Created($"/employees/{created.Id}", created);
})
.WithName("CreateEmployee")
.Produces<EmployeeDetailDto>(StatusCodes.Status201Created)
.ProducesValidationProblem();

app.MapPut("/employees/{id:int}", async (int id, UpsertEmployeeRequest request, IEmployeeRepository employees, IDepartmentRepository departments, CancellationToken ct) =>
{
    var departmentExists = await departments.ExistsAsync(request.DepartmentId, ct);
    var errors = EmployeeValidator.Validate(request, departmentExists);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var updated = await employees.UpdateAsync(id, request, ct);
    return updated ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateEmployee")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.ProducesValidationProblem();

app.MapDelete("/employees/{id:int}", async (int id, IEmployeeRepository employees, CancellationToken ct) =>
{
    var deleted = await employees.DeleteAsync(id, ct);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteEmployee")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.MapGet("/departments", async (IDepartmentRepository repository, CancellationToken ct) =>
{
    var departments = await repository.GetAllAsync(ct);
    return Results.Ok(departments);
})
.WithName("GetDepartments")
.Produces<IReadOnlyList<DepartmentDto>>(StatusCodes.Status200OK);

app.Run();

public partial class Program;
