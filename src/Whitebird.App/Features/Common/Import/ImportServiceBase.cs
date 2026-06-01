using System.Data;
using Microsoft.Extensions.Logging;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Common.Import;

/// <summary>
/// Base abstract class for import services providing common Excel/TXT import functionality
/// </summary>
/// <typeparam name="TDto">The DTO type for the import data</typeparam>
/// <typeparam name="TEntity">The entity type to be created from the DTO</typeparam>
public abstract class ImportServiceBase<TDto, TEntity> : BaseService, IImportService<TDto>
    where TDto : class, new()
    where TEntity : class
{
    protected readonly IGenericRepository<TEntity> _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly string _tableName;
    private readonly string _entityName;

    protected ImportServiceBase(
        IGenericRepository<TEntity> repository,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger logger,
        string tableName,
        string entityName)
        : base(logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
        _tableName = tableName;
        _entityName = entityName;
    }

    /// <summary>
    /// Gets the column template for Excel generation
    /// </summary>
    protected abstract Dictionary<string, string> GetTemplateColumns();

    /// <summary>
    /// Processes a single row from the import file and returns the entity
    /// </summary>
    protected abstract Task<TEntity?> ProcessRowAsync(
        DataRow row,
        int rowNumber,
        ImportResult result,
        Dictionary<string, object> cache,
        string createdBy);

    /// <summary>
    /// Validates entity uniqueness before insertion
    /// </summary>
    protected abstract Task<bool> IsEntityUniqueAsync(TEntity entity, ImportResult result, int rowNumber);

    /// <summary>
    /// Gets the entity identifier for logging
    /// </summary>
    protected abstract string GetEntityIdentifier(TEntity entity);

    /// <summary>
    /// Initializes caches before processing rows
    /// </summary>
    protected virtual Task InitializeCachesAsync(Dictionary<string, object> cache)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual async Task<ServiceResult<ImportResult>> ImportFromExcelAsync(Stream fileStream, string? createdBy = null)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var result = new ImportResult();
            var entitiesToInsert = new List<TEntity>();
            var createdByUser = createdBy ?? _currentUserService.GetDisplayName();
            var cache = new Dictionary<string, object>();

            var dataTable = await ExcelHelper.ReadExcelToDataTableAsync(fileStream, true);
            result.TotalRows = dataTable.Rows.Count;

            await InitializeCachesAsync(cache);

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                var rowNumber = i + 2;

                try
                {
                    var entity = await ProcessRowAsync(row, rowNumber, result, cache, createdByUser);
                    
                    if (entity == null || result.Errors.Any(e => e.RowNumber == rowNumber))
                    {
                        continue;
                    }

                    var isUnique = await IsEntityUniqueAsync(entity, result, rowNumber);
                    if (!isUnique)
                    {
                        continue;
                    }

                    entitiesToInsert.Add(entity);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.AddError(rowNumber, "General", $"Error processing row: {ex.Message}");
                    _logger.LogError(ex, "Error processing {EntityName} import row {RowNumber}", _entityName, rowNumber);
                }
            }

            if (entitiesToInsert.Any())
            {
                await _repository.BulkInsertAsync(entitiesToInsert);

                foreach (var entity in entitiesToInsert)
                {
                    await _activityLogService.LogCreateAsync(
                        _tableName,
                        0,
                        $"{_entityName} '{GetEntityIdentifier(entity)}' imported successfully",
                        createdByUser);
                }
            }

            var message = $"Import completed: {result.SuccessCount} successful, {result.ErrorCount} errors";
            return ServiceResult<ImportResult>.Success(result, message);
        }, $"import {_entityName.ToLowerInvariant()}s", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(_tableName, 0, $"Import {_entityName}s", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public virtual async Task<ServiceResult<byte[]>> GenerateTemplateAsync()
    {
        return await ExecuteSafelyAsync(() =>
        {
            var template = ExcelHelper.GenerateTemplate(
                $"{_entityName}s", 
                GetTemplateColumns(),
                $"{_entityName} Import Template - Fill in the data below.");
            return Task.FromResult(ServiceResult<byte[]>.Success(template));
        }, $"generate {_entityName.ToLowerInvariant()} import template");
    }

    /// <inheritdoc />
    public virtual async Task<ServiceResult<ImportResult>> ValidateOnlyAsync(Stream fileStream)
    {
        var result = new ImportResult();
        return await Task.FromResult(ServiceResult<ImportResult>.Success(result));
    }
}