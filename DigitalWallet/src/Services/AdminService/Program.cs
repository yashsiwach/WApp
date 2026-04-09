using System.Text;
using AdminService.Application.Interfaces;
using AdminService.Application.Interfaces.Repositories;
using AdminService.Application.Services;
using AdminService.Infrastructure.Consumers;
using AdminService.Infrastructure.Data;
using AdminService.Infrastructure.Repositories;
using AdminService.Middleware;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// ── EF Core ──
builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<RewardsAdminDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RewardsConnection")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT Authentication ──
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSection["Issuer"],
        ValidAudience            = jwtSection["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
        ClockSkew                = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

// ── MassTransit + RabbitMQ ──
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserKYCSubmittedConsumer>();

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

// ── Exception Handlers (strategy pattern — add new handlers here to extend error handling) ──
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.UnauthorizedExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.InvalidOperationExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.NotFoundExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.FallbackExceptionHandler>();

// ── Repositories ──
builder.Services.AddScoped<IKYCReviewRepository, KYCReviewRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

// ── Application Services ──
builder.Services.AddScoped<IKYCManagementService, KYCManagementServiceImpl>();
builder.Services.AddScoped<IDashboardService, DashboardServiceImpl>();
builder.Services.AddScoped<IRewardsCatalogService, RewardsCatalogServiceImpl>();

// ── Controllers ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Admin Service API", Version = "v1" });

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

// ── Auto-migrate (dev) ──
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();

        var adminDb = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        adminDb.Database.Migrate();

        var rewardsDb = scope.ServiceProvider.GetRequiredService<RewardsAdminDbContext>();
        rewardsDb.Database.Migrate();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Development database migration failed. AdminService will continue running.");
    }
}

app.Run();
