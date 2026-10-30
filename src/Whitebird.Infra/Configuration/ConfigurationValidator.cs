using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Whitebird.Infra.Configuration;

/// <summary>
/// Startup configuration validator.
///
/// Hard failures (return <c>false</c>, blocking startup):
///   * Missing / empty <c>DefaultConnection</c> — app cannot function without a DB.
///   * Missing / empty <c>Storage:BasePath</c>  — uploads cannot function.
///   * Production environment + SA default credentials (`User Id=sa` or
///     `Password=as`) — refuse to start; forces operators to override via
///     environment variable or a secrets store before running in Production.
///     Non-production environments only warn (local dev uses SA by design).
///
/// Warnings (log only, do not block):
///   * Non-production DefaultConnection appears to use SA credentials.
///   * DefaultConnection does not request <c>Encrypt=True</c>. (The real
///     Encrypt gate is enforced at the connection layer: DatabaseConnectionFactory
///     throws in Production when Encrypt=True is missing — this warning is
///     advisory only.)
///   * SMTP configuration missing / empty — email features are disabled.
///
/// P2-2 note: this method accepts the environment name as a plain string so
/// that Whitebird.Infra (a class library) does not need a reference to
/// Microsoft.AspNetCore.Hosting.Abstractions or Microsoft.Extensions.Hosting.
/// The caller (Program.cs) passes app.Environment.EnvironmentName — the same
/// value that IWebHostEnvironment.IsProduction() compares against.
/// </summary>
public static class ConfigurationValidator
{
    private const string ProductionEnvironmentName = "Production";

    public static bool Validate(
        IConfiguration configuration,
        string? environmentName,
        ILogger logger)
    {
        var isValid = true;
        var isProduction = string.Equals(
            environmentName,
            ProductionEnvironmentName,
            StringComparison.OrdinalIgnoreCase);

        // ============================================================
        // CONNECTION STRING
        // ============================================================
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogError("CONFIGURATION ERROR: DefaultConnection is missing or empty");
            isValid = false;
        }
        else
        {
            var hasSaUserId = connectionString.Contains("User Id=sa", StringComparison.OrdinalIgnoreCase);
            var hasSaPassword =
                connectionString.Contains("Password=as;", StringComparison.OrdinalIgnoreCase)
                || connectionString.TrimEnd(';').EndsWith("Password=as", StringComparison.OrdinalIgnoreCase);

            if (hasSaUserId || hasSaPassword)
            {
                if (isProduction)
                {
                    // P2-2 (Option A): hard failure in Production. The shipped
                    // appsettings.Production.json contains the SA / as default pair,
                    // so shipping without an override would otherwise leave the
                    // backend exposed on the production network with well-known
                    // credentials.
                    logger.LogError(
                        "CONFIGURATION ERROR: DefaultConnection uses SA default credentials in Production. " +
                        "Refusing to start. Override ConnectionStrings:DefaultConnection via environment " +
                        "variable (ConnectionStrings__DefaultConnection) or a secrets store before deploying.");
                    isValid = false;
                }
                else
                {
                    // Non-production: warn only. Local dev and staging commonly
                    // use SA; failing here would break the developer workflow.
                    logger.LogWarning(
                        "CONFIGURATION WARNING: DefaultConnection appears to use default SA credentials. " +
                        "Please configure proper credentials before deploying to production.");
                }
            }
            else if (!connectionString.Contains("Encrypt=True", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "CONFIGURATION WARNING: DefaultConnection does not have Encrypt=True. Consider enabling encryption.");
            }
        }

        // ============================================================
        // STORAGE PATH
        // ============================================================
        var storagePath = configuration["Storage:BasePath"];
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            logger.LogError("CONFIGURATION ERROR: Storage:BasePath is missing or empty");
            isValid = false;
        }

        // ============================================================
        // EMAIL (Warning only, not critical)
        // ============================================================
        var smtpHost = configuration["Email:SmtpHost"];
        var smtpUser = configuration["Email:SmtpUser"];
        var fromEmail = configuration["Email:FromEmail"];

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            logger.LogWarning("CONFIGURATION WARNING: Email:SmtpHost is not configured. Email features will not work.");
        }
        if (string.IsNullOrWhiteSpace(smtpUser))
        {
            logger.LogWarning("CONFIGURATION WARNING: Email:SmtpUser is not configured. Email features will not work.");
        }
        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            logger.LogWarning("CONFIGURATION WARNING: Email:FromEmail is not configured. Email features will not work.");
        }

        return isValid;
    }
}