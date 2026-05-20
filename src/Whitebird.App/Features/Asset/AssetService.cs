using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Reports;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;
using Whitebird.Infra.Features.Supplier;

namespace Whitebird.App.Features.Asset;

public class AssetService : BaseService, IAssetService
{
    private readonly IGenericRepository<AssetEntity> _repository;
    private readonly IAssetReps _assetReps;
    private readonly IAssetTransactionReps _transactionReps;
    private readonly ICategoryReps _categoryReps;
    private readonly ISupplierReps _supplierReps;
    private readonly IOfficeReps _officeReps;
    private readonly IEmployeeReps _employeeReps;
    private readonly IMasterDataService _masterDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    private const int HANDOVER = 1;
    private const int TRANSFER = 2;
    private const int LOAN = 3;
    private const int MAINTENANCE = 6;
    private const int DISPOSAL = 8;

    public AssetService(
        IGenericRepository<AssetEntity> repository,
        IAssetReps assetReps,
        IAssetTransactionReps transactionReps,
        ICategoryReps categoryReps,
        ISupplierReps supplierReps,
        IOfficeReps officeReps,
        IEmployeeReps employeeReps,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<AssetService> logger) : base(logger)
    {
        _repository = repository;
        _assetReps = assetReps;
        _transactionReps = transactionReps;
        _categoryReps = categoryReps;
        _supplierReps = supplierReps;
        _officeReps = officeReps;
        _employeeReps = employeeReps;
        _masterDataService = masterDataService;
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

    public async Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetByOfficeAsync(int officeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetByOfficeWithRelationsAsync(officeId);
            return ServiceResult<IEnumerable<AssetListViewModel>>.Success(assets.Adapt<IEnumerable<AssetListViewModel>>());
        }, "get assets by office");
    }

    public async Task<ServiceResult<AssetDetailViewModel>> CreateAsync(AssetCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.AssetCode))
            return ServiceResult<AssetDetailViewModel>.BadRequest("Asset code is required");

        if (string.IsNullOrWhiteSpace(model.AssetName))
            return ServiceResult<AssetDetailViewModel>.BadRequest("Asset name is required");

        if (model.CategoryId <= 0)
            return ServiceResult<AssetDetailViewModel>.BadRequest("Valid category is required");

        if (await _assetReps.IsAssetCodeExistsAsync(model.AssetCode))
            return ServiceResult<AssetDetailViewModel>.Conflict($"Asset code '{model.AssetCode}' already exists");

        var category = await _categoryReps.GetByIdRawAsync(model.CategoryId);
        if (category == null)
            return ServiceResult<AssetDetailViewModel>.BadRequest($"Category with id {model.CategoryId} does not exist");

        if (model.SupplierId.HasValue && model.SupplierId.Value > 0)
        {
            var supplier = await _supplierReps.GetByIdAsync(model.SupplierId.Value);
            if (supplier == null)
                return ServiceResult<AssetDetailViewModel>.BadRequest($"Supplier with id {model.SupplierId} does not exist");
        }

        if (model.OfficeId.HasValue && model.OfficeId.Value > 0)
        {
            var office = await _officeReps.GetByIdAsync(model.OfficeId.Value);
            if (office == null)
                return ServiceResult<AssetDetailViewModel>.BadRequest($"Office with id {model.OfficeId} does not exist");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<AssetEntity>();
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

            if (await _assetReps.IsAssetCodeExistsAsync(model.AssetCode, id))
                return ServiceResult<AssetDetailViewModel>.Conflict($"Asset code '{model.AssetCode}' already exists");

            var oldCode = existing.AssetCode;
            var oldName = existing.AssetName;

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
                $"Asset updated: Code '{oldCode}' -> '{model.AssetCode}', Name '{oldName}' -> '{model.AssetName}'",
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

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(id);
            if (activeTransaction != null)
                return ServiceResult.BadRequest("Cannot delete asset with active transaction");

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
            var stats = new DashboardStatsViewModel
            {
                TotalAssets = await _assetReps.GetTotalAssetsCountAsync(),
                AvailableAssets = await _assetReps.GetAvailableAssetsCountAsync(),
                AssignedAssets = await _assetReps.GetAssignedAssetsCountAsync(),
                AssetsOnLoan = await _assetReps.GetAssetsOnLoanCountAsync(),
                AssetsInMaintenance = await _assetReps.GetAssetsInMaintenanceCountAsync(),
                ExpiredWarrantyCount = await _assetReps.GetExpiredWarrantyCountAsync(),
                UpcomingMaintenanceCount = await _assetReps.GetUpcomingMaintenanceCountAsync(30),
                TotalAssetValue = await _assetReps.GetTotalAssetValueAsync(),
                OverdueLoanCount = (await _transactionReps.GetOverdueLoansWithRelationsAsync()).Count(),
                PendingApprovals = (await _transactionReps.GetPendingApprovalsWithRelationsAsync()).Count()
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
            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(assetId);

            var currentStatus = DeriveAssetStatus(activeTransaction);
            var isOnLoan = activeTransaction?.TransactionType == LOAN;
            var isInMaintenance = activeTransaction?.TransactionType == MAINTENANCE;
            var isOverdue = activeTransaction?.TransactionType == LOAN
                && activeTransaction.ExpectedReturnDate.HasValue
                && activeTransaction.ExpectedReturnDate.Value < DateTime.Now;

            string? currentHolderName = null;
            if (activeTransaction?.ToEmployeeId.HasValue == true)
            {
                var employee = await _employeeReps.GetByIdAsync(activeTransaction.ToEmployeeId.Value);
                currentHolderName = employee?.FullName;
            }

            var timeline = new List<AssetTimelineEntry>();
            foreach (var txn in history.OrderByDescending(t => t.TransactionDate))
            {
                var transactionTypeName = await GetTransactionTypeName(txn.TransactionType);

                timeline.Add(new AssetTimelineEntry
                {
                    Date = txn.TransactionDate,
                    ActivityType = transactionTypeName ?? txn.TransactionType.ToString(),
                    Description = txn.Notes ?? $"Transaction: {transactionTypeName}",
                    PreviousHolder = txn.FromEmployeeName,
                    NewHolder = txn.ToEmployeeName,
                    PreviousStatus = txn.ConditionBeforeName,
                    NewStatus = txn.ConditionAfterName,
                    Notes = txn.Notes
                });
            }

            var tracking = new AssetTrackingViewModel
            {
                AssetId = asset.AssetId,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                CurrentStatus = currentStatus,
                CategoryName = asset.CategoryName,
                CurrentHolderName = currentHolderName,
                CurrentLocation = asset.OfficeName,
                Condition = asset.AssetConditionName,
                IsOnLoan = isOnLoan,
                IsInMaintenance = isInMaintenance,
                IsOverdue = isOverdue,
                LoanDueDate = activeTransaction?.ExpectedReturnDate,
                TotalTransactions = history.Count(),
                Timeline = timeline
            };

            return ServiceResult<AssetTrackingViewModel>.Success(tracking);
        }, "get asset tracking");
    }

    public async Task<ServiceResult<AssetCurrentStatusDto>> GetCurrentStatusAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
                return ServiceResult<AssetCurrentStatusDto>.NotFound($"Asset with id {assetId} not found");

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(assetId);
            var currentStatus = DeriveAssetStatus(activeTransaction);

            string? currentHolderName = null;
            if (activeTransaction?.ToEmployeeId.HasValue == true)
            {
                var employee = await _employeeReps.GetByIdAsync(activeTransaction.ToEmployeeId.Value);
                currentHolderName = employee?.FullName;
            }

            var dto = new AssetCurrentStatusDto
            {
                AssetId = asset.AssetId,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                CurrentStatus = currentStatus,
                CurrentStatusType = activeTransaction?.TransactionType,
                CurrentHolderName = currentHolderName,
                CurrentHolderId = activeTransaction?.ToEmployeeId,
                CurrentOfficeName = asset.OfficeName,
                CurrentOfficeId = asset.OfficeId,
                ExpectedReturnDate = activeTransaction?.ExpectedReturnDate,
                IsOverdue = activeTransaction?.TransactionType == LOAN
                    && activeTransaction.ExpectedReturnDate.HasValue
                    && activeTransaction.ExpectedReturnDate.Value < DateTime.Now,
                ConditionName = asset.AssetConditionName,
                ConditionCode = asset.AssetCondition
            };

            return ServiceResult<AssetCurrentStatusDto>.Success(dto);
        }, "get asset current status");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionDto>>> GetAssetTransactionHistoryAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
                return ServiceResult<IEnumerable<AssetTransactionDto>>.NotFound($"Asset with id {assetId} not found");

            var history = await _transactionReps.GetAssetTransactionHistoryAsync(assetId);
            var dtos = new List<AssetTransactionDto>();

            foreach (var txn in history.OrderByDescending(t => t.TransactionDate))
            {
                dtos.Add(new AssetTransactionDto
                {
                    AssetTransactionId = txn.AssetTransactionId,
                    TransactionType = txn.TransactionType,
                    TransactionTypeName = await GetTransactionTypeName(txn.TransactionType) ?? txn.TransactionType.ToString(),
                    FromEmployeeId = txn.FromEmployeeId,
                    FromEmployeeName = txn.FromEmployeeName,
                    ToEmployeeId = txn.ToEmployeeId,
                    ToEmployeeName = txn.ToEmployeeName,
                    ToOfficeId = txn.ToLocationId,
                    ToOfficeName = txn.ToLocationName,
                    TransactionDate = txn.TransactionDate,
                    ExpectedReturnDate = txn.ExpectedReturnDate,
                    ActualReturnDate = txn.ActualReturnDate,
                    Notes = txn.Notes,
                    ConditionBefore = txn.ConditionBefore,
                    ConditionBeforeName = txn.ConditionBeforeName,
                    ConditionAfter = txn.ConditionAfter,
                    ConditionAfterName = txn.ConditionAfterName,
                    Approved = txn.Approved,
                    ApprovedBy = txn.ApprovedBy,
                    FromAssetTransactionId = txn.FromAssetTransactionId
                });
            }

            return ServiceResult<IEnumerable<AssetTransactionDto>>.Success(dtos);
        }, "get asset transaction history");
    }

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
            return ServiceResult<int>.BadRequest("No asset IDs provided");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var activatedCount = 0;

            foreach (var id in request.Ids)
            {
                var asset = await _assetReps.GetByIdRawAsync(id);
                if (asset != null && asset.IsActive != request.Activate)
                {
                    asset.IsActive = request.Activate;
                    asset.ModifiedDate = DateTime.Now;
                    asset.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(asset);
                    activatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        "Asset",
                        id,
                        $"Asset '{asset.AssetCode}' {(request.Activate ? "activated" : "deactivated")} via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(activatedCount,
                $"{activatedCount} asset(s) {(request.Activate ? "activated" : "deactivated")} successfully");
        }, "bulk activate assets", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Asset", 0, "Bulk Activate Assets", ex, _currentUserService.GetDisplayName());
        });
    }

    #region Private Helpers

    private string DeriveAssetStatus(AssetTransactionEntity? activeTransaction)
    {
        if (activeTransaction == null)
            return "Available";

        return activeTransaction.TransactionType switch
        {
            HANDOVER or TRANSFER => "Assigned",
            LOAN => "On Loan",
            MAINTENANCE => "In Maintenance",
            DISPOSAL => "Disposed",
            _ => "Available"
        };
    }

    private async Task<string?> GetTransactionTypeName(int transactionType)
    {
        var result = await _masterDataService.GetValueAsync("TransactionType", transactionType);
        return result.IsSuccess ? result.Data : null;
    }

    #endregion
}