namespace Whitebird.App.Features.Common.Import;

/// <summary>
/// Generic interface for import service
/// </summary>
/// <typeparam name="TDto">The DTO type for the import data</typeparam>
/// <typeparam name="TEntity">The entity type to be created from the DTO</typeparam>
public interface IImportService<TDto, TEntity>
    where TDto : class, new()
    where TEntity : class
{
    /// <summary>
    /// Imports data from a file stream
    /// </summary>
    /// <param name="fileStream">The file stream to import</param>
    /// <param name="createdBy">The user who performed the import</param>
    /// <returns>Import result with success/error counts</returns>
    Task<ServiceResult<ImportResult>> ImportFromFileAsync(Stream fileStream, string? createdBy = null);

    /// <summary>
    /// Generates an import template
    /// </summary>
    /// <returns>Template file bytes</returns>
    Task<ServiceResult<byte[]>> GenerateTemplateAsync();

    /// <summary>
    /// Validates data without importing
    /// </summary>
    /// <param name="fileStream">The file stream to validate</param>
    /// <returns>Validation result</returns>
    Task<ServiceResult<ImportResult>> ValidateOnlyAsync(Stream fileStream);
}