using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RewardsService.Application.Interfaces;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Application.Options;
using RewardsService.Application.Services;
using RewardsService.Infrastructure.Consumers;
using RewardsService.Infrastructure.Data;
using RewardsService.Infrastructure.Repositories;
using RewardsService.Middleware;
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
builder.Services.AddDbContext<RewardsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<TopUpCompletedConsumer>();
    x.AddConsumer<TransferCompletedConsumer>();

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

// ── Options ──
builder.Services.Configure<RewardsOptions>(
    builder.Configuration.GetSection(RewardsOptions.SectionName));

// ── Exception Handlers (strategy pattern — add new handlers here to extend error handling) ──
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.UnauthorizedExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.InvalidOperationExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.NotFoundExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.FallbackExceptionHandler>();

// ── Repositories ──
builder.Services.AddScoped<IRewardsAccountRepository, RewardsAccountRepository>();
builder.Services.AddScoped<IRewardsTransactionRepository, RewardsTransactionRepository>();
builder.Services.AddScoped<IEarnRuleRepository, EarnRuleRepository>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<IRedemptionRepository, RedemptionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ── Application Services ──
builder.Services.AddScoped<RewardsServiceImpl>();
builder.Services.AddScoped<IRewardsQueryService>(sp => sp.GetRequiredService<RewardsServiceImpl>());
builder.Services.AddScoped<IRedemptionService>(sp  => sp.GetRequiredService<RewardsServiceImpl>());
builder.Services.AddScoped<IPointsEarningService>(sp => sp.GetRequiredService<RewardsServiceImpl>());

// ── Controllers ──
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Rewards Service API", Version = "v1" });

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
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RewardsDbContext>();
    db.Database.EnsureCreated();

    // Seed default earn rules if missing (HasData only runs on first EnsureCreated,
    // so existing databases need this explicit upsert).
    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT 1 FROM EarnRules WHERE Id = '11111111-0000-0000-0000-000000000001')
            INSERT INTO EarnRules (Id, Name, TriggerType, PointsPerRupee, IsActive, CreatedAt)
            VALUES ('11111111-0000-0000-0000-000000000001', 'TopUp Earn', 'TopUp', 0.01, 1, '2024-01-01');
        """);
    db.Database.ExecuteSqlRaw("""
        IF NOT EXISTS (SELECT 1 FROM EarnRules WHERE Id = '11111111-0000-0000-0000-000000000002')
            INSERT INTO EarnRules (Id, Name, TriggerType, PointsPerRupee, IsActive, CreatedAt)
            VALUES ('11111111-0000-0000-0000-000000000002', 'Transfer Earn', 'Transfer', 0.005, 1, '2024-01-01');
        """);
}

app.Run();
