using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.Common.Import;

public interface IImportService<T>
{
    Task<ServiceResult<ImportResult>> ImportFromExcelAsync(Stream fileStream, string? createdBy = null);
    Task<ServiceResult<byte[]>> GenerateTemplateAsync();
    Task<ServiceResult<ImportResult>> ValidateOnlyAsync(Stream fileStream);
}