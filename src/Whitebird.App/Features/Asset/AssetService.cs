using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Reports;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;
using Whitebird.Infra.Features.Supplier;

namespace Whitebird.App.Features.Asset;

/// <summary>
/// Service implementation for Asset business logic
/// </summary>
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
    private readonly IMasterDataLookupService _masterDataLookupService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public AssetService(
        IGenericRepository<AssetEntity> repository,
        IAssetReps assetReps,
        IAssetTransactionReps transactionReps,
        ICategoryReps categoryReps,
        ISupplierReps supplierReps,
        IOfficeReps officeReps,
        IEmployeeReps employeeReps,
        IMasterDataService masterDataService,
        IMasterDataLookupService masterDataLookupService,
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
        _masterDataLookupService = masterDataLookupService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AssetDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetDetailByIdAsync(id);
            if (asset == null)
            {
                return ServiceResult<AssetDetailView>.NotFound($"Asset with id {id} not found");
            }
            return ServiceResult<AssetDetailView>.Success(asset);
        }, "get asset by id");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<AssetListView>>.Success(assets);
        }, "get all assets");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetListView>>> GetByCategoryAsync(int categoryId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var category = await _categoryReps.GetByIdRawAsync(categoryId);
            if (category == null)
            {
                return ServiceResult<IEnumerable<AssetListView>>.NotFound($"Category with id {categoryId} not found");
            }
            
            var assets = await _assetReps.GetByCategoryListViewAsync(categoryId);
            return ServiceResult<IEnumerable<AssetListView>>.Success(assets);
        }, "get assets by category");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetListView>>> GetByOfficeAsync(int officeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var office = await _officeReps.GetByIdRawAsync(officeId);
            if (office == null)
            {
                return ServiceResult<IEnumerable<AssetListView>>.NotFound($"Office with id {officeId} not found");
            }
            
            var assets = await _assetReps.GetByOfficeListViewAsync(officeId);
            return ServiceResult<IEnumerable<AssetListView>>.Success(assets);
        }, "get assets by office");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AssetDetailView>> CreateAsync(AssetCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.AssetCode))
        {
            return ServiceResult<AssetDetailView>.BadRequest("Asset code is required");
        }

        if (string.IsNullOrWhiteSpace(model.AssetName))
        {
            return ServiceResult<AssetDetailView>.BadRequest("Asset name is required");
        }

        if (model.CategoryId <= 0)
        {
            return ServiceResult<AssetDetailView>.BadRequest("Valid category is required");
        }

        var codeExists = await _assetReps.IsAssetCodeExistsAsync(model.AssetCode);
        if (codeExists)
        {
            return ServiceResult<AssetDetailView>.Conflict($"Asset code '{model.AssetCode}' already exists");
        }

        var category = await _categoryReps.GetByIdRawAsync(model.CategoryId);
        if (category == null)
        {
            return ServiceResult<AssetDetailView>.BadRequest($"Category with id {model.CategoryId} does not exist");
        }

        if (model.SupplierId.HasValue && model.SupplierId.Value > 0)
        {
            var supplier = await _supplierReps.GetByIdRawAsync(model.SupplierId.Value);
            if (supplier == null)
            {
                return ServiceResult<AssetDetailView>.BadRequest($"Supplier with id {model.SupplierId} does not exist");
            }
        }

        if (model.OfficeId.HasValue && model.OfficeId.Value > 0)
        {
            var office = await _officeReps.GetByIdRawAsync(model.OfficeId.Value);
            if (office == null)
            {
                return ServiceResult<AssetDetailView>.BadRequest($"Office with id {model.OfficeId} does not exist");
            }
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<AssetEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _assetReps.GetDetailByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.Asset,
                    created.AssetId,
                    $"Asset '{created.AssetCode}' - '{created.AssetName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<AssetDetailView>.Failure("Failed to retrieve created asset")
                : ServiceResult<AssetDetailView>.Success(created, "Asset created successfully");
        }, "create asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Asset, 0, "Create Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AssetDetailView>> UpdateAsync(int id, AssetUpdateViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<AssetDetailView>.NotFound($"Asset with id {id} not found");
            }

            var codeExists = await _assetReps.IsAssetCodeExistsAsync(model.AssetCode, id);
            if (codeExists)
            {
                return ServiceResult<AssetDetailView>.Conflict($"Asset code '{model.AssetCode}' already exists");
            }

            var oldCode = existing.AssetCode;
            var oldName = existing.AssetName;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<AssetDetailView>.Failure("Failed to update asset");
            }

            var updated = await _assetReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.Asset,
                id,
                $"Asset updated: Code '{oldCode}' -> '{model.AssetCode}', Name '{oldName}' -> '{model.AssetName}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetDetailView>.Success(updated!, "Asset updated successfully");
        }, "update asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Asset, id, "Update Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Asset with id {id} not found");
            }

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(id);
            if (activeTransaction != null)
            {
                return ServiceResult.BadRequest("Cannot delete asset with active transaction");
            }

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    TableNames.Asset,
                    id,
                    $"Asset '{existing.AssetCode}' - '{existing.AssetName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete asset")
                : ServiceResult.Success("Asset deleted successfully");
        }, "delete asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Asset, id, "Delete Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Asset with id {id} not found");
            }

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    TableNames.Asset,
                    id,
                    $"Asset '{existing.AssetCode}' - '{existing.AssetName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete asset")
                : ServiceResult.Success("Asset soft deleted successfully");
        }, "soft delete asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Asset, id, "Soft Delete Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaginatedResult<AssetListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetReps.GetPagedListAsync(page, pageSize, search, sortBy, sortDescending, filters);
            return ServiceResult<PaginatedResult<AssetListView>>.Success(result);
        }, "get asset grid data");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetListView>>> SearchAsync(string keyword)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            {
                return ServiceResult<IEnumerable<AssetListView>>.Success(new List<AssetListView>());
            }

            var assets = await _assetReps.SearchAssetsAsync(keyword, 10);
            return ServiceResult<IEnumerable<AssetListView>>.Success(assets);
        }, "search assets");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetListView>>> GetExpiredWarrantyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetExpiredWarrantyListViewAsync();
            return ServiceResult<IEnumerable<AssetListView>>.Success(assets);
        }, "get expired warranty");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetListView>>> GetUpcomingMaintenanceAsync(int daysAhead = 30)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetUpcomingMaintenanceListViewAsync(daysAhead);
            return ServiceResult<IEnumerable<AssetListView>>.Success(assets);
        }, "get upcoming maintenance");
    }

    /// <inheritdoc />
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
                OverdueLoanCount = (await _transactionReps.GetOverdueLoansListViewAsync()).Count(),
                PendingApprovals = (await _transactionReps.GetPendingApprovalsListViewAsync()).Count()
            };
            return ServiceResult<DashboardStatsViewModel>.Success(stats);
        }, "get dashboard stats");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AssetTrackingViewModel>> GetAssetTrackingAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetDetailByIdAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<AssetTrackingViewModel>.NotFound($"Asset with id {assetId} not found");
            }

            var history = await _transactionReps.GetAssetTransactionHistoryAsync(assetId);
            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(assetId);

            var currentStatus = DeriveAssetStatus(activeTransaction);
            var isOnLoan = activeTransaction?.TransactionType == TransactionTypeConstants.LOAN;
            var isInMaintenance = activeTransaction?.TransactionType == TransactionTypeConstants.MAINTENANCE;
            var isOverdue = activeTransaction?.TransactionType == TransactionTypeConstants.LOAN
                && activeTransaction.ExpectedReturnDate.HasValue
                && activeTransaction.ExpectedReturnDate.Value < DateTime.Now;

            string? currentHolderName = null;
            if (activeTransaction?.ToEmployeeId.HasValue == true)
            {
                var employee = await _employeeReps.GetByIdRawAsync(activeTransaction.ToEmployeeId.Value);
                currentHolderName = employee?.FullName;
            }

            var timeline = new List<AssetTimelineEntry>();
            foreach (var txn in history.OrderByDescending(t => t.TransactionDate))
            {
                var transactionTypeNameResult = await _masterDataLookupService.GetTransactionTypeNameAsync(txn.TransactionType);
                var transactionTypeName = transactionTypeNameResult.IsSuccess ? transactionTypeNameResult.Data : txn.TransactionType.ToString();

                timeline.Add(new AssetTimelineEntry
                {
                    Id = txn.AssetTransactionId,
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

    /// <inheritdoc />
    public async Task<ServiceResult<AssetCurrentStatusDto>> GetCurrentStatusAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetDetailByIdAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<AssetCurrentStatusDto>.NotFound($"Asset with id {assetId} not found");
            }

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(assetId);
            var currentStatus = DeriveAssetStatus(activeTransaction);

            string? currentHolderName = null;
            if (activeTransaction?.ToEmployeeId.HasValue == true)
            {
                var employee = await _employeeReps.GetByIdRawAsync(activeTransaction.ToEmployeeId.Value);
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
                IsOverdue = activeTransaction?.TransactionType == TransactionTypeConstants.LOAN
                    && activeTransaction.ExpectedReturnDate.HasValue
                    && activeTransaction.ExpectedReturnDate.Value < DateTime.Now,
                ConditionName = asset.AssetConditionName,
                ConditionCode = asset.AssetCondition
            };

            return ServiceResult<AssetCurrentStatusDto>.Success(dto);
        }, "get asset current status");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetTransactionDto>>> GetAssetTransactionHistoryAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<IEnumerable<AssetTransactionDto>>.NotFound($"Asset with id {assetId} not found");
            }

            var history = await _transactionReps.GetAssetTransactionHistoryAsync(assetId);
            var dtos = new List<AssetTransactionDto>();

            foreach (var txn in history)
            {
                dtos.Add(new AssetTransactionDto
                {
                    AssetTransactionId = txn.AssetTransactionId,
                    TransactionType = txn.TransactionType,
                    TransactionTypeName = txn.TransactionTypeName,
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

            return ServiceResult<IEnumerable<AssetTransactionDto>>.Success(dtos.OrderByDescending(d => d.TransactionDate));
        }, "get asset transaction history");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No asset IDs provided");
        }

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
                        TableNames.Asset,
                        id,
                        $"Asset '{asset.AssetCode}' {(request.Activate ? "activated" : "deactivated")} via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(activatedCount,
                $"{activatedCount} asset(s) {(request.Activate ? "activated" : "deactivated")} successfully");
        }, "bulk activate assets", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Asset, 0, "Bulk Activate Assets", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<AssetDropdownView>>.Success(assets);
        }, "get asset dropdown list");
    }

    #region Private Helpers

    private string DeriveAssetStatus(AssetTransactionListView? activeTransaction)
    {
        if (activeTransaction == null)
        {
            return "Available";
        }

        if (activeTransaction.ActualReturnDate.HasValue)
        {
            return "Available";
        }

        if (activeTransaction.Approved == false)
        {
            return "Rejected";
        }

        if (activeTransaction.Approved == null)
        {
            return "Pending Approval";
        }

        return activeTransaction.TransactionType switch
        {
            TransactionTypeConstants.HANDOVER or TransactionTypeConstants.TRANSFER => "Assigned",
            TransactionTypeConstants.LOAN => "On Loan",
            TransactionTypeConstants.MAINTENANCE => "In Maintenance",
            TransactionTypeConstants.LOAN_RETURN => "Returned",
            TransactionTypeConstants.DISPOSAL => "Disposed",
            _ => "Available"
        };
    }

    #endregion
}