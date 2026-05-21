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
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Asetku - Asset Management System API",
        Version = "v2.0.0",
        Description = @"Complete API for managing assets, tracking transactions, employees, offices, departments, and file attachments.

## Key Features:
- **Asset Management**: CRUD, tracking, warranty, maintenance
- **Transaction Management**: HANDOVER, TRANSFER, LOAN, RETURN, LOAN_RETURN, MAINTENANCE, POST_MAINTENANCE, DISPOSAL
- **Employee Management**: CRUD, asset summary, import
- **Master Data**: Centralized lookup for positions, statuses, types
- **File Attachments**: Single/multiple upload, preview for images
- **Reports**: 5 report types with Excel export
- **Import**: Bulk import for Assets and Employees (Excel)

## Authentication:
Use session token from /api/Auth/login
",
        Contact = new OpenApiContact
        {
            Name = "Asetku Support",
            Email = "support@asetku.local"
        }
    });

    c.AddSecurityDefinition("SessionToken", new OpenApiSecurityScheme
    {
        Name = "X-Session-Token",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKey",
        In = ParameterLocation.Header,
        Description = "Session token for authentication (get from /api/Auth/login)"
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

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ========== CORS ==========
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:4200", "http://localhost:8080" };

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Asetku API V2");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Asetku Asset Management API";
    });
}

// ========== SERVE STATIC FILES (Frontend) ==========
// Ini kunci untuk IIS deployment: serve frontend dari wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Fallback: Untuk SPA routing, semua request non-API diarahkan ke index.html
app.MapFallbackToFile("/index.html");

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
    version = "2.0.0",
    modules = new[]
    {
        "Asset", "AssetTransaction", "Auth", "Category", "Department",
        "Employee", "FileAttachment", "MasterData", "Office", "Reports", "Supplier"
    }
}));

app.Run();