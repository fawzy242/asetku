using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Whitebird.Infra.Configuration;

public static class ConfigurationValidator
{
    public static bool Validate(IConfiguration configuration, ILogger logger)
    {
        var isValid = true;

        // Validate Connection String
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogError("CONFIGURATION ERROR: DefaultConnection is missing or empty");
            isValid = false;
        }
        else if (connectionString.Contains("sa") && connectionString.Contains("Password=as"))
        {
            logger.LogError("CONFIGURATION ERROR: DefaultConnection contains default credentials. Please configure proper credentials.");
            isValid = false;
        }
        else if (!connectionString.Contains("Encrypt=True"))
        {
            logger.LogWarning("CONFIGURATION WARNING: DefaultConnection does not have Encrypt=True. Consider enabling encryption.");
        }

        // Validate Storage Path
        var storagePath = configuration["Storage:BasePath"];
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            logger.LogError("CONFIGURATION ERROR: Storage:BasePath is missing or empty");
            isValid = false;
        }

        // Validate Email (Warning only, not critical)
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