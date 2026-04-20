namespace Whitebird.Infra.Configuration;

public class DatabaseSettings
{
    public bool EnableRetryOnFailure { get; set; } = true;
    public int MaxRetryCount { get; set; } = 3;
    public int MaxRetryDelaySeconds { get; set; } = 30;
    public int CommandTimeoutSeconds { get; set; } = 60;
    public bool EnableDetailedErrors { get; set; } = false;
    public int ConnectionPoolSize { get; set; } = 100;
    public int ConnectionLifetimeSeconds { get; set; } = 300;
}