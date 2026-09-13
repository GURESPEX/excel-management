using System.Text;
using ExcelManagement.Api.Auth;
using ExcelManagement.Application.Auth;
using ExcelManagement.Application.Departments;
using ExcelManagement.Application.Employees;
using ExcelManagement.Domain;
using ExcelManagement.Infrastructure;
using ExcelManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(nameof(UserRole.Admin)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db);
}

app.MapPost("/auth/login", async (LoginRequest request, IUserRepository users, JwtTokenService tokens, HttpContext http, CancellationToken ct) =>
{
    var user = await users.GetByUsernameAsync(request.Username, ct);
    if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var principal = new AuthPrincipal(user.Id, user.Username, user.Role);
    IssueAuthCookies(http, tokens, app.Environment, principal);
    return Results.Ok(new AuthenticatedUserDto(principal.Id, principal.Username, principal.Role.ToString()));
})
.WithName("Login")
.Produces<AuthenticatedUserDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.AllowAnonymous();

app.MapPost("/auth/refresh", (HttpContext http, JwtTokenService tokens) =>
{
    if (!http.Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
    {
        return Results.Unauthorized();
    }

    var principal = tokens.ValidateRefreshToken(refreshToken);
    if (principal is null)
    {
        return Results.Unauthorized();
    }

    IssueAuthCookies(http, tokens, app.Environment, principal);
    return Results.Ok(new AuthenticatedUserDto(principal.Id, principal.Username, principal.Role.ToString()));
})
.WithName("RefreshToken")
.Produces<AuthenticatedUserDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.AllowAnonymous();

app.MapPost("/auth/logout", (HttpContext http) =>
{
    http.Response.Cookies.Delete("access_token");
    http.Response.Cookies.Delete("refresh_token");
    return Results.NoContent();
})
.WithName("Logout")
.Produces(StatusCodes.Status204NoContent)
.AllowAnonymous();

app.MapGet("/employees", async (IEmployeeRepository repository, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var result = await repository.GetPagedAsync(page, pageSize, ct);
    return Results.Ok(result);
})
.WithName("GetEmployees")
.Produces<PagedResult<EmployeeListItemDto>>(StatusCodes.Status200OK)
.RequireAuthorization();

app.MapGet("/employees/{id:int}", async (int id, IEmployeeRepository repository, CancellationToken ct) =>
{
    var employee = await repository.GetByIdAsync(id, ct);
    return employee is not null ? Results.Ok(employee) : Results.NotFound();
})
.WithName("GetEmployeeById")
.Produces<EmployeeDetailDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.RequireAuthorization();

app.MapPost("/employees", async (UpsertEmployeeRequest request, IEmployeeRepository employees, IDepartmentRepository departments, CancellationToken ct) =>
{
    var validationError = await ValidateEmployeeRequestAsync(request, departments, ct);
    if (validationError is not null)
    {
        return validationError;
    }

    var created = await employees.CreateAsync(request, ct);
    return Results.Created($"/employees/{created.Id}", created);
})
.WithName("CreateEmployee")
.Produces<EmployeeDetailDto>(StatusCodes.Status201Created)
.ProducesValidationProblem()
.RequireAuthorization("AdminOnly");

app.MapPut("/employees/{id:int}", async (int id, UpsertEmployeeRequest request, IEmployeeRepository employees, IDepartmentRepository departments, CancellationToken ct) =>
{
    var validationError = await ValidateEmployeeRequestAsync(request, departments, ct);
    if (validationError is not null)
    {
        return validationError;
    }

    var updated = await employees.UpdateAsync(id, request, ct);
    return updated ? Results.NoContent() : Results.NotFound();
})
.WithName("UpdateEmployee")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.ProducesValidationProblem()
.RequireAuthorization("AdminOnly");

app.MapDelete("/employees/{id:int}", async (int id, IEmployeeRepository employees, CancellationToken ct) =>
{
    var deleted = await employees.DeleteAsync(id, ct);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteEmployee")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound)
.RequireAuthorization("AdminOnly");

app.MapGet("/departments", async (IDepartmentRepository repository, bool includeInactive = false, CancellationToken ct = default) =>
{
    var departments = await repository.GetAllAsync(includeInactive, ct);
    return Results.Ok(departments);
})
.WithName("GetDepartments")
.Produces<IReadOnlyList<DepartmentDto>>(StatusCodes.Status200OK)
.RequireAuthorization();

app.MapPost("/departments", async (CreateDepartmentRequest request, IDepartmentRepository departments, CancellationToken ct) =>
{
    var nameConflict = await departments.NameExistsAsync(request.Name, null, ct);
    var errors = DepartmentValidator.Validate(request.Name, nameConflict);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var created = await departments.CreateAsync(request.Name, ct);
    return Results.Created($"/departments/{created.Id}", created);
})
.WithName("CreateDepartment")
.Produces<DepartmentDto>(StatusCodes.Status201Created)
.ProducesValidationProblem()
.RequireAuthorization("AdminOnly");

app.MapPut("/departments/{id:int}", async (int id, UpdateDepartmentRequest request, IDepartmentRepository departments, CancellationToken ct) =>
{
    var nameConflict = await departments.NameExistsAsync(request.Name, id, ct);
    var errors = DepartmentValidator.Validate(request.Name, nameConflict);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var updated = await departments.UpdateAsync(id, request.Name, request.IsActive, ct);
    return updated is not null ? Results.Ok(updated) : Results.NotFound();
})
.WithName("UpdateDepartment")
.Produces<DepartmentDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound)
.ProducesValidationProblem()
.RequireAuthorization("AdminOnly");

app.Run();

static async Task<IResult?> ValidateEmployeeRequestAsync(UpsertEmployeeRequest request, IDepartmentRepository departments, CancellationToken ct)
{
    var departmentExists = await departments.ExistsAsync(request.DepartmentId, ct);
    var errors = EmployeeValidator.Validate(request, departmentExists);
    return errors.Count > 0 ? Results.ValidationProblem(errors) : null;
}

static void IssueAuthCookies(HttpContext http, JwtTokenService tokens, IWebHostEnvironment env, AuthPrincipal principal)
{
    var (accessToken, accessExpires) = tokens.CreateAccessToken(principal);
    var (refreshToken, refreshExpires) = tokens.CreateRefreshToken(principal);

    AppendAuthCookie(http, env, "access_token", accessToken, accessExpires);
    AppendAuthCookie(http, env, "refresh_token", refreshToken, refreshExpires);
}

static void AppendAuthCookie(HttpContext http, IWebHostEnvironment env, string name, string value, DateTime expires)
{
    http.Response.Cookies.Append(name, value, new CookieOptions
    {
        HttpOnly = true,
        Secure = !env.IsDevelopment(),
        SameSite = SameSiteMode.Lax,
        Expires = expires,
    });
}

public partial class Program;
