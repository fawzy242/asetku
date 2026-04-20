using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Asset.Interfaces;
using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.Asset.Entities;
using Whitebird.Domain.Features.Asset.View;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Asset.Service;

public class AssetService : BaseService, IAssetService
{
    private readonly IGenericRepository<AssetEntity> _repository;
    private readonly IAssetReps _assetReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public AssetService(
        IGenericRepository<AssetEntity> repository,
        IAssetReps assetReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<AssetService> logger) : base(logger)
    {
        _repository = repository;
        _assetReps = assetReps;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<AssetDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdWithRelationsAsync(id);
            if (asset == null)
                return ServiceResult<AssetDetailViewModel>.NotFound($"Asset with id {id} not found");

            return ServiceResult<AssetDetailViewModel>.Success(asset.Adapt<AssetDetailViewModel>());
        }, "get asset by id");
    }

    public async Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetAllWithRelationsAsync();
            return ServiceResult<IEnumerable<AssetListViewModel>>.Success(assets.Adapt<IEnumerable<AssetListViewModel>>());
        }, "get all assets");
    }

    public async Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetByCategoryAsync(int categoryId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetByCategoryWithRelationsAsync(categoryId);
            return ServiceResult<IEnumerable<AssetListViewModel>>.Success(assets.Adapt<IEnumerable<AssetListViewModel>>());
        }, "get assets by category");
    }

    public async Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetByStatusAsync(string status)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetByStatusWithRelationsAsync(status);
            return ServiceResult<IEnumerable<AssetListViewModel>>.Success(assets.Adapt<IEnumerable<AssetListViewModel>>());
        }, "get assets by status");
    }

    public async Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetByHolderAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetByHolderWithRelationsAsync(employeeId);
            return ServiceResult<IEnumerable<AssetListViewModel>>.Success(assets.Adapt<IEnumerable<AssetListViewModel>>());
        }, "get assets by holder");
    }

    public async Task<ServiceResult<AssetDetailViewModel>> CreateAsync(AssetCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.AssetName))
            return ServiceResult<AssetDetailViewModel>.BadRequest("Asset name is required");
        if (model.CategoryId <= 0)
            return ServiceResult<AssetDetailViewModel>.BadRequest("Valid category is required");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<AssetEntity>();
            entity.AssetCode = await GenerateAssetCodeAsync();

            if (await _assetReps.IsAssetCodeExistsAsync(entity.AssetCode))
                return ServiceResult<AssetDetailViewModel>.Conflict($"Asset code {entity.AssetCode} already exists");

            entity.Status = "Available";
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _assetReps.GetByIdWithRelationsAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    "Asset",
                    created.AssetId,
                    $"Asset '{created.AssetCode}' - '{created.AssetName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<AssetDetailViewModel>.Failure("Failed to retrieve created asset")
                : ServiceResult<AssetDetailViewModel>.Success(created.Adapt<AssetDetailViewModel>(), "Asset created successfully");
        }, "create asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Asset", 0, "Create Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<AssetDetailViewModel>> UpdateAsync(int id, AssetUpdateViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult<AssetDetailViewModel>.NotFound($"Asset with id {id} not found");
            if (existing.Status == "Retired")
                return ServiceResult<AssetDetailViewModel>.BadRequest("Cannot update retired asset");

            var oldCode = existing.AssetCode;
            var oldName = existing.AssetName;
            var oldStatus = existing.Status;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
                return ServiceResult<AssetDetailViewModel>.Failure("Failed to update asset");

            var updated = await _assetReps.GetByIdWithRelationsAsync(id);

            await _activityLogService.LogUpdateAsync(
                "Asset",
                id,
                $"Asset updated: Code '{oldCode}' -> '{existing.AssetCode}', Name '{oldName}' -> '{existing.AssetName}', Status '{oldStatus}' -> '{existing.Status}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetDetailViewModel>.Success(updated!.Adapt<AssetDetailViewModel>(), "Asset updated successfully");
        }, "update asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Asset", id, "Update Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Asset with id {id} not found");
            if (existing.Status == "Assigned")
                return ServiceResult.BadRequest("Cannot delete asset that is currently assigned");

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    "Asset",
                    id,
                    $"Asset '{existing.AssetCode}' - '{existing.AssetName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete asset")
                : ServiceResult.Success("Asset deleted successfully");
        }, "delete asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Asset", id, "Delete Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Asset with id {id} not found");

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    "Asset",
                    id,
                    $"Asset '{existing.AssetCode}' - '{existing.AssetName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete asset")
                : ServiceResult.Success("Asset soft deleted successfully");
        }, "soft delete asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Asset", id, "Soft Delete Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<PaginatedResult<AssetListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetReps.GetPagedWithRelationsAsync(page, pageSize, search, sortBy, sortDescending, filters);
            var viewModels = result.Data.Adapt<List<AssetListViewModel>>();

            return ServiceResult<PaginatedResult<AssetListViewModel>>.Success(new PaginatedResult<AssetListViewModel>
            {
                Data = viewModels,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                Filters = filters,
                SortBy = sortBy,
                SortDescending = sortDescending
            });
        }, "get asset grid data");
    }

    public async Task<ServiceResult<IEnumerable<AssetListViewModel>>> SearchAsync(string keyword)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return ServiceResult<IEnumerable<AssetListViewModel>>.Success(new List<AssetListViewModel>());

            var assets = await _assetReps.GetAllWithRelationsAsync();
            var filtered = assets.Where(a =>
                a.AssetCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                a.AssetName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (a.SerialNumber?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (a.Brand?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (a.Model?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
            );

            return ServiceResult<IEnumerable<AssetListViewModel>>.Success(filtered.Adapt<IEnumerable<AssetListViewModel>>());
        }, "search assets");
    }

    public async Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetExpiredWarrantyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetExpiredWarrantyWithRelationsAsync();
            return ServiceResult<IEnumerable<AssetListViewModel>>.Success(assets.Adapt<IEnumerable<AssetListViewModel>>());
        }, "get expired warranty");
    }

    public async Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetUpcomingMaintenanceAsync(int daysAhead = 30)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetUpcomingMaintenanceWithRelationsAsync(daysAhead);
            return ServiceResult<IEnumerable<AssetListViewModel>>.Success(assets.Adapt<IEnumerable<AssetListViewModel>>());
        }, "get upcoming maintenance");
    }

    public async Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetAllWithRelationsAsync();
            var stats = new DashboardStatsViewModel
            {
                TotalAssets = assets.Count(),
                AvailableAssets = assets.Count(a => a.Status == "Available"),
                AssignedAssets = assets.Count(a => a.Status == "Assigned"),
                UnderRepairAssets = assets.Count(a => a.Status == "Under Repair"),
                RetiredAssets = assets.Count(a => a.Status == "Retired"),
                ExpiredWarrantyCount = assets.Count(a => a.WarrantyExpiryDate < DateTime.Now && a.WarrantyExpiryDate != null),
                UpcomingMaintenanceCount = assets.Count(a => a.NextMaintenanceDate.HasValue && a.NextMaintenanceDate.Value <= DateTime.Now.AddDays(30) && a.NextMaintenanceDate.Value >= DateTime.Now),
                TotalAssetValue = assets.Sum(a => a.PurchasePrice ?? 0)
            };
            return ServiceResult<DashboardStatsViewModel>.Success(stats);
        }, "get dashboard stats");
    }

    private async Task<string> GenerateAssetCodeAsync()
    {
        var nextNumber = await _assetReps.GetNextAssetNumberAsync();
        return $"AST-{nextNumber:D6}";
    }
}