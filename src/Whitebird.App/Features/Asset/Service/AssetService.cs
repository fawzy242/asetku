using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Asset.Interfaces;
using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.Asset.Entities;
using Whitebird.Domain.Features.Asset.View;
using Whitebird.Domain.Features.AssetTransaction.Enums;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Asset.Service;

public class AssetService : BaseService, IAssetService
{
    private readonly IGenericRepository<AssetEntity> _repository;
    private readonly IAssetReps _assetReps;
    private readonly IAssetTransactionReps _transactionReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public AssetService(
        IGenericRepository<AssetEntity> repository,
        IAssetReps assetReps,
        IAssetTransactionReps transactionReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<AssetService> logger) : base(logger)
    {
        _repository = repository;
        _assetReps = assetReps;
        _transactionReps = transactionReps;
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
                $"Asset updated: Code '{oldCode}', Name '{oldName}' -> '{model.AssetName}', Status '{oldStatus}' -> '{model.Status}'",
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
            if (existing.Status == "Assigned" || existing.Status == "On Loan")
                return ServiceResult.BadRequest("Cannot delete asset that is currently assigned or on loan");

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
                (a.AssetCode != null && a.AssetCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (a.AssetName != null && a.AssetName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (a.SerialNumber != null && a.SerialNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (a.Brand != null && a.Brand.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (a.Model != null && a.Model.Contains(keyword, StringComparison.OrdinalIgnoreCase))
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
            var statusCounts = await _assetReps.GetStatusCountsAsync();
            var expiredWarranty = await _assetReps.GetExpiredWarrantyCountAsync();
            var upcomingMaintenance = await _assetReps.GetUpcomingMaintenanceCountAsync(30);
            var totalValue = await _assetReps.GetTotalAssetValueAsync();
            var overdueLoans = await _transactionReps.GetOverdueLoansWithRelationsAsync();

            var stats = new DashboardStatsViewModel
            {
                TotalAssets = statusCounts.Values.Sum(),
                AvailableAssets = statusCounts.GetValueOrDefault("Available", 0),
                AssignedAssets = statusCounts.GetValueOrDefault("Assigned", 0),
                AssetsOnLoan = statusCounts.GetValueOrDefault("On Loan", 0),
                AssetsInMaintenance = statusCounts.GetValueOrDefault("In Maintenance", 0),
                UnderRepairAssets = statusCounts.GetValueOrDefault("Under Repair", 0),
                DamagedAssets = statusCounts.GetValueOrDefault("Damaged", 0),
                RetiredAssets = statusCounts.GetValueOrDefault("Retired", 0),
                ExpiredWarrantyCount = expiredWarranty,
                UpcomingMaintenanceCount = upcomingMaintenance,
                OverdueLoanCount = overdueLoans.Count(),
                TotalAssetValue = totalValue
            };

            return ServiceResult<DashboardStatsViewModel>.Success(stats);
        }, "get dashboard stats");
    }

    public async Task<ServiceResult<AssetTrackingViewModel>> GetAssetTrackingAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdWithRelationsAsync(assetId);
            if (asset == null)
                return ServiceResult<AssetTrackingViewModel>.NotFound($"Asset with id {assetId} not found");

            var history = await _transactionReps.GetAssetTransactionHistoryAsync(assetId);
            var activeLoan = history.FirstOrDefault(t =>
                t.TransactionType == TransactionTypeConstants.Loan &&
                t.TransactionStatus == "Approved" &&
                t.PairedTransactionId == null);

            var timeline = new List<AssetTimelineEntry>();
            foreach (var txn in history.OrderByDescending(t => t.TransactionDate))
            {
                timeline.Add(new AssetTimelineEntry
                {
                    Date = txn.TransactionDate,
                    ActivityType = txn.TransactionType,
                    Description = txn.Notes ?? $"Transaction: {txn.TransactionType}",
                    PreviousHolder = txn.FromEmployeeName,
                    NewHolder = txn.ToEmployeeName,
                    PreviousStatus = txn.ConditionBefore,
                    NewStatus = txn.ConditionAfter,
                    Notes = txn.Notes
                });
            }

            var tracking = new AssetTrackingViewModel
            {
                AssetId = asset.AssetId,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                CurrentStatus = asset.Status,
                CategoryName = asset.CategoryName,
                CurrentHolderName = asset.CurrentHolderName,
                CurrentLocation = asset.Location,
                Condition = asset.Condition,
                IsOnLoan = asset.Status == "On Loan",
                IsInMaintenance = asset.Status == "In Maintenance",
                IsOverdue = activeLoan?.ExpectedReturnDate.HasValue == true && activeLoan.ExpectedReturnDate.Value < DateTime.Now,
                LoanDueDate = activeLoan?.ExpectedReturnDate,
                TotalTransactions = history.Count(),
                Timeline = timeline
            };

            return ServiceResult<AssetTrackingViewModel>.Success(tracking);
        }, "get asset tracking");
    }

    private async Task<string> GenerateAssetCodeAsync()
    {
        var nextNumber = await _assetReps.GetNextAssetNumberAsync();
        return $"AST-{nextNumber:D6}";
    }
}