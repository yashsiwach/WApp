using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Application.Interfaces.Repositories;
using AuthService.Application.Services;
using AuthService.Infrastructure.Consumers;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using AuthService.Middleware;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

try
{
var builder = WebApplication.CreateBuilder(args);

// ── Logfile directory setup ──
var baseDir = AppDomain.CurrentDomain.BaseDirectory;
var logDir = baseDir.Contains("bin", StringComparison.OrdinalIgnoreCase)
    ? baseDir.Substring(0, baseDir.IndexOf("bin", StringComparison.OrdinalIgnoreCase))
    : baseDir;
logDir = System.IO.Path.Combine(logDir, "Logs");

if (!System.IO.Directory.Exists(logDir))
{
    System.IO.Directory.CreateDirectory(logDir);
}

var logFilePath = System.IO.Path.Combine(logDir, "auth-service-log.txt");
if (!System.IO.File.Exists(logFilePath))
{
    System.IO.File.Create(logFilePath).Dispose();
}

// ── Serilog ──
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(logFilePath, shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

// ── EF Core (SQL Server) ──
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Memory Cache ──
builder.Services.AddMemoryCache();

// ── JWT Authentication ──
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

// ── MassTransit + RabbitMQ ──
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<KYCApprovedConsumer>();
    x.AddConsumer<KYCRejectedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// ── Repositories ──
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IOTPRepository, OTPRepository>();
builder.Services.AddScoped<IKYCRepository, KYCRepository>();

// ── Exception Handlers (strategy pattern — add new handlers here to extend error handling) ──
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.UnauthorizedExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.InvalidOperationExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.NotFoundExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.FallbackExceptionHandler>();

// ── Application Services ──
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthServiceImpl>();
builder.Services.AddScoped<IOTPService, OTPServiceImpl>();
builder.Services.AddScoped<IKYCService, KYCServiceImpl>();

// ── Controllers ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth Service API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Middleware pipeline ──
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ── Auto-migrate in development ──
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        // If tables were created outside of EF (e.g. manual SQL / EnsureCreated), mark
        // InitialCreate as applied before calling Migrate() to avoid the "object already
        // exists" collision.
        ReconcileMigrationHistory(db);
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Development database migration failed. AuthService will continue running.");
    }
}

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AuthService terminated unexpectedly during startup.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}























// ── Reconcile migration history ──────────────────────────────────────────────
// Called before Database.Migrate() to handle the case where the schema was
// created outside EF migrations (manual SQL, EnsureCreated, prior tooling).
// It marks InitialCreate as applied in __EFMigrationsHistory so EF Core skips
// the CREATE TABLE statements that would otherwise conflict with existing tables.
static void ReconcileMigrationHistory(AuthDbContext db)
{
    const string migrationId     = "20260409120805_InitialCreate";
    const string productVersion  = "8.0.12"; // must match snapshot ProductVersion

    // Use a dedicated connection so we don't interfere with EF Core's own connection.
    var connStr = db.Database.GetConnectionString()!;
    using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
    conn.Open();

    // 1. Create __EFMigrationsHistory if it doesn't exist yet.
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = @"
            IF NOT EXISTS (
                SELECT 1 FROM sys.objects
                WHERE name = '__EFMigrationsHistory' AND type = 'U'
            )
            CREATE TABLE [__EFMigrationsHistory] (
                [MigrationId]    nvarchar(150) NOT NULL,
                [ProductVersion] nvarchar(32)  NOT NULL,
                CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
            )";
        cmd.ExecuteNonQuery();
    }

    // 2. Check whether the Users table already exists.
    bool usersExists;
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText =
            "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES " +
            "WHERE TABLE_NAME = 'Users' AND TABLE_SCHEMA = 'dbo'";
        usersExists = (int)cmd.ExecuteScalar()! > 0;
    }

    // 3. Check whether InitialCreate is already recorded.
    bool alreadyRecorded;
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText =
            "SELECT COUNT(1) FROM [__EFMigrationsHistory] WHERE [MigrationId] = @id";
        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@id", migrationId));
        alreadyRecorded = (int)cmd.ExecuteScalar()! > 0;
    }

    // 4. If tables exist but migration isn't recorded, mark it applied.
    if (usersExists && !alreadyRecorded)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) " +
            "VALUES (@id, @ver)";
        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@id",  migrationId));
        cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@ver", productVersion));
        cmd.ExecuteNonQuery();
        Log.Information(
            "Reconciled migration history: marked {MigrationId} as applied " +
            "(schema pre-existed EF migration tracking).", migrationId);
    }
}
