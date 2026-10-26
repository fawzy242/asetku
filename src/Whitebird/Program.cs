using Microsoft.OpenApi.Models;
using Whitebird.Infra.DependencyInjection;
using Whitebird.App.DependencyInjection;
using FluentMigrator.Runner;
using Whitebird.Api.Middleware;
using Microsoft.AspNetCore.Authentication;
using Whitebird.Middleware;
using Whitebird.Infra.Configuration;
using Whitebird.Infra.Resilience;

var builder = WebApplication.CreateBuilder(args);

// ========== SERVICES ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// NOTE: AddHttpContextAccessor() is called inside AddApplicationServices().
// Do not add a second registration here.
builder.Services.AddAuthentication("Session").AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>("Session", null);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddMapsterConfiguration();

// ========== SWAGGER ==========
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Asetku API",
        Version = "v2.0.0",
        Description = "Asset Management System API"
    });
    c.AddSecurityDefinition("SessionToken", new OpenApiSecurityScheme
    {
        Name = "X-Session-Token",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Session token from /api/Auth/login"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "SessionToken" } }, Array.Empty<string>() }
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// ========== CORS ==========
// Production-ready CORS: restrict to configured origins
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? new[] { "http://localhost:3000", "http://localhost:4200", "http://localhost:8080" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// ========== OTHER SERVICES ==========
builder.Services.AddResponseCompression(options => options.EnableForHttps = false);
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
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ========== CONFIGURATION VALIDATION ==========
// Fail fast on invalid configuration (missing connection string, missing storage path,
// default credentials, etc.). Previously the ConfigurationValidator helper existed but
// was never invoked.
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
if (!ConfigurationValidator.Validate(app.Configuration, startupLogger))
{
    throw new InvalidOperationException("Application configuration is invalid. See logs for details.");
}

// ========== DATABASE MIGRATION ==========
// Wrapped in a Polly retry policy so transient SQL errors during startup do not take
// the app down. The policy is only invoked here; queries are not retried at runtime
// (TransactionScope semantics are not retry-friendly).
var migrationRetryPolicy = PollyRetryPolicy.CreateRetryPolicy(startupLogger, maxRetries: 3, retryDelaySeconds: 2);
await migrationRetryPolicy.ExecuteAsync(() =>
{
    using var scope = app.Services.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    if (runner.HasMigrationsToApplyUp()) runner.MigrateUp();
    return Task.CompletedTask;
});

// ========== MIDDLEWARE ==========
app.UseCors();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<AuthRateLimitingMiddleware>();

// ========== SWAGGER ==========
// Swagger is exposed only when Swagger:Enabled is true. Development defaults to true;
// Production config explicitly sets it to false. This prevents accidental API-doc
// exposure on public deployments.
var swaggerEnabled = app.Configuration.GetValue<bool?>("Swagger:Enabled")
                     ?? app.Environment.IsDevelopment();

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Asetku API V2");
        c.RoutePrefix = "swagger";
    });
}

// ========== STATIC FILES & SPA ==========
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapWhen(ctx => !ctx.Request.Path.StartsWithSegments("/api") &&
                   !ctx.Request.Path.StartsWithSegments("/swagger") &&
                   !ctx.Request.Path.StartsWithSegments("/health"), appBuilder =>
{
    appBuilder.Use(async (context, next) => { context.Request.Path = "/index.html"; await next(); });
    appBuilder.UseStaticFiles();
});

// ========== ENDPOINTS ==========
app.UseResponseCompression();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow, environment = app.Environment.EnvironmentName }));

app.Run();