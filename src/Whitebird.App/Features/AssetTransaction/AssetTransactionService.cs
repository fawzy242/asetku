using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.AssetTransaction;

/// <summary>
/// Service implementation for Asset Transaction business logic
/// </summary>
public class AssetTransactionService : BaseService, IAssetTransactionService
{
    private readonly IGenericRepository<AssetTransactionEntity> _repository;
    private readonly IAssetTransactionReps _transactionReps;
    private readonly IAssetReps _assetReps;
    private readonly IEmployeeReps _employeeReps;
    private readonly IOfficeReps _officeReps;
    private readonly IMasterDataService _masterDataService;
    private readonly IMasterDataLookupService _masterDataLookupService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public AssetTransactionService(
        IGenericRepository<AssetTransactionEntity> repository,
        IAssetTransactionReps transactionReps,
        IAssetReps assetReps,
        IEmployeeReps employeeReps,
        IOfficeReps officeReps,
        IMasterDataService masterDataService,
        IMasterDataLookupService masterDataLookupService,
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
        _masterDataLookupService = masterDataLookupService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AssetTransactionDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transaction = await _transactionReps.GetDetailByIdAsync(id);
            if (transaction == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {id} not found");
            }
            return ServiceResult<AssetTransactionDetailView>.Success(transaction);
        }, "get transaction by id");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetAllListViewAsync();
            MarkOverdueStatus(transactions);
            return ServiceResult<IEnumerable<AssetTransactionListView>>.Success(transactions);
        }, "get all transactions");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetByAssetIdAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<IEnumerable<AssetTransactionListView>>.NotFound($"Asset with id {assetId} not found");
            }

            var transactions = await _transactionReps.GetByAssetIdListViewAsync(assetId);
            MarkOverdueStatus(transactions);
            return ServiceResult<IEnumerable<AssetTransactionListView>>.Success(transactions);
        }, "get transactions by asset");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetByEmployeeIdAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetByIdRawAsync(employeeId);
            if (employee == null)
            {
                return ServiceResult<IEnumerable<AssetTransactionListView>>.NotFound($"Employee with id {employeeId} not found");
            }

            var transactions = await _transactionReps.GetByEmployeeIdListViewAsync(employeeId);
            MarkOverdueStatus(transactions);
            return ServiceResult<IEnumerable<AssetTransactionListView>>.Success(transactions);
        }, "get transactions by employee");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetByApprovalStatusAsync(bool? approved)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetByApprovalStatusListViewAsync(approved);
            MarkOverdueStatus(transactions);
            return ServiceResult<IEnumerable<AssetTransactionListView>>.Success(transactions);
        }, "get transactions by approval status");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetPendingApprovalsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetPendingApprovalsListViewAsync();
            MarkOverdueStatus(transactions);
            return ServiceResult<IEnumerable<AssetTransactionListView>>.Success(transactions);
        }, "get pending approvals");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetActiveLoansAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetActiveLoansListViewAsync();
            MarkOverdueStatus(transactions);
            return ServiceResult<IEnumerable<AssetTransactionListView>>.Success(transactions);
        }, "get active loans");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetOverdueLoansAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transactions = await _transactionReps.GetOverdueLoansListViewAsync();
            MarkOverdueStatus(transactions);
            return ServiceResult<IEnumerable<AssetTransactionListView>>.Success(transactions);
        }, "get overdue loans");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AssetTransactionDetailView>> CreateAsync(AssetTransactionCreateViewModel model)
    {
        var asset = await _assetReps.GetByIdRawAsync(model.AssetId);
        if (asset == null)
        {
            return ServiceResult<AssetTransactionDetailView>.NotFound($"Asset with id {model.AssetId} not found");
        }

        var validationResult = await ValidateTransactionRulesAsync(model, asset);
        if (validationResult != null)
        {
            return validationResult;
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var freshAsset = await _assetReps.GetByIdRawAsync(model.AssetId);
            var reValidationResult = await ValidateTransactionRulesAsync(model, freshAsset!);
            if (reValidationResult != null)
            {
                return reValidationResult;
            }

            var entity = model.Adapt<AssetTransactionEntity>();
            entity.Approved = null;
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);

            if (entity.FromAssetTransactionId.HasValue &&
                (entity.TransactionType == TransactionTypeConstants.LOAN_RETURN || 
                 entity.TransactionType == TransactionTypeConstants.POST_MAINTENANCE))
            {
                var pairedTxn = await _transactionReps.GetByIdRawAsync(entity.FromAssetTransactionId.Value);
                if (pairedTxn != null && !pairedTxn.FromAssetTransactionId.HasValue)
                {
                    pairedTxn.FromAssetTransactionId = Convert.ToInt32(id);
                    pairedTxn.ModifiedDate = DateTime.Now;
                    pairedTxn.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(pairedTxn);
                }
                else
                {
                    return ServiceResult<AssetTransactionDetailView>.Conflict(
                        $"Transaction {entity.FromAssetTransactionId} is already paired or invalid");
                }
            }

            var created = await _transactionReps.GetDetailByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.AssetTransaction,
                    created.AssetTransactionId,
                    $"Transaction created: Type '{created.TransactionTypeName}' for Asset ID {created.AssetId}",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<AssetTransactionDetailView>.Failure("Failed to retrieve created transaction")
                : ServiceResult<AssetTransactionDetailView>.Success(created, "Transaction created successfully");
        }, "create transaction", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.AssetTransaction, 0, "Create Transaction", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AssetTransactionDetailView>> UpdateAsync(int id, AssetTransactionUpdateViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {id} not found");
            }

            if (existing.Approved != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Cannot update approved or rejected transaction");
            }

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<AssetTransactionDetailView>.Failure("Failed to update transaction");
            }

            var updated = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                id,
                $"Transaction updated: Type '{existing.TransactionType}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailView>.Success(updated!, "Transaction updated successfully");
        }, "update transaction", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.AssetTransaction, id, "Update Transaction", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ApproveAsync(int id, AssetTransactionApproveViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Transaction with id {id} not found");
            }

            if (existing.Approved != null)
            {
                return ServiceResult.BadRequest("Transaction is already processed");
            }

            var asset = await _assetReps.GetByIdRawAsync(existing.AssetId);
            if (asset == null)
            {
                return ServiceResult.NotFound($"Asset with id {existing.AssetId} not found");
            }

            var typeNameResult = await _masterDataLookupService.GetTransactionTypeNameAsync(existing.TransactionType);
            var typeName = typeNameResult.IsSuccess ? typeNameResult.Data : existing.TransactionType.ToString();

            if (model.IsApproved)
            {
                existing.Approved = true;
                existing.ApprovedBy = _currentUserService.GetDisplayName();
                existing.Notes = string.IsNullOrEmpty(existing.Notes)
                    ? model.ApprovalNotes
                    : $"{existing.Notes}\nApproval note: {model.ApprovalNotes}";

                await _activityLogService.LogUpdateAsync(
                    TableNames.AssetTransaction,
                    id,
                    $"Transaction approved: Type '{typeName}' for Asset '{asset.AssetCode}'",
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
                    TableNames.AssetTransaction,
                    id,
                    $"Transaction rejected: Type '{typeName}' for Asset '{asset.AssetCode}'",
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
            await _activityLogService.LogErrorAsync(TableNames.AssetTransaction, id, "Approve Transaction", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ReturnAssetAsync(AssetReturnViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(model.AssetTransactionId);
            if (transaction == null)
            {
                return ServiceResult.NotFound($"Transaction with id {model.AssetTransactionId} not found");
            }

            if (transaction.Approved != true)
            {
                return ServiceResult.BadRequest("Cannot return asset from unapproved transaction");
            }

            if (transaction.ActualReturnDate.HasValue)
            {
                return ServiceResult.BadRequest("Asset already returned");
            }

            if (transaction.FromAssetTransactionId != null)
            {
                return ServiceResult.BadRequest("Transaction is already paired/closed");
            }

            transaction.ActualReturnDate = model.ActualReturnDate;
            transaction.ConditionAfter = model.ConditionAfter;
            transaction.Notes = model.Notes ?? transaction.Notes;
            transaction.ModifiedDate = DateTime.Now;
            transaction.ModifiedBy = _currentUserService.GetDisplayName();

            if (await _repository.UpdateAsync(transaction) <= 0)
            {
                return ServiceResult.Failure("Failed to return asset");
            }

            var asset = await _assetReps.GetByIdRawAsync(transaction.AssetId);
            var assetCode = asset?.AssetCode ?? "Unknown";

            var conditionName = model.ConditionAfter.HasValue
                ? (await _masterDataLookupService.GetAssetConditionNameAsync(model.ConditionAfter.Value)).Data
                : "Unknown";

            var logDesc = $"Asset returned: Asset '{assetCode}' returned on {model.ActualReturnDate:yyyy-MM-dd}";
            if (model.ConditionAfter.HasValue)
            {
                logDesc += $" (Condition: {conditionName})";
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                model.AssetTransactionId,
                logDesc,
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Asset returned successfully");
        }, "return asset", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.AssetTransaction, model.AssetTransactionId, "Return Asset", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CancelAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Transaction with id {id} not found");
            }

            if (existing.Approved != null)
            {
                return ServiceResult.BadRequest("Cannot cancel approved or rejected transaction");
            }

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogUpdateAsync(
                    TableNames.AssetTransaction,
                    id,
                    $"Transaction cancelled: Type '{existing.TransactionType}'",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to cancel transaction")
                : ServiceResult.Success("Transaction cancelled successfully");
        }, "cancel transaction", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.AssetTransaction, id, "Cancel Transaction", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetPagedListAsync(page, pageSize, search, approved, assetId);
            var viewModels = result.Data.ToList();
            MarkOverdueStatus(viewModels);

            return ServiceResult<PaginatedResult<AssetTransactionListView>>.Success(new PaginatedResult<AssetTransactionListView>
            {
                Data = viewModels,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            });
        }, "get transaction grid data");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<AssetTransactionDropdownView>>> GetAvailablePairedTransactionsAsync(int assetId, int transactionType)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<IEnumerable<AssetTransactionDropdownView>>.NotFound($"Asset with id {assetId} not found");
            }

            var transactions = await _transactionReps.GetAvailablePairedTransactionsAsync(assetId, transactionType);
            return ServiceResult<IEnumerable<AssetTransactionDropdownView>>.Success(transactions);
        }, "get available paired transactions");
    }

    // ============================================================
    // NEW: SHORTCUT METHODS
    // ============================================================

    /// <inheritdoc />
    public async Task<ServiceResult<AssetTransactionDetailView>> CreateReturnTransactionAsync(int sourceTransactionId, AssetReturnViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            // Get source transaction
            var sourceTransaction = await _transactionReps.GetByIdRawAsync(sourceTransactionId);
            if (sourceTransaction == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Source transaction with id {sourceTransactionId} not found");
            }

            // Validate source transaction type (only HANDOVER, LOAN, MAINTENANCE can be returned)
            if (sourceTransaction.TransactionType != TransactionTypeConstants.HANDOVER &&
                sourceTransaction.TransactionType != TransactionTypeConstants.LOAN &&
                sourceTransaction.TransactionType != TransactionTypeConstants.MAINTENANCE)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest(
                    $"Transaction type '{sourceTransaction.TransactionType}' cannot be returned. Only HANDOVER, LOAN, and MAINTENANCE can be returned.");
            }

            // Check if already returned
            if (sourceTransaction.ActualReturnDate.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Asset already returned");
            }

            // Check if transaction is approved
            if (sourceTransaction.Approved != true)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Cannot return unapproved transaction");
            }

            var asset = await _assetReps.GetByIdRawAsync(sourceTransaction.AssetId);
            if (asset == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Asset with id {sourceTransaction.AssetId} not found");
            }

            // Create RETURN transaction
            var returnTransaction = new AssetTransactionEntity
            {
                AssetId = sourceTransaction.AssetId,
                TransactionType = TransactionTypeConstants.RETURN,
                FromEmployeeId = sourceTransaction.ToEmployeeId,
                ToEmployeeId = null,
                TransactionDate = model.ActualReturnDate,
                Notes = model.Notes,
                ConditionBefore = sourceTransaction.ConditionAfter,
                ConditionAfter = model.ConditionAfter,
                Approved = null, // Pending approval
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName(),
                FromAssetTransactionId = sourceTransactionId
            };

            var id = await _repository.InsertAsync(returnTransaction);

            // Update source transaction with return date
            sourceTransaction.ActualReturnDate = model.ActualReturnDate;
            sourceTransaction.ModifiedDate = DateTime.Now;
            sourceTransaction.ModifiedBy = _currentUserService.GetDisplayName();
            await _repository.UpdateAsync(sourceTransaction);

            var created = await _transactionReps.GetDetailByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.AssetTransaction,
                    created.AssetTransactionId,
                    $"RETURN transaction created as shortcut from transaction ID {sourceTransactionId} for Asset ID {created.AssetId}",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<AssetTransactionDetailView>.Failure("Failed to retrieve created return transaction")
                : ServiceResult<AssetTransactionDetailView>.Success(created, "Return transaction created successfully (Pending approval)");
        }, "create return transaction", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.AssetTransaction, 0, "Create Return Transaction", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AssetTransactionDetailView>> CreatePostMaintenanceTransactionAsync(int sourceTransactionId, PostMaintenanceViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            // Get source transaction
            var sourceTransaction = await _transactionReps.GetByIdRawAsync(sourceTransactionId);
            if (sourceTransaction == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Source transaction with id {sourceTransactionId} not found");
            }

            // Validate source transaction type (only MAINTENANCE)
            if (sourceTransaction.TransactionType != TransactionTypeConstants.MAINTENANCE)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest(
                    $"Transaction type '{sourceTransaction.TransactionType}' cannot create POST_MAINTENANCE. Only MAINTENANCE can.");
            }

            // Check if already has paired transaction
            if (sourceTransaction.FromAssetTransactionId != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Maintenance transaction already has a paired POST_MAINTENANCE");
            }

            var asset = await _assetReps.GetByIdRawAsync(sourceTransaction.AssetId);
            if (asset == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Asset with id {sourceTransaction.AssetId} not found");
            }

            // Create POST_MAINTENANCE transaction
            var postMaintenanceTransaction = new AssetTransactionEntity
            {
                AssetId = sourceTransaction.AssetId,
                TransactionType = TransactionTypeConstants.POST_MAINTENANCE,
                FromAssetTransactionId = sourceTransactionId,
                TransactionDate = model.CompletionDate,
                ConditionAfter = model.ConditionAfter,
                Notes = model.Notes,
                Approved = null, // Pending approval
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };

            var id = await _repository.InsertAsync(postMaintenanceTransaction);

            // Update source transaction with pairing
            sourceTransaction.FromAssetTransactionId = Convert.ToInt32(id);
            sourceTransaction.ModifiedDate = DateTime.Now;
            sourceTransaction.ModifiedBy = _currentUserService.GetDisplayName();
            await _repository.UpdateAsync(sourceTransaction);

            var created = await _transactionReps.GetDetailByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.AssetTransaction,
                    created.AssetTransactionId,
                    $"POST_MAINTENANCE transaction created as shortcut from transaction ID {sourceTransactionId} for Asset ID {created.AssetId}",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<AssetTransactionDetailView>.Failure("Failed to retrieve created post-maintenance transaction")
                : ServiceResult<AssetTransactionDetailView>.Success(created, "Post-maintenance transaction created successfully (Pending approval)");
        }, "create post-maintenance transaction", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.AssetTransaction, 0, "Create Post-Maintenance Transaction", ex, _currentUserService.GetDisplayName());
        });
    }
    
    #region Private Helpers

    private async Task<ServiceResult<AssetTransactionDetailView>?> ValidateTransactionRulesAsync(AssetTransactionCreateViewModel model, AssetEntity asset)
    {
        var type = model.TransactionType;

        var typeExists = await _masterDataLookupService.GetTransactionTypeNameAsync(type);
        if (!typeExists.IsSuccess || typeExists.Data == null)
        {
            return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid TransactionType: {type}");
        }

        if (type == TransactionTypeConstants.HANDOVER)
        {
            if (!model.ToEmployeeId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("HANDOVER requires ToEmployeeId");
            }

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset '{asset.AssetCode}' is currently in an active transaction");
            }
        }

        if (type == TransactionTypeConstants.TRANSFER)
        {
            if (!model.FromEmployeeId.HasValue || !model.ToEmployeeId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("TRANSFER requires both FromEmployeeId and ToEmployeeId");
            }

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction == null || activeTransaction.ToEmployeeId != model.FromEmployeeId)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset '{asset.AssetCode}' is not currently held by employee {model.FromEmployeeId}");
            }
        }

        if (type == TransactionTypeConstants.LOAN)
        {
            if (!model.ToEmployeeId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("LOAN requires ToEmployeeId");
            }

            if (!model.ExpectedReturnDate.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("LOAN requires ExpectedReturnDate");
            }

            if (model.ExpectedReturnDate.Value <= model.TransactionDate)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("ExpectedReturnDate must be after TransactionDate");
            }

            var hasOpenLoan = await _transactionReps.HasOpenPairedTransactionAsync(asset.AssetId, TransactionTypeConstants.LOAN);
            if (hasOpenLoan)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset '{asset.AssetCode}' already has an active loan");
            }
        }

        if (type == TransactionTypeConstants.RETURN)
        {
            if (!model.FromEmployeeId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("RETURN requires FromEmployeeId");
            }

            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction == null || activeTransaction.ToEmployeeId != model.FromEmployeeId)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset '{asset.AssetCode}' is not currently held by employee {model.FromEmployeeId}");
            }

            if (activeTransaction.TransactionType != TransactionTypeConstants.HANDOVER && 
                activeTransaction.TransactionType != TransactionTypeConstants.TRANSFER)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset '{asset.AssetCode}' is not in assigned state");
            }
        }

        if (type == TransactionTypeConstants.LOAN_RETURN)
        {
            if (!model.FromAssetTransactionId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("LOAN_RETURN requires FromAssetTransactionId");
            }

            var pairedTransaction = await _transactionReps.GetByIdRawAsync(model.FromAssetTransactionId.Value);
            if (pairedTransaction == null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Paired transaction with id {model.FromAssetTransactionId} not found");
            }

            if (pairedTransaction.AssetId != model.AssetId)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Paired transaction must be for the same asset");
            }

            if (pairedTransaction.TransactionType != TransactionTypeConstants.LOAN)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Cannot pair LOAN_RETURN with transaction type {pairedTransaction.TransactionType}");
            }

            if (pairedTransaction.FromAssetTransactionId != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Loan transaction is already closed");
            }
        }

        if (type == TransactionTypeConstants.MAINTENANCE)
        {
            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction != null && activeTransaction.TransactionType == TransactionTypeConstants.MAINTENANCE)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset '{asset.AssetCode}' is already in maintenance");
            }
        }

        if (type == TransactionTypeConstants.POST_MAINTENANCE)
        {
            if (!model.FromAssetTransactionId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("POST_MAINTENANCE requires FromAssetTransactionId");
            }

            var pairedTransaction = await _transactionReps.GetByIdRawAsync(model.FromAssetTransactionId.Value);
            if (pairedTransaction == null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Paired transaction with id {model.FromAssetTransactionId} not found");
            }

            if (pairedTransaction.AssetId != model.AssetId)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Paired transaction must be for the same asset");
            }

            if (pairedTransaction.TransactionType != TransactionTypeConstants.MAINTENANCE)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Cannot pair POST_MAINTENANCE with transaction type {pairedTransaction.TransactionType}");
            }

            if (pairedTransaction.FromAssetTransactionId != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Maintenance transaction is already closed");
            }

            if (!model.ConditionAfter.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("POST_MAINTENANCE requires ConditionAfter");
            }
        }

        if (type == TransactionTypeConstants.DISPOSAL)
        {
            var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);
            if (activeTransaction != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset '{asset.AssetCode}' has active transaction, cannot dispose");
            }
        }

        if (model.ToEmployeeId.HasValue)
        {
            var employee = await _employeeReps.GetByIdRawAsync(model.ToEmployeeId.Value);
            if (employee == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Employee with id {model.ToEmployeeId} not found");
            }
        }

        if (model.FromEmployeeId.HasValue)
        {
            var employee = await _employeeReps.GetByIdRawAsync(model.FromEmployeeId.Value);
            if (employee == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Employee with id {model.FromEmployeeId} not found");
            }
        }

        if (model.ToLocationId.HasValue)
        {
            var office = await _officeReps.GetByIdRawAsync(model.ToLocationId.Value);
            if (office == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Office with id {model.ToLocationId} not found");
            }
        }

        if (model.ConditionBefore.HasValue)
        {
            var conditionExists = await _masterDataLookupService.GetAssetConditionNameAsync(model.ConditionBefore.Value);
            if (!conditionExists.IsSuccess || conditionExists.Data == null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid ConditionBefore value: {model.ConditionBefore}");
            }
        }

        if (model.ConditionAfter.HasValue)
        {
            var conditionExists = await _masterDataLookupService.GetAssetConditionNameAsync(model.ConditionAfter.Value);
            if (!conditionExists.IsSuccess || conditionExists.Data == null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid ConditionAfter value: {model.ConditionAfter}");
            }
        }

        if (model.MaintenanceType.HasValue)
        {
            var maintenanceExists = await _masterDataLookupService.GetMaintenanceTypeNameAsync(model.MaintenanceType.Value);
            if (!maintenanceExists.IsSuccess || maintenanceExists.Data == null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid MaintenanceType value: {model.MaintenanceType}");
            }
        }

        return null;
    }

    private void MarkOverdueStatus(IEnumerable<AssetTransactionListView> transactions)
    {
        var now = DateTime.Now;
        foreach (var t in transactions)
        {
            t.IsOverdue = t.TransactionType == TransactionTypeConstants.LOAN
                && t.Approved == true
                && t.FromAssetTransactionId == null
                && t.ExpectedReturnDate.HasValue
                && t.ExpectedReturnDate.Value < now;
        }
    }

    #endregion
}