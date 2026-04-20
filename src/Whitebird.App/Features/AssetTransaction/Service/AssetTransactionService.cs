using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.AssetTransaction.Interfaces;
using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.AssetTransaction.Entities;
using Whitebird.Domain.Features.AssetTransaction.View;
using Whitebird.Domain.Features.Asset.Entities;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.Employee;

namespace Whitebird.App.Features.AssetTransaction.Service;

public class AssetTransactionService : BaseService, IAssetTransactionService
{
    private readonly IGenericRepository<AssetTransactionEntity> _repository;
    private readonly IGenericRepository<AssetEntity> _assetRepository;
    private readonly IAssetTransactionReps _transactionReps;
    private readonly IAssetReps _assetReps;
    private readonly IEmployeeReps _employeeReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public AssetTransactionService(
        IGenericRepository<AssetTransactionEntity> repository,
        IGenericRepository<AssetEntity> assetRepository,
        IAssetTransactionReps transactionReps,
        IAssetReps assetReps,
        IEmployeeReps employeeReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<AssetTransactionService> logger) : base(logger)
    {
        _repository = repository;
        _assetRepository = assetRepository;
        _transactionReps = transactionReps;
        _assetReps = assetReps;
        _employeeReps = employeeReps;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<AssetTransactionDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdWithRelationsAsync(id);
            if (transaction == null)
                return ServiceResult<AssetTransactionDetailViewModel>.NotFound($"Transaction with id {id} not found");

            return ServiceResult<AssetTransactionDetailViewModel>.Success(transaction.Adapt<AssetTransactionDetailViewModel>());
        }, "get transaction by id");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetAllWithRelationsAsync();
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(transactions.Adapt<IEnumerable<AssetTransactionListViewModel>>());
        }, "get all transactions");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByAssetIdAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetByAssetIdWithRelationsAsync(assetId);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(transactions.Adapt<IEnumerable<AssetTransactionListViewModel>>());
        }, "get transactions by asset");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByEmployeeIdAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetByEmployeeIdWithRelationsAsync(employeeId);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(transactions.Adapt<IEnumerable<AssetTransactionListViewModel>>());
        }, "get transactions by employee");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByStatusAsync(string status)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetByStatusWithRelationsAsync(status);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(transactions.Adapt<IEnumerable<AssetTransactionListViewModel>>());
        }, "get transactions by status");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetPendingApprovalsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetPendingApprovalsWithRelationsAsync();
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(transactions.Adapt<IEnumerable<AssetTransactionListViewModel>>());
        }, "get pending approvals");
    }

    public async Task<ServiceResult<AssetTransactionDetailViewModel>> CreateAsync(AssetTransactionCreateViewModel model)
    {
        var asset = await _assetReps.GetByIdRawAsync(model.AssetId);
        if (asset == null)
            return ServiceResult<AssetTransactionDetailViewModel>.NotFound($"Asset with id {model.AssetId} not found");
        if (model.TransactionType == "Assignment" && asset.Status != "Available")
            return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not available for assignment");
        if (model.ToEmployeeId.HasValue && await _employeeReps.GetByIdAsync(model.ToEmployeeId.Value) == null)
            return ServiceResult<AssetTransactionDetailViewModel>.NotFound($"Employee with id {model.ToEmployeeId} not found");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<AssetTransactionEntity>();
            entity.TransactionDate = DateTime.Now;
            entity.TransactionStatus = "Pending";
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _transactionReps.GetByIdWithRelationsAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    "AssetTransaction",
                    created.AssetTransactionId,
                    $"Transaction created: Type '{created.TransactionType}' for Asset '{created.AssetCode}'",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<AssetTransactionDetailViewModel>.Failure("Failed to retrieve created transaction")
                : ServiceResult<AssetTransactionDetailViewModel>.Success(created.Adapt<AssetTransactionDetailViewModel>(), "Transaction created successfully");
        }, "create transaction", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("AssetTransaction", 0, "Create Transaction", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<AssetTransactionDetailViewModel>> UpdateAsync(int id, AssetTransactionUpdateViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult<AssetTransactionDetailViewModel>.NotFound($"Transaction with id {id} not found");
            if (existing.TransactionStatus != "Pending")
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("Cannot update approved or completed transaction");

            var oldType = existing.TransactionType;
            var oldStatus = existing.TransactionStatus;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
                return ServiceResult<AssetTransactionDetailViewModel>.Failure("Failed to update transaction");

            var updated = await _transactionReps.GetByIdWithRelationsAsync(id);

            await _activityLogService.LogUpdateAsync(
                "AssetTransaction",
                id,
                $"Transaction updated: Type '{oldType}' -> '{existing.TransactionType}', Status '{oldStatus}' -> '{existing.TransactionStatus}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailViewModel>.Success(updated!.Adapt<AssetTransactionDetailViewModel>(), "Transaction updated successfully");
        }, "update transaction", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("AssetTransaction", id, "Update Transaction", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> ApproveAsync(int id, AssetTransactionApproveViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Transaction with id {id} not found");
            if (existing.TransactionStatus != "Pending")
                return ServiceResult.BadRequest("Transaction is not pending approval");

            var asset = await _assetReps.GetByIdRawAsync(existing.AssetId);
            var assetCode = asset?.AssetCode ?? "Unknown";

            if (model.IsApproved)
            {
                existing.TransactionStatus = "Approved";
                existing.ApprovedBy = _currentUserService.UserId;

                if (existing.TransactionType == "Assignment" && existing.ToEmployeeId.HasValue)
                {
                    if (asset != null)
                    {
                        asset.Status = "Assigned";
                        asset.CurrentHolderId = existing.ToEmployeeId;
                        asset.ModifiedDate = DateTime.Now;
                        asset.ModifiedBy = _currentUserService.GetDisplayName();
                        await _assetRepository.UpdateAsync(asset);
                    }
                }

                await _activityLogService.LogUpdateAsync(
                    "AssetTransaction",
                    id,
                    $"Transaction approved: Type '{existing.TransactionType}' for Asset '{assetCode}'",
                    _currentUserService.GetDisplayName());
            }
            else
            {
                existing.TransactionStatus = "Rejected";

                await _activityLogService.LogUpdateAsync(
                    "AssetTransaction",
                    id,
                    $"Transaction rejected: Type '{existing.TransactionType}' for Asset '{assetCode}'",
                    _currentUserService.GetDisplayName());
            }

            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            return result <= 0
                ? ServiceResult.Failure("Failed to approve transaction")
                : ServiceResult.Success(model.IsApproved ? "Transaction approved successfully" : "Transaction rejected successfully");
        }, "approve transaction", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("AssetTransaction", id, "Approve Transaction", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> ReturnAssetAsync(AssetReturnViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(model.AssetTransactionId);
            if (transaction == null)
                return ServiceResult.NotFound($"Transaction with id {model.AssetTransactionId} not found");
            if (transaction.TransactionStatus != "Approved")
                return ServiceResult.BadRequest("Cannot return asset from unapproved transaction");
            if (transaction.ActualReturnDate.HasValue)
                return ServiceResult.BadRequest("Asset already returned");

            transaction.ActualReturnDate = model.ActualReturnDate;
            transaction.ConditionAfter = model.ConditionAfter;
            transaction.Notes = model.Notes;
            transaction.ModifiedDate = DateTime.Now;
            transaction.ModifiedBy = _currentUserService.GetDisplayName();

            if (await _repository.UpdateAsync(transaction) <= 0)
                return ServiceResult.Failure("Failed to return asset");

            var asset = await _assetReps.GetByIdRawAsync(transaction.AssetId);
            if (asset != null)
            {
                asset.Status = "Available";
                asset.CurrentHolderId = null;
                asset.ModifiedDate = DateTime.Now;
                asset.ModifiedBy = _currentUserService.GetDisplayName();
                await _assetRepository.UpdateAsync(asset);
            }

            await _activityLogService.LogUpdateAsync(
                "AssetTransaction",
                model.AssetTransactionId,
                $"Asset returned: Asset '{asset?.AssetCode}' returned on {model.ActualReturnDate:yyyy-MM-dd}",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Asset returned successfully");
        }, "return asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("AssetTransaction", model.AssetTransactionId, "Return Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> CancelAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Transaction with id {id} not found");
            if (existing.TransactionStatus != "Pending")
                return ServiceResult.BadRequest("Cannot cancel approved or completed transaction");

            existing.TransactionStatus = "Cancelled";
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogUpdateAsync(
                    "AssetTransaction",
                    id,
                    $"Transaction cancelled: Type '{existing.TransactionType}'",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to cancel transaction")
                : ServiceResult.Success("Transaction cancelled successfully");
        }, "cancel transaction", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("AssetTransaction", id, "Cancel Transaction", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<PaginatedResult<AssetTransactionListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, string? status = null, int? assetId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetPagedWithRelationsAsync(page, pageSize, search, status, assetId);
            var viewModels = result.Data.Adapt<List<AssetTransactionListViewModel>>();

            return ServiceResult<PaginatedResult<AssetTransactionListViewModel>>.Success(new PaginatedResult<AssetTransactionListViewModel>
            {
                Data = viewModels,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            });
        }, "get transaction grid data");
    }
}