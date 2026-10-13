using Microsoft.AspNetCore.Http;

namespace Whitebird.App.Features.FileAttachment;

public interface IStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subDirectory);
    Task<string> SaveBytesAsync(byte[] bytes, string originalFileName, string subDirectory);
    Task DeleteFileAsync(string filePath);
    Task<byte[]> ReadFileAsync(string filePath);
    string ComputeFileHash(IFormFile file);
    string ComputeHash(byte[] bytes);
    bool IsImageFile(string fileName);
    string GetFileExtension(string fileName);
    string GetMimeType(string fileName);
    long GetFileSize(IFormFile file);
    string GetBasePath();
}