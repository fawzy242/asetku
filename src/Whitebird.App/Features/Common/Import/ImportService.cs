using System.Data;
using Microsoft.Extensions.Logging;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Common.Import;

public abstract class ImportService<TDto, TEntity> : BaseService, IImportService<TDto, TEntity>
    where TDto : class, new()
    where TEntity : class
{
    protected readonly ICommonReps _repository;
    protected readonly ICurrentUserService _currentUserService;
    protected readonly IActivityLogService _activityLogService;
    protected readonly string _tableName;
    protected readonly string _entityName;

    protected ImportService(
        ICommonReps repository,
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

    protected abstract Dictionary<string, string> GetTemplateColumns();
    protected abstract Task<TEntity?> ProcessRowAsync(DataRow row, int rowNumber, ImportResult result, Dictionary<string, object> cache, string createdBy);
    protected abstract Task<bool> IsEntityUniqueAsync(TEntity entity, ImportResult result, int rowNumber);
    protected abstract string GetEntityIdentifier(TEntity entity);

    protected virtual Task InitializeCachesAsync(Dictionary<string, object> cache) => Task.CompletedTask;

    public virtual async Task<ServiceResult<ImportResult>> ImportFromFileAsync(Stream fileStream, string? createdBy = null)
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

            // Do NOT increment result.SuccessCount inside the loop. SuccessCount must
            // reflect how many rows were actually persisted.
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                var rowNumber = i + 2;

                try
                {
                    var entity = await ProcessRowAsync(row, rowNumber, result, cache, createdByUser);
                    if (entity == null || result.Errors.Any(e => e.RowNumber == rowNumber)) continue;

                    if (!await IsEntityUniqueAsync(entity, result, rowNumber)) continue;

                    entitiesToInsert.Add(entity);
                }
                catch (Exception ex)
                {
                    result.AddError(rowNumber, "General", $"Error processing row: {ex.Message}");
                    _logger.LogError(ex, "Error processing {EntityName} import row {RowNumber}", _entityName, rowNumber);
                }
            }

            if (entitiesToInsert.Any())
            {
                var insertedCount = await _repository.BulkInsertOptimizedAsync(entitiesToInsert);
                result.SuccessCount = insertedCount;

                if (insertedCount < entitiesToInsert.Count)
                {
                    var missing = entitiesToInsert.Count - insertedCount;
                    result.Errors.Add(new ImportError
                    {
                        RowNumber = -1,
                        Column = "BulkInsert",
                        Message = $"{missing} row(s) were not persisted (bulk insert returned {insertedCount} of {entitiesToInsert.Count})"
                    });
                    result.ErrorCount += missing;
                }

                foreach (var entity in entitiesToInsert)
                {
                    await _activityLogService.LogCreateAsync(
                        _tableName, 0,
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

    public virtual async Task<ServiceResult<ImportResult>> ValidateOnlyAsync(Stream fileStream)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = new ImportResult();
            var createdByUser = _currentUserService.GetDisplayName();
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
                    if (entity == null || result.Errors.Any(e => e.RowNumber == rowNumber)) continue;

                    if (!await IsEntityUniqueAsync(entity, result, rowNumber)) continue;

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.AddError(rowNumber, "General", $"Error processing row: {ex.Message}");
                    _logger.LogError(ex, "Error validating {EntityName} import row {RowNumber}", _entityName, rowNumber);
                }
            }

            var message = $"Validation completed: {result.SuccessCount} valid, {result.ErrorCount} errors";
            return ServiceResult<ImportResult>.Success(result, message);
        }, $"validate {_entityName.ToLowerInvariant()}s import");
    }
}