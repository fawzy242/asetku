using Microsoft.OpenApi.Models;
using Whitebird.Infra.DependencyInjection;
using Whitebird.App.DependencyInjection;
using FluentMigrator.Runner;
using Whitebird.Api.Middleware;
using Microsoft.AspNetCore.Authentication;
using Whitebird.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ========== SERVICES ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
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
// DISABLE CORS - Allow all origins untuk kemudahan akses
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

// ========== DATABASE MIGRATION ==========
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    if (runner.HasMigrationsToApplyUp()) runner.MigrateUp();
}

// ========== MIDDLEWARE ==========
app.UseCors();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<AuthRateLimitingMiddleware>();

// ========== SWAGGER ==========
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "Asetku API V2");
    c.RoutePrefix = "swagger";
});

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