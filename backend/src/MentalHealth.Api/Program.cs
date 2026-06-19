using MentalHealth.Api.Auth;
using MentalHealth.Application;
using MentalHealth.Application.Common.Interfaces;
using MentalHealth.Infrastructure;
using MentalHealth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "frontend";

// --- Services -----------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

// --- Pipeline -----------------------------------------------------------------
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);
app.MapControllers();

await ApplyMigrationsAndSeedAsync(app);

app.Run();

// Best-effort: the app still starts (e.g. to serve Swagger) if the DB is unreachable.
static async Task ApplyMigrationsAndSeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
        logger.LogInformation("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database migration/seed skipped. Is PostgreSQL running and the connection string correct?");
    }
}

// For WebApplicationFactory<Program> in tests.
public partial class Program;
