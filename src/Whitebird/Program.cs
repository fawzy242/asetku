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
// H10: explicit authorization services. Standard ASP.NET Core practice — never
// depend on framework-version-specific implicit registration. If the runtime
// would have registered these anyway, this is a no-op; if it would not, this
// is the difference between "every [Authorize] attribute is enforced" and
// "every [Authorize] attribute is a silent no-op". Relying on this being
// implicitly registered is not a safe assumption across .NET versions.
builder.Services.AddAuthorization();
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
// P2-2 (Option A): pass app.Environment.EnvironmentName (a plain string) so
// ConfigurationValidator can fail-closed in Production without requiring
// Whitebird.Infra to reference Microsoft.AspNetCore.Hosting.Abstractions.
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
if (!ConfigurationValidator.Validate(app.Configuration, app.Environment.EnvironmentName, startupLogger))
{
    throw new InvalidOperationException("Application configuration is invalid. See logs for details.");
}

// ========== DATABASE MIGRATION ==========
var migrationRetryPolicy = PollyRetryPolicy.CreateRetryPolicy(startupLogger, maxRetries: 3, retryDelaySeconds: 2);
await migrationRetryPolicy.ExecuteAsync(() =>
{
    using var scope = app.Services.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    if (runner.HasMigrationsToApplyUp()) runner.MigrateUp();
    return Task.CompletedTask;
});

// ========== MIDDLEWARE ==========
// H10: response compression moved to the TOP of the pipeline (before static
// files and the SPA fallback branch). Previously it was placed after the SPA
// branch, which meant static-file responses short-circuited before reaching it,
// and SPA-fallback responses entered a branch that never returned to the main
// pipeline — so neither was compressed. Only API responses were. Microsoft's
// recommended placement is before UseStaticFiles, which is now the case.
app.UseResponseCompression();

app.UseCors();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<AuthRateLimitingMiddleware>();

// ========== SWAGGER ==========
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
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow, environment = app.Environment.EnvironmentName }));

app.Run();