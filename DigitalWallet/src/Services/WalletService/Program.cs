using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using WalletService.Application.Interfaces;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Options;
using WalletService.Application.Services;
using WalletService.Infrastructure.Consumers;
using WalletService.Infrastructure.Data;
using WalletService.Infrastructure.Repositories;
using WalletService.Infrastructure.ServiceClients;
using WalletService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// —— Serilog ——
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// —— EF Core (SQL Server) ——
builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// —— JWT Authentication ——
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

// —— MassTransit + RabbitMQ ——
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<KYCApprovedConsumer>();

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

// —— Options ——
builder.Services.Configure<WalletOptions>(
    builder.Configuration.GetSection(WalletOptions.SectionName));

// —— Exception Handlers (strategy pattern — add new handlers here to extend error handling) ——
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.UnauthorizedExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.InvalidOperationExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.NotFoundExceptionHandler>();
builder.Services.AddSingleton<SharedContracts.Middleware.IExceptionHandler, SharedContracts.Middleware.FallbackExceptionHandler>();

// —— HTTP Clients ——
builder.Services.AddHttpClient<IAuthServiceClient, AuthServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AuthService:BaseUrl"] ?? "http://localhost:5001");
});

// —— Repositories ——
builder.Services.AddScoped<IWalletAccountRepository, WalletAccountRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<ITopUpRepository, TopUpRepository>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<IDailyLimitRepository, DailyLimitRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// —— Application Services ——
builder.Services.AddScoped<WalletServiceImpl>();
builder.Services.AddScoped<IWalletQueryService>(sp   => sp.GetRequiredService<WalletServiceImpl>());
builder.Services.AddScoped<IWalletCommandService>(sp => sp.GetRequiredService<WalletServiceImpl>());

// —— Controllers ——
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Wallet Service API", Version = "v1" });

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

// —— Middleware pipeline ——
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// —— Auto-migrate in development ——
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Development database migration failed. WalletService will continue running.");
    }
}

app.Run();
