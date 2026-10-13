using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Whitebird.Infra.Configuration;

namespace Whitebird.App.Features.FileAttachment;

public class StorageService : IStorageService
{
    private readonly ILogger<StorageService> _logger;
    private readonly string _basePath;

    public StorageService(IConfiguration configuration, ILogger<StorageService> logger)
    {
        _logger = logger;
        _basePath = StoragePathResolver.Resolve(configuration);
        _logger.LogInformation("Storage path initialized: {BasePath}", _basePath);
    }

    public string GetBasePath()
    {
        return _basePath;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subDirectory)
    {
        var directoryPath = Path.Combine(_basePath, subDirectory);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(directoryPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Path.Combine(subDirectory, fileName).Replace('\\', '/');
    }

    public async Task<string> SaveBytesAsync(byte[] bytes, string originalFileName, string subDirectory)
    {
        if (bytes == null || bytes.Length == 0)
            throw new ArgumentException("Bytes content is empty", nameof(bytes));

        var directoryPath = Path.Combine(_basePath, subDirectory);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var extension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(directoryPath, fileName);

        await File.WriteAllBytesAsync(filePath, bytes);

        return Path.Combine(subDirectory, fileName).Replace('\\', '/');
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file: {FilePath}", fullPath);
        }
        return Task.CompletedTask;
    }

    public async Task<byte[]> ReadFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return await File.ReadAllBytesAsync(fullPath);
    }

    public string ComputeFileHash(IFormFile file)
    {
        using var sha256 = SHA256.Create();
        using var stream = file.OpenReadStream();
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public string ComputeHash(byte[] bytes)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool IsImageFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp";
    }

    public string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant();
    }

    public string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    public long GetFileSize(IFormFile file)
    {
        return file.Length;
    }
}