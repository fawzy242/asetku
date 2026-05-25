using Microsoft.Extensions.Configuration;

namespace Whitebird.Infra.Configuration;

public static class StoragePathResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var relativePath = configuration["Storage:BasePath"];

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            relativePath = "Upload";
        }

        var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativePath));

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        return basePath;
    }
}