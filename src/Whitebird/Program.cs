using Microsoft.OpenApi.Models;
using Whitebird.Infra.DependencyInjection;
using Whitebird.App.DependencyInjection;
using FluentMigrator.Runner;

var builder = WebApplication.CreateBuilder(args);

// ========== BASIC SERVICES ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// ========== INFRASTRUCTURE SERVICES ==========
builder.Services.AddInfrastructureServices(builder.Configuration);

// ========== APPLICATION SERVICES ==========
builder.Services.AddApplicationServices();
builder.Services.AddMapsterConfiguration();

// ========== SWAGGER ==========
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Whitebird Asset Management API",
        Version = "v1",
        Description = "API for managing assets, tracking transactions, employees, and inventory",
        Contact = new OpenApiContact
        {
            Name = "Whitebird Support",
            Email = "support@whitebird.local"
        }
    });

    c.AddSecurityDefinition("SessionToken", new OpenApiSecurityScheme
    {
        Name = "X-Session-Token",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKey",
        In = ParameterLocation.Header,
        Description = "Session token for authentication"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "SessionToken"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========== CORS ==========
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ========== RESPONSE COMPRESSION ==========
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// ========== RATE LIMITING ==========
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
        httpContext => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// ========== LOGGING ==========
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ========== AUTO DATABASE MIGRATION ==========
using (var scope = app.Services.CreateScope())
{
    Console.WriteLine("=== MIGRATION BLOCK EXECUTED ===");

    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("FluentMigrator");

    try
    {
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        if (runner.HasMigrationsToApplyUp())
        {
            logger.LogInformation("Applying pending migrations...");
            runner.MigrateUp();
            logger.LogInformation("Database migration completed successfully.");
        }
        else
        {
            logger.LogInformation("No pending migrations.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Migration failed during startup.");
        throw;
    }
}

// ========== SWAGGER ==========
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Whitebird API V1");
    c.RoutePrefix = "swagger";
});

// ========== PRODUCTION SECURITY ==========
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// ========== SECURITY HEADERS ==========
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    await next();
});

// TEMPORARY DISABLE FOR IIS HTTP SETUP
// app.UseHttpsRedirection();

app.UseResponseCompression();
app.UseRateLimiter();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ========== HEALTH CHECK ==========
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    version = "1.0.0"
}));

app.Run();