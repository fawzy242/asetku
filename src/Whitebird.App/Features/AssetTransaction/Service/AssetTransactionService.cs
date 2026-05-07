using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.AssetTransaction.Interfaces;
using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.AssetTransaction.Entities;
using Whitebird.Domain.Features.AssetTransaction.View;
using Whitebird.Domain.Features.Asset.Entities;
using Whitebird.Domain.Features.AssetTransaction.Enums;
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

    public async Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByStatusAsync(string status)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetByStatusWithRelationsAsync(status);
            var viewModels = transactions.Adapt<List<AssetTransactionListViewModel>>();
            MarkOverdueStatus(viewModels);
            return ServiceResult<IEnumerable<AssetTransactionListViewModel>>.Success(viewModels);
        }, "get transactions by status");
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
        // Validate transaction type
        if (!TransactionTypeConstants.All.Contains(model.TransactionType))
            return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Invalid transaction type: {model.TransactionType}");

        // Validate asset exists and is active
        var asset = await _assetReps.GetByIdRawAsync(model.AssetId);
        if (asset == null)
            return ServiceResult<AssetTransactionDetailViewModel>.NotFound($"Asset with id {model.AssetId} not found");

        // Validate pairing if required
        if (TransactionTypeConstants.RequiresPairing.Contains(model.TransactionType))
        {
            if (!model.PairedTransactionId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Transaction type '{model.TransactionType}' requires a PairedTransactionId");

            var pairedTransaction = await _transactionReps.GetByIdRawAsync(model.PairedTransactionId.Value);
            if (pairedTransaction == null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Paired transaction with id {model.PairedTransactionId} not found");
            if (pairedTransaction.AssetId != model.AssetId)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("Paired transaction must be for the same asset");
            if (pairedTransaction.PairedTransactionId != null)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("Paired transaction is already closed");

            // Validate pairing types
            bool isValidPair = (model.TransactionType == TransactionTypeConstants.LoanReturn && pairedTransaction.TransactionType == TransactionTypeConstants.Loan) ||
                               (model.TransactionType == TransactionTypeConstants.PostMaintenance && pairedTransaction.TransactionType == TransactionTypeConstants.Maintenance);
            if (!isValidPair)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Invalid pairing: {model.TransactionType} cannot pair with {pairedTransaction.TransactionType}");
        }

        // Validate business rules per transaction type
        var validationResult = await ValidateTransactionRulesAsync(model, asset);
        if (validationResult != null)
            return validationResult;

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<AssetTransactionEntity>();
            entity.TransactionStatus = "Pending";
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);

            // If this is a pairing transaction, update the paired transaction's PairedTransactionId
            if (entity.PairedTransactionId.HasValue)
            {
                var pairedTxn = await _transactionReps.GetByIdRawAsync(entity.PairedTransactionId.Value);
                if (pairedTxn != null)
                {
                    pairedTxn.PairedTransactionId = Convert.ToInt32(id);
                    pairedTxn.ModifiedDate = DateTime.Now;
                    pairedTxn.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(pairedTxn);
                }
            }

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
            if (asset == null)
                return ServiceResult.NotFound($"Asset with id {existing.AssetId} not found");

            var assetCode = asset.AssetCode;

            if (model.IsApproved)
            {
                existing.TransactionStatus = "Approved";
                existing.ApprovedBy = _currentUserService.UserId;

                // Update asset status & holder based on transaction type
                string newStatus = DeriveAssetStatus(existing.TransactionType);
                asset.Status = newStatus;

                if (TransactionTypeConstants.ChangesHolder.Contains(existing.TransactionType))
                {
                    if (existing.TransactionType == TransactionTypeConstants.Return ||
                        existing.TransactionType == TransactionTypeConstants.LoanReturn)
                    {
                        asset.CurrentHolderId = null;
                    }
                    else
                    {
                        asset.CurrentHolderId = existing.ToEmployeeId;
                    }
                }

                asset.ModifiedDate = DateTime.Now;
                asset.ModifiedBy = _currentUserService.GetDisplayName();
                await _assetRepository.UpdateAsync(asset);

                await _activityLogService.LogUpdateAsync(
                    "AssetTransaction",
                    id,
                    $"Transaction approved: Type '{existing.TransactionType}' for Asset '{assetCode}'. Asset status updated to '{newStatus}'",
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
            if (transaction.PairedTransactionId != null)
                return ServiceResult.BadRequest("Transaction is already paired/closed");

            transaction.ActualReturnDate = model.ActualReturnDate;
            transaction.ConditionAfter = model.ConditionAfter;
            transaction.DamageReason = model.DamageReason;
            transaction.Notes = model.Notes ?? transaction.Notes;
            transaction.ModifiedDate = DateTime.Now;
            transaction.ModifiedBy = _currentUserService.GetDisplayName();

            if (await _repository.UpdateAsync(transaction) <= 0)
                return ServiceResult.Failure("Failed to return asset");

            var asset = await _assetReps.GetByIdRawAsync(transaction.AssetId);
            if (asset != null)
            {
                asset.Status = !string.IsNullOrEmpty(model.DamageReason) ? "Damaged" : "Available";
                asset.CurrentHolderId = null;
                asset.Condition = model.ConditionAfter ?? asset.Condition;
                asset.ModifiedDate = DateTime.Now;
                asset.ModifiedBy = _currentUserService.GetDisplayName();
                await _assetRepository.UpdateAsync(asset);
            }

            var logDesc = $"Asset returned: Asset '{asset?.AssetCode}' returned on {model.ActualReturnDate:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(model.DamageReason))
                logDesc += $" (Damaged: {model.DamageReason})";

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

        // HANDOVER: Company → Employee (ToEmployeeId required)
        if (type == TransactionTypeConstants.Handover)
        {
            if (!model.ToEmployeeId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("HANDOVER requires ToEmployeeId");
            if (asset.Status != "Available")
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not available (current status: {asset.Status})");
        }

        // TRANSFER: Employee A → Employee B (both required)
        if (type == TransactionTypeConstants.Transfer)
        {
            if (!model.FromEmployeeId.HasValue || !model.ToEmployeeId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("TRANSFER requires both FromEmployeeId and ToEmployeeId");
            if (asset.Status != "Assigned" && asset.Status != "On Loan")
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not assigned to any employee (current status: {asset.Status})");
            if (asset.CurrentHolderId != model.FromEmployeeId)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not held by employee {model.FromEmployeeId}");
        }

        // LOAN: Company → Employee (ToEmployeeId required)
        if (type == TransactionTypeConstants.Loan)
        {
            if (!model.ToEmployeeId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("LOAN requires ToEmployeeId");
            if (asset.Status != "Available")
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not available for loan (current status: {asset.Status})");
        }

        // RETURN: Employee → Company (FromEmployeeId required)
        if (type == TransactionTypeConstants.Return)
        {
            if (!model.FromEmployeeId.HasValue)
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest("RETURN requires FromEmployeeId");
            if (asset.Status != "Assigned" && asset.Status != "On Loan")
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not assigned (current status: {asset.Status})");
        }

        // LOAN_RETURN: Return from loan (PairedTransactionId required, validated above)
        if (type == TransactionTypeConstants.LoanReturn)
        {
            if (asset.Status != "On Loan")
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not on loan (current status: {asset.Status})");
        }

        // MAINTENANCE: Asset to maintenance
        if (type == TransactionTypeConstants.Maintenance)
        {
            if (asset.Status == "In Maintenance" || asset.Status == "Under Repair")
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is already in maintenance");
        }

        // POST_MAINTENANCE: Return from maintenance (PairedTransactionId required, validated above)
        if (type == TransactionTypeConstants.PostMaintenance)
        {
            if (asset.Status != "In Maintenance")
                return ServiceResult<AssetTransactionDetailViewModel>.BadRequest($"Asset '{asset.AssetCode}' is not in maintenance (current status: {asset.Status})");
        }

        // Validate employee exists if provided
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

        return null;
    }

    private string DeriveAssetStatus(string transactionType)
    {
        return transactionType switch
        {
            TransactionTypeConstants.Handover => "Assigned",
            TransactionTypeConstants.Transfer => "Assigned",
            TransactionTypeConstants.Loan => "On Loan",
            TransactionTypeConstants.Return => "Available",
            TransactionTypeConstants.LoanReturn => "Available",
            TransactionTypeConstants.Maintenance => "In Maintenance",
            TransactionTypeConstants.PostMaintenance => "Available",
            TransactionTypeConstants.Disposal => "Disposed",
            _ => "Available"
        };
    }

    private void MarkOverdueStatus(List<AssetTransactionListViewModel> transactions)
    {
        var now = DateTime.Now;
        foreach (var t in transactions)
        {
            t.IsOverdue = t.TransactionType == TransactionTypeConstants.Loan
                && t.TransactionStatus == "Approved"
                && t.PairedTransactionId == null
                && t.ExpectedReturnDate.HasValue
                && t.ExpectedReturnDate.Value < now;
        }
    }

    #endregion
}