using Microsoft.AspNetCore.Http;

namespace Whitebird.App.Features.FileAttachment;

public interface IStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subDirectory);
    Task DeleteFileAsync(string filePath);
    Task<byte[]> ReadFileAsync(string filePath);
    string GetFileUrl(string filePath, HttpRequest request);
    string ComputeFileHash(IFormFile file);
    bool IsImageFile(string fileName);
    string GetFileExtension(string fileName);
    string GetMimeType(string fileName);
    long GetFileSize(IFormFile file);
}