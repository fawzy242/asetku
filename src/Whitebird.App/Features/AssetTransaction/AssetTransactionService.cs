using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.AssetTransaction;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.AssetTransaction;

public class AssetTransactionService : BaseService, IAssetTransactionService
{
    private readonly IGenericRepository<AssetTransactionEntity> _repository;
    private readonly IAssetTransactionReps _transactionReps;
    private readonly IAssetReps _assetReps;
    private readonly IEmployeeReps _employeeReps;
    private readonly IOfficeReps _officeReps;
    private readonly IMasterDataService _masterDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    // Transaction type constants
    private const int HANDOVER = 1;
    private const int TRANSFER = 2;
    private const int LOAN = 3;
    private const int RETURN = 4;
    private const int LOAN_RETURN = 5;
    private const int MAINTENANCE = 6;
    private const int POST_MAINTENANCE = 7;
    private const int DISPOSAL = 8;

    public AssetTransactionService(
        IGenericRepository<AssetTransactionEntity> repository,
        IAssetTransactionReps transactionReps,
        IAssetReps assetReps,
        IEmployeeReps employeeReps,
        IOfficeReps officeReps,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<AssetTransactionService> logger) : base(logger)
    {
        _repository = repository;
        _transactionReps = transactionReps;
        _assetReps = assetReps;
        _employeeReps = employeeReps;
        _officeReps = officeReps;
        _masterDataService = masterDataService;
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
            var viewModels = transactions.Adapt<List<AssetTransactionListViewModel>>();
            MarkOverdueStatus(viewModels);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(viewModels);
        }, "get all transactions");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByAssetIdAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetByAssetIdWithRelationsAsync(assetId);
            var viewModels = transactions.Adapt<List<AssetTransactionListViewModel>>();
            MarkOverdueStatus(viewModels);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(viewModels);
        }, "get transactions by asset");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByEmployeeIdAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetByEmployeeIdWithRelationsAsync(employeeId);
            var viewModels = transactions.Adapt<List<AssetTransactionListViewModel>>();
            MarkOverdueStatus(viewModels);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(viewModels);
        }, "get transactions by employee");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByApprovalStatusAsync(bool? approved)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetByApprovalStatusAsync(approved);
            var viewModels = transactions.Adapt<List<AssetTransactionListViewModel>>();
            MarkOverdueStatus(viewModels);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(viewModels);
        }, "get transactions by approval status");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetPendingApprovalsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetPendingApprovalsWithRelationsAsync();
            var viewModels = transactions.Adapt<List<AssetTransactionListViewModel>>();
            MarkOverdueStatus(viewModels);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(viewModels);
        }, "get pending approvals");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetActiveLoansAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetActiveLoansWithRelationsAsync();
            var viewModels = transactions.Adapt<List<AssetTransactionListViewModel>>();
            MarkOverdueStatus(viewModels);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(viewModels);
        }, "get active loans");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetOverdueLoansAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetOverdueLoansWithRelationsAsync();
            var viewModels = transactions.Adapt<List<AssetTransactionListViewModel>>();
            MarkOverdueStatus(viewModels);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(viewModels);
        }, "get overdue loans");
    }

    public async Task<ServiceResult<AssetTransactionDetailViewModel>> CreateAsync(AssetTransactionCreateViewModel model)
    {
        var asset = await _assetReps.GetByIdRawAsync(model.AssetId);
        if (asset == null)
            return ServiceResult<AssetTransactionDetailViewModel>.NotFound($"Asset with id {model.AssetId} not found");

        var validationResult = await ValidateTransactionRulesAsync(model, asset);
        if (validationResult != null)
            return validationResult;

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<AssetTransactionEntity>();
            entity.Approved = null;
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);

            if (entity.FromAssetTransactionId.HasValue &&
                (entity.TransactionType == LOAN_RETURN || entity.TransactionType == POST_MAINTENANCE))
            {
                var pairedTxn = await _transactionReps.GetByIdRawAsync(entity.FromAssetTransactionId.Value);
                if (pairedTxn != null)
                {
                    pairedTxn.FromAssetTransactionId = Convert.ToInt32(id);
                    pairedTxn.ModifiedDate = DateTime.Now;
                    pairedTxn.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(pairedTxn);
                }
            }

            var created = await _transactionReps.GetByIdWithRelationsAsync(Convert.ToInt32(id));

            if (created != null)
            {
                var typeName = await _masterDataService.GetValueAsync("TransactionType", created.TransactionType);
                await _activityLogService.LogCreateAsync(
                    "AssetTransaction",
                    created.AssetTransactionId,
                    $"Transaction created: Type '{typeName.Data}' for Asset '{created.AssetCode}'",
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

            if (existing.Approved != null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("Cannot update approved or rejected transaction");

            var oldType = existing.TransactionType;
            var oldApproved = existing.Approved;

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
                $"Transaction updated: Type '{oldType}' -> '{existing.TransactionType}', Approved '{oldApproved}' -> '{existing.Approved}'",
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

            if (existing.Approved != null)
                return ServiceResult.BadRequest("Transaction is already processed");

            var asset = await _assetReps.GetByIdRawAsync(existing.AssetId);
            if (asset == null)
                return ServiceResult.NotFound($"Asset with id {existing.AssetId} not found");

            if (model.IsApproved)
            {
                existing.Approved = true;
                existing.ApprovedBy = _currentUserService.GetDisplayName();
                existing.Notes = string.IsNullOrEmpty(existing.Notes)
                    ? model.ApprovalNotes
                    : $"{existing.Notes}\nApproval note: {model.ApprovalNotes}";

                await _activityLogService.LogUpdateAsync(
                    "AssetTransaction",
                    id,
                    $"Transaction approved: Type '{existing.TransactionType}' for Asset '{asset.AssetCode}'",
                    _currentUserService.GetDisplayName());
            }
            else
            {
                existing.Approved = false;
                existing.ApprovedBy = _currentUserService.GetDisplayName();
                existing.Notes = string.IsNullOrEmpty(existing.Notes)
                    ? $"Rejected: {model.ApprovalNotes}"
                    : $"{existing.Notes}\nRejected: {model.ApprovalNotes}";

                await _activityLogService.LogUpdateAsync(
                    "AssetTransaction",
                    id,
                    $"Transaction rejected: Type '{existing.TransactionType}' for Asset '{asset.AssetCode}'",
                    _currentUserService.GetDisplayName());
            }

            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            return result <= 0
                ? ServiceResult.Failure("Failed to process transaction")
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

            if (transaction.Approved != true)
                return ServiceResult.BadRequest("Cannot return asset from unapproved transaction");

            if (transaction.ActualReturnDate.HasValue)
                return ServiceResult.BadRequest("Asset already returned");

            if (transaction.FromAssetTransactionId != null)
                return ServiceResult.BadRequest("Transaction is already paired/closed");

            transaction.ActualReturnDate = model.ActualReturnDate;
            transaction.ConditionAfter = model.ConditionAfter;
            transaction.Notes = model.Notes ?? transaction.Notes;
            transaction.ModifiedDate = DateTime.Now;
            transaction.ModifiedBy = _currentUserService.GetDisplayName();

            if (await _repository.UpdateAsync(transaction) <= 0)
                return ServiceResult.Failure("Failed to return asset");

            var asset = await _assetReps.GetByIdRawAsync(transaction.AssetId);
            var assetCode = asset?.AssetCode ?? "Unknown";

            var conditionName = model.ConditionAfter.HasValue
                ? (await _masterDataService.GetValueAsync("AssetCondition", model.ConditionAfter.Value)).Data
                : "Unknown";

            var logDesc = $"Asset returned: Asset '{assetCode}' returned on {model.ActualReturnDate:yyyy-MM-dd}";
            if (model.ConditionAfter.HasValue)
                logDesc += $" (Condition: {conditionName})";

            await _activityLogService.LogUpdateAsync(
                "AssetTransaction",
                model.AssetTransactionId,
                logDesc,
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

            if (existing.Approved != null)
                return ServiceResult.BadRequest("Cannot cancel approved or rejected transaction");

            existing.IsActive = false;
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

    public async Task<ServiceResult<PaginatedResult<AssetTransactionListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetPagedWithRelationsAsync(page, pageSize, search, approved, assetId);
            var viewModels = result.Data.Adapt<List<AssetTransactionListViewModel>>();
            MarkOverdueStatus(viewModels);

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

    #region Private Helpers

    private async Task<ServiceResult<AssetTransactionDetailViewModel>?> ValidateTransactionRulesAsync(AssetTransactionCreateViewModel model, AssetEntity asset)
    {
        var type = model.TransactionType;

        var typeExists = await _masterDataService.GetValueAsync("TransactionType", type);
        if (!typeExists.IsSuccess || typeExists.Data == null)
            return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Invalid TransactionType: {type}");

        if (type == HANDOVER)
        {
            if (!model.ToEmployeeId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("HANDOVER requires ToEmployeeId");

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction != null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is currently in an active transaction");
        }

        if (type == TRANSFER)
        {
            if (!model.FromEmployeeId.HasValue || !model.ToEmployeeId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("TRANSFER requires both FromEmployeeId and ToEmployeeId");

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction == null || activeTransaction.ToEmployeeId != model.FromEmployeeId)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not currently held by employee {model.FromEmployeeId}");
        }

        if (type == LOAN)
        {
            if (!model.ToEmployeeId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("LOAN requires ToEmployeeId");

            if (!model.ExpectedReturnDate.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("LOAN requires ExpectedReturnDate");

            if (model.ExpectedReturnDate.Value <= model.TransactionDate)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("ExpectedReturnDate must be after TransactionDate");

            var hasOpenLoan = await _transactionReps.HasOpenPairedTransactionAsync(asset.AssetId, LOAN);
            if (hasOpenLoan)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' already has an active loan");
        }

        if (type == RETURN)
        {
            if (!model.FromEmployeeId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("RETURN requires FromEmployeeId");

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction == null || activeTransaction.ToEmployeeId != model.FromEmployeeId)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not currently held by employee {model.FromEmployeeId}");

            if (activeTransaction.TransactionType != HANDOVER && activeTransaction.TransactionType != TRANSFER)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not in assigned state");
        }

        if (type == LOAN_RETURN)
        {
            if (!model.FromAssetTransactionId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("LOAN_RETURN requires FromAssetTransactionId");

            var pairedTransaction = await _transactionReps.GetByIdRawAsync(model.FromAssetTransactionId.Value);
            if (pairedTransaction == null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Paired transaction with id {model.FromAssetTransactionId} not found");

            if (pairedTransaction.AssetId != model.AssetId)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("Paired transaction must be for the same asset");

            if (pairedTransaction.TransactionType != LOAN)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Cannot pair LOAN_RETURN with transaction type {pairedTransaction.TransactionType}");

            if (pairedTransaction.FromAssetTransactionId != null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("Loan transaction is already closed");
        }

        if (type == MAINTENANCE)
        {
            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction != null && activeTransaction.TransactionType == MAINTENANCE)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is already in maintenance");
        }

        if (type == POST_MAINTENANCE)
        {
            if (!model.FromAssetTransactionId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("POST_MAINTENANCE requires FromAssetTransactionId");

            var pairedTransaction = await _transactionReps.GetByIdRawAsync(model.FromAssetTransactionId.Value);
            if (pairedTransaction == null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Paired transaction with id {model.FromAssetTransactionId} not found");

            if (pairedTransaction.AssetId != model.AssetId)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("Paired transaction must be for the same asset");

            if (pairedTransaction.TransactionType != MAINTENANCE)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Cannot pair POST_MAINTENANCE with transaction type {pairedTransaction.TransactionType}");

            if (pairedTransaction.FromAssetTransactionId != null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("Maintenance transaction is already closed");

            if (!model.ConditionAfter.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("POST_MAINTENANCE requires ConditionAfter");
        }

        if (type == DISPOSAL)
        {
            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction != null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' has active transaction, cannot dispose");
        }

        if (model.ToEmployeeId.HasValue)
        {
            var employee = await _employeeReps.GetByIdAsync(model.ToEmployeeId.Value);
            if (employee == null)
                return ServiceResult<AssetTransactionDetailViewModel>.NotFound($"Employee with id {model.ToEmployeeId} not found");
        }

        if (model.FromEmployeeId.HasValue)
        {
            var employee = await _employeeReps.GetByIdAsync(model.FromEmployeeId.Value);
            if (employee == null)
                return ServiceResult<AssetTransactionDetailViewModel>.NotFound($"Employee with id {model.FromEmployeeId} not found");
        }

        if (model.ToLocationId.HasValue)
        {
            var office = await _officeReps.GetByIdAsync(model.ToLocationId.Value);
            if (office == null)
                return ServiceResult<AssetTransactionDetailViewModel>.NotFound($"Office with id {model.ToLocationId} not found");
        }

        if (model.ConditionBefore.HasValue)
        {
            var conditionExists = await _masterDataService.GetValueAsync("AssetCondition", model.ConditionBefore.Value);
            if (!conditionExists.IsSuccess || conditionExists.Data == null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Invalid ConditionBefore value: {model.ConditionBefore}");
        }

        if (model.ConditionAfter.HasValue)
        {
            var conditionExists = await _masterDataService.GetValueAsync("AssetCondition", model.ConditionAfter.Value);
            if (!conditionExists.IsSuccess || conditionExists.Data == null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Invalid ConditionAfter value: {model.ConditionAfter}");
        }

        if (model.MaintenanceType.HasValue)
        {
            var maintenanceExists = await _masterDataService.GetValueAsync("MaintenanceType", model.MaintenanceType.Value);
            if (!maintenanceExists.IsSuccess || maintenanceExists.Data == null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Invalid MaintenanceType value: {model.MaintenanceType}");
        }

        return null;
    }

    private void MarkOverdueStatus(List<AssetTransactionListViewModel> transactions)
    {
        var now = DateTime.Now;
        foreach (var t in transactions)
        {
            t.IsOverdue = t.TransactionType == LOAN
                && t.Approved == true
                && t.FromAssetTransactionId == null
                && t.ExpectedReturnDate.HasValue
                && t.ExpectedReturnDate.Value < now;
        }
    }

    #endregion
}