using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Employee;
using Whitebird.App.Features.FileAttachment;
using Whitebird.App.Features.MasterData;
using Whitebird.App.Features.Office;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.AssetTransaction;

/// <summary>
/// Service implementation for Asset Transaction business logic
/// </summary>
public class AssetTransactionService : BaseService, IAssetTransactionService
{
    private readonly IAssetTransactionReps _transactionReps;
    private readonly IAssetService _assetService;
    private readonly IEmployeeService _employeeService;
    private readonly IOfficeService _officeService;
    private readonly IMasterDataService _masterDataService;
    private readonly IFileAttachmentService _fileAttachmentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    private static readonly int[] _addTypes = {
        TransactionTypeConstants.HANDOVER,
        TransactionTypeConstants.TRANSFER,
        TransactionTypeConstants.LOAN,
        TransactionTypeConstants.MAINTENANCE,
        TransactionTypeConstants.DISPOSAL,
        TransactionTypeConstants.OPERATIONAL_OFFICE
    };

    public AssetTransactionService(
        IAssetTransactionReps transactionReps,
        IAssetService assetService,
        IEmployeeService employeeService,
        IOfficeService officeService,
        IMasterDataService masterDataService,
        IFileAttachmentService fileAttachmentService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<AssetTransactionService> logger) : base(logger)
    {
        _transactionReps = transactionReps;
        _assetService = assetService;
        _employeeService = employeeService;
        _officeService = officeService;
        _masterDataService = masterDataService;
        _fileAttachmentService = fileAttachmentService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    // ============================================================
    // CRUD
    // ============================================================

    public async Task<ServiceResult<AssetTransactionDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetDetailByIdAsync(id);
            if (result == null)
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {id} not found");
            return ServiceResult<AssetTransactionDetailView>.Success(result);
        }, "get transaction by id");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var request = new AssetTransactionGridRequest
            {
                Page = 1,
                PageSize = int.MaxValue,
                SortDescending = true
            };

            var addResult = await _transactionReps.GetGridAddAsync(request);
            var returnResult = await _transactionReps.GetGridReturnAsync(request);

            var allData = addResult.Data.Concat(returnResult.Data);
            return ServiceResult<IEnumerable<AssetTransactionListView>>.Success(allData);
        }, "get all transactions");
    }

    public async Task<ServiceResult<AssetTransactionDetailView>> CreateAsync(AssetTransactionCreateViewModel model)
    {
        if (!_addTypes.Contains(model.TransactionType))
            return ServiceResult<AssetTransactionDetailView>.BadRequest(
                $"Transaction type {model.TransactionType} cannot be created manually. Use shortcut endpoint.");

        var validationResult = await ValidateTransactionAsync(model, null);
        if (validationResult != null) return validationResult;

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<AssetTransactionEntity>();
            entity.IsActive = true;
            entity.Approved = null;
            entity.Completed = null;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();
            entity.AssetTransactionCode = GenerateTransactionCode(model.TransactionType);

            var id = await _transactionReps.InsertAsync(entity);

            if (entity.FromAssetTransactionId.HasValue)
            {
                var source = await _transactionReps.GetByIdRawAsync(entity.FromAssetTransactionId.Value);
                if (source != null && source.FromAssetTransactionId == null)
                {
                    source.FromAssetTransactionId = id;
                    source.ModifiedDate = DateTime.Now;
                    source.ModifiedBy = _currentUserService.GetDisplayName();
                    await _transactionReps.UpdateAsync(source);
                }
            }

            var created = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogCreateAsync(
                TableNames.AssetTransaction, id,
                $"Transaction created: Type {model.TransactionType}, Asset {model.AssetId}",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailView>.Success(created!, "Transaction created successfully");
        }, "create transaction");
    }

    public async Task<ServiceResult<AssetTransactionDetailView>> UpdateAsync(int id, AssetTransactionUpdateViewModel model)
    {
        var validation = await ValidateTransactionReferencesAsync(
            model.AssetId, model.TransactionType, model.EmployeeId, model.OfficeId,
            model.Condition, model.MaintenanceType, model.TransactionDate, model.ExpectedLoanReturnDate);
        if (validation != null) return validation;

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null) return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {id} not found");

            if (existing.Approved != null)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Cannot update approved/rejected transaction");

            existing.AssetId = model.AssetId;
            existing.TransactionType = model.TransactionType;
            existing.EmployeeId = model.EmployeeId;
            existing.OfficeId = model.OfficeId;
            existing.TransactionDate = model.TransactionDate;
            existing.ExpectedLoanReturnDate = model.ExpectedLoanReturnDate;
            existing.Notes = model.Notes;
            existing.Condition = model.Condition;
            existing.MaintenanceType = model.MaintenanceType;
            existing.MaintenanceCost = model.MaintenanceCost;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            await _transactionReps.UpdateAsync(existing);
            await ReevaluateCompletedAsync(id);

            var updated = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction, id,
                $"Transaction updated: Type {model.TransactionType}",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailView>.Success(updated!, "Transaction updated successfully");
        }, "update transaction");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null) return ServiceResult.NotFound($"Transaction with id {id} not found");

            var paired = await _transactionReps.GetPairedTransactionAsync(id);
            if (paired != null)
                return ServiceResult.BadRequest("Cannot delete transaction that has a paired transaction");

            // H9: FK dependency checks — a hard delete on AssetTransaction fires
            // every inbound FK, so verify them all before touching the parent row.
            var attachmentCount = await _transactionReps.GetAttachmentCountAsync(id);
            if (attachmentCount > 0)
                return ServiceResult.BadRequest(
                    $"Cannot delete transaction with {attachmentCount} attachment(s)");

            var approveAttachmentCount = await _transactionReps.GetApproveAttachmentCountAsync(id);
            if (approveAttachmentCount > 0)
                return ServiceResult.BadRequest(
                    $"Cannot delete transaction with {approveAttachmentCount} approval-document attachment(s)");

            var handoverCount = await _transactionReps.GetScheduleHandoverCountAsync(id);
            if (handoverCount > 0)
                return ServiceResult.BadRequest(
                    $"Cannot delete transaction with {handoverCount} linked handover schedule(s)");

            var returnCount = await _transactionReps.GetScheduleReturnCountAsync(id);
            if (returnCount > 0)
                return ServiceResult.BadRequest(
                    $"Cannot delete transaction with {returnCount} linked return schedule(s)");

            var maintenanceCount = await _transactionReps.GetScheduleMaintenanceCountAsync(id);
            if (maintenanceCount > 0)
                return ServiceResult.BadRequest(
                    $"Cannot delete transaction with {maintenanceCount} linked maintenance schedule(s)");

            await _transactionReps.DeleteAsync(id);

            await _activityLogService.LogDeleteAsync(
                TableNames.AssetTransaction, id,
                $"Transaction deleted permanently",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Transaction deleted successfully");
        }, "delete transaction");
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null) return ServiceResult.NotFound($"Transaction with id {id} not found");

            var paired = await _transactionReps.GetPairedTransactionAsync(id);
            if (paired != null)
                return ServiceResult.BadRequest("Cannot soft delete transaction that has a paired transaction");

            await _transactionReps.SoftDeleteAsync(id);

            await _activityLogService.LogSoftDeleteAsync(
                TableNames.AssetTransaction, id,
                $"Transaction soft deleted",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Transaction soft deleted successfully");
        }, "soft delete transaction");
    }

    // ============================================================
    // UPDATE WITH ATTACHMENTS
    // ============================================================

    public async Task<ServiceResult<AssetTransactionDetailView>> UpdateWithAttachmentsAsync(
        int id, AssetTransactionUpdateFormDto model)
    {
        var validation = await ValidateTransactionReferencesAsync(
            model.AssetId, model.TransactionType, model.EmployeeId, model.OfficeId,
            model.Condition, model.MaintenanceType, model.TransactionDate, model.ExpectedLoanReturnDate);
        if (validation != null) return validation;

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null) return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {id} not found");

            existing.AssetId = model.AssetId;
            existing.TransactionType = model.TransactionType;
            existing.EmployeeId = model.EmployeeId;
            existing.OfficeId = model.OfficeId;
            existing.TransactionDate = model.TransactionDate;
            existing.ExpectedLoanReturnDate = model.ExpectedLoanReturnDate;
            existing.Notes = model.Notes;
            existing.Condition = model.Condition;
            existing.MaintenanceType = model.MaintenanceType;
            existing.MaintenanceCost = model.MaintenanceCost;
            existing.IsActive = model.IsActive;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            await _transactionReps.UpdateAsync(existing);

            if (model.ReplaceAttachments || (model.AttachmentFilesBase64 != null && model.AttachmentFilesBase64.Any()))
            {
                var existingAttachments = await _transactionReps.GetAttachmentsByTransactionIdAsync(id);
                var keepIds = model.AttachmentIdsToKeep ?? new List<int>();

                foreach (var att in existingAttachments)
                {
                    if (!keepIds.Contains(att.FileAttachmentId))
                    {
                        await _fileAttachmentService.DeleteAsync(att.FileAttachmentId);
                        var attEntity = await _transactionReps.GetAttachmentByIdAsync(att.AssetTransactionAttachmentId);
                        if (attEntity != null)
                            await _transactionReps.DeleteAttachmentAsync(attEntity.AssetTransactionAttachmentId);
                    }
                }

                if (model.AttachmentFilesBase64 != null && model.AttachmentFilesBase64.Any())
                {
                    int order = existingAttachments.Count(a => keepIds.Contains(a.FileAttachmentId));
                    for (int i = 0; i < model.AttachmentFilesBase64.Count; i++)
                    {
                        var base64 = model.AttachmentFilesBase64[i];
                        var fileName = model.AttachmentFileNames?[i] ?? $"attachment_{i}.pdf";
                        if (!string.IsNullOrEmpty(base64))
                        {
                            await AddAttachmentFromBase64Async(id, base64, fileName, order);
                            order++;
                        }
                    }
                }
            }

            if (model.ReplaceApproveAttachments || (model.ApproveAttachments != null && model.ApproveAttachments.Any()))
            {
                var existingApproveAttachments = await _transactionReps.GetApproveAttachmentsByTransactionIdAsync(id);
                var keepIds = model.ApproveAttachmentIdsToKeep ?? new List<int>();

                foreach (var att in existingApproveAttachments)
                {
                    if (!keepIds.Contains(att.FileAttachmentId))
                    {
                        await _fileAttachmentService.DeleteAsync(att.FileAttachmentId);
                        var attEntity = await _transactionReps.GetApproveAttachmentByIdAsync(att.AssetTransactionApproveAttachmentId);
                        if (attEntity != null)
                            await _transactionReps.DeleteApproveAttachmentAsync(attEntity.AssetTransactionApproveAttachmentId);
                    }
                }

                if (model.ApproveAttachments != null && model.ApproveAttachments.Any())
                {
                    int order = existingApproveAttachments.Count(a => keepIds.Contains(a.FileAttachmentId));
                    foreach (var approveAtt in model.ApproveAttachments)
                    {
                        if (!string.IsNullOrEmpty(approveAtt.FileBase64))
                        {
                            await AddApproveAttachmentFromBase64Async(id, approveAtt.FileBase64, approveAtt.FileName, approveAtt.DocumentType, order);
                            order++;
                        }
                    }
                }
            }

            await ReevaluateCompletedAsync(id);

            var updated = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction, id,
                $"Transaction updated with attachments",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailView>.Success(updated!, "Transaction updated successfully");
        }, "update transaction with attachments");
    }

    public async Task<ServiceResult<AssetTransactionDetailView>> UpdateApprovalWithAttachmentsAsync(
        int id, AssetTransactionUpdateApprovalFormDto model)
    {
        var validation = await ValidateTransactionReferencesAsync(
            model.AssetId, model.TransactionType, model.EmployeeId, model.OfficeId,
            model.Condition, model.MaintenanceType, model.TransactionDate, model.ExpectedLoanReturnDate);
        if (validation != null) return validation;

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null) return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {id} not found");

            existing.AssetId = model.AssetId;
            existing.TransactionType = model.TransactionType;
            existing.EmployeeId = model.EmployeeId;
            existing.OfficeId = model.OfficeId;
            existing.TransactionDate = model.TransactionDate;
            existing.ExpectedLoanReturnDate = model.ExpectedLoanReturnDate;
            existing.Notes = model.Notes;
            existing.Condition = model.Condition;
            existing.MaintenanceType = model.MaintenanceType;
            existing.MaintenanceCost = model.MaintenanceCost;
            existing.IsActive = model.IsActive;
            existing.Approved = model.Approved;
            existing.ApprovedBy = model.ApprovedBy;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            await _transactionReps.UpdateAsync(existing);

            if (model.ReplaceApproveAttachments || (model.ApproveAttachments != null && model.ApproveAttachments.Any()))
            {
                var existingApproveAttachments = await _transactionReps.GetApproveAttachmentsByTransactionIdAsync(id);
                var keepIds = model.ApproveAttachmentIdsToKeep ?? new List<int>();

                foreach (var att in existingApproveAttachments)
                {
                    if (!keepIds.Contains(att.FileAttachmentId))
                    {
                        await _fileAttachmentService.DeleteAsync(att.FileAttachmentId);
                        var attEntity = await _transactionReps.GetApproveAttachmentByIdAsync(att.AssetTransactionApproveAttachmentId);
                        if (attEntity != null)
                            await _transactionReps.DeleteApproveAttachmentAsync(attEntity.AssetTransactionApproveAttachmentId);
                    }
                }

                if (model.ApproveAttachments != null && model.ApproveAttachments.Any())
                {
                    int order = existingApproveAttachments.Count(a => keepIds.Contains(a.FileAttachmentId));
                    foreach (var approveAtt in model.ApproveAttachments)
                    {
                        if (!string.IsNullOrEmpty(approveAtt.FileBase64))
                        {
                            await AddApproveAttachmentFromBase64Async(id, approveAtt.FileBase64, approveAtt.FileName, approveAtt.DocumentType, order);
                            order++;
                        }
                    }
                }
            }

            await ReevaluateCompletedAsync(id);

            var updated = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction, id,
                $"Transaction approval updated with attachments",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailView>.Success(updated!, "Transaction approval updated successfully");
        }, "update transaction approval with attachments");
    }

    // ============================================================
    // BULK APPROVAL
    // ============================================================

    public async Task<ServiceResult<int>> BulkApprovedAsync(BulkApprovalRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
            return ServiceResult<int>.BadRequest("No transaction IDs provided");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var count = 0;
            foreach (var id in request.Ids)
            {
                var transaction = await _transactionReps.GetByIdRawAsync(id);
                if (transaction != null && transaction.Approved == null && transaction.IsActive == true)
                {
                    transaction.Approved = true;
                    transaction.ApprovedBy = _currentUserService.GetDisplayName();
                    transaction.ModifiedDate = DateTime.Now;
                    transaction.ModifiedBy = _currentUserService.GetDisplayName();
                    await _transactionReps.UpdateAsync(transaction);
                    count++;
                    await ReevaluateCompletedAsync(id);
                }
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction, 0,
                $"{count} transaction(s) approved via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(count, $"{count} transaction(s) approved successfully");
        }, "bulk approved");
    }

    public async Task<ServiceResult<int>> BulkPendingAsync(BulkApprovalRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
            return ServiceResult<int>.BadRequest("No transaction IDs provided");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var count = 0;
            foreach (var id in request.Ids)
            {
                var transaction = await _transactionReps.GetByIdRawAsync(id);
                if (transaction != null && transaction.Approved == true && transaction.IsActive == true)
                {
                    transaction.Approved = null;
                    transaction.ApprovedBy = null;
                    transaction.ModifiedDate = DateTime.Now;
                    transaction.ModifiedBy = _currentUserService.GetDisplayName();
                    await _transactionReps.UpdateAsync(transaction);
                    count++;
                    await ReevaluateCompletedAsync(id);
                }
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction, 0,
                $"{count} transaction(s) moved to pending via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(count, $"{count} transaction(s) moved to pending successfully");
        }, "bulk pending");
    }

    public async Task<ServiceResult<int>> BulkRejectedAsync(BulkApprovalRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
            return ServiceResult<int>.BadRequest("No transaction IDs provided");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var count = 0;
            foreach (var id in request.Ids)
            {
                var transaction = await _transactionReps.GetByIdRawAsync(id);
                if (transaction != null && transaction.Approved != false && transaction.IsActive == true)
                {
                    transaction.Approved = false;
                    transaction.ApprovedBy = _currentUserService.GetDisplayName();
                    transaction.IsActive = false;
                    transaction.Completed = false;
                    transaction.CompletedBy = null;
                    transaction.ModifiedDate = DateTime.Now;
                    transaction.ModifiedBy = _currentUserService.GetDisplayName();
                    await _transactionReps.UpdateAsync(transaction);
                    count++;
                }
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction, 0,
                $"{count} transaction(s) rejected via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(count, $"{count} transaction(s) rejected successfully");
        }, "bulk rejected");
    }

    // ============================================================
    // COMPLETE / UNCOMPLETE
    // ============================================================

    public async Task<ServiceResult> CompleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(id);
            if (transaction == null) return ServiceResult.NotFound($"Transaction with id {id} not found");

            if (transaction.Approved != true)
                return ServiceResult.BadRequest("Transaction must be approved before complete");

            if (transaction.Completed == true)
                return ServiceResult.BadRequest("Transaction already completed");

            var requiredDocs = (await _transactionReps.GetRequiredDocumentTypesAsync(id)).ToList();
            if (requiredDocs.Count == 0)
                return ServiceResult.BadRequest("Cannot complete: no approval document types configured in MasterData");

            if (!requiredDocs.All(d => d.IsUploaded))
                return ServiceResult.BadRequest("All required documents must be uploaded before complete");

            var allApproved = await _transactionReps.IsAllRequiredDocumentsApprovedAsync(id);
            if (!allApproved)
                return ServiceResult.BadRequest("All uploaded documents must be approved before complete");

            await ReevaluateCompletedAsync(id);

            var reloaded = await _transactionReps.GetByIdRawAsync(id);
            if (reloaded?.Completed != true)
                return ServiceResult.Failure("Failed to complete transaction: document state re-check did not yield Completed = true");

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction, id,
                $"Transaction completed",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Transaction completed successfully");
        }, "complete transaction");
    }

    public async Task<ServiceResult> UncompleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(id);
            if (transaction == null) return ServiceResult.NotFound($"Transaction with id {id} not found");

            if (transaction.Completed != true)
                return ServiceResult.BadRequest("Transaction is not completed");

            await ReevaluateCompletedAsync(id);

            var reloaded = await _transactionReps.GetByIdRawAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction, id,
                $"Transaction uncomplete requested; re-evaluated Completed = {reloaded?.Completed}",
                _currentUserService.GetDisplayName());

            if (reloaded?.Completed == true)
            {
                return ServiceResult.BadRequest(
                    "Cannot uncomplete: all required approval documents are still approved. " +
                    "Mark a document for revision or delete a required approval attachment to revert the transaction to Uncompleted.");
            }

            return ServiceResult.Success("Transaction uncompleted successfully");
        }, "uncomplete transaction");
    }

    // ============================================================
    // GRID
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetGridAddAsync(AssetTransactionGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetGridAddAsync(request);
            return ServiceResult<PaginatedResult<AssetTransactionListView>>.Success(result);
        }, "get grid add");
    }

    public async Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetGridReturnAsync(AssetTransactionGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetGridReturnAsync(request);
            return ServiceResult<PaginatedResult<AssetTransactionListView>>.Success(result);
        }, "get grid return");
    }

    public async Task<ServiceResult<PaginatedResult<AssetTransactionPendingApprovalView>>> GetGridPendingApprovalAsync(AssetTransactionGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetGridPendingApprovalAsync(request);
            return ServiceResult<PaginatedResult<AssetTransactionPendingApprovalView>>.Success(result);
        }, "get grid pending approval");
    }

    public async Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetCompleteTransactionsAsync(AssetTransactionGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetCompleteTransactionsAsync(request);
            return ServiceResult<PaginatedResult<AssetTransactionListView>>.Success(result);
        }, "get complete transactions");
    }

    public async Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetActiveLoansAsync(AssetTransactionGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetActiveLoansAsync(request);
            return ServiceResult<PaginatedResult<AssetTransactionListView>>.Success(result);
        }, "get active loans");
    }

    public async Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetOverdueLoansAsync(AssetTransactionGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetOverdueLoansAsync(request);
            return ServiceResult<PaginatedResult<AssetTransactionListView>>.Success(result);
        }, "get overdue loans");
    }

    public async Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetUnderMaintenanceAsync(AssetTransactionGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _transactionReps.GetUnderMaintenanceAsync(request);
            return ServiceResult<PaginatedResult<AssetTransactionListView>>.Success(result);
        }, "get under maintenance");
    }

    // ============================================================
    // SHORTCUT
    // ============================================================

    public async Task<ServiceResult<AssetTransactionDetailView>> CreateReturnAsync(int sourceTransactionId, AssetReturnViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var source = await _transactionReps.GetByIdRawAsync(sourceTransactionId);
            if (source == null) return ServiceResult<AssetTransactionDetailView>.NotFound($"Source transaction with id {sourceTransactionId} not found");

            var returnableTypes = new[] {
                TransactionTypeConstants.HANDOVER,
                TransactionTypeConstants.LOAN,
                TransactionTypeConstants.OPERATIONAL_OFFICE
            };

            if (!returnableTypes.Contains(source.TransactionType))
                return ServiceResult<AssetTransactionDetailView>.BadRequest(
                    $"Transaction type {source.TransactionType} cannot be returned. Allowed: HANDOVER, LOAN, OPERATIONAL OFFICE");

            if (source.Approved != true)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction must be approved");

            if (source.FromAssetTransactionId != null)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction already has a paired return");

            int returnType;
            if (source.TransactionType == TransactionTypeConstants.LOAN)
                returnType = TransactionTypeConstants.LOAN_RETURN;
            else
                returnType = TransactionTypeConstants.RETURN;

            var returnEntity = new AssetTransactionEntity
            {
                AssetId = source.AssetId,
                TransactionType = returnType,
                EmployeeId = source.EmployeeId,
                TransactionDate = model.ActualReturnDate,
                Notes = model.Notes,
                Condition = model.Condition,
                FromAssetTransactionId = sourceTransactionId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };

            returnEntity.AssetTransactionCode = GenerateTransactionCode(returnType);

            var id = await _transactionReps.InsertAsync(returnEntity);

            source.FromAssetTransactionId = id;
            source.ModifiedDate = DateTime.Now;
            source.ModifiedBy = _currentUserService.GetDisplayName();
            await _transactionReps.UpdateAsync(source);

            var created = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogCreateAsync(
                TableNames.AssetTransaction, id,
                $"RETURN created from transaction {sourceTransactionId}",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailView>.Success(created!, "Return transaction created successfully");
        }, "create return shortcut");
    }

    public async Task<ServiceResult<AssetTransactionDetailView>> CreatePostMaintenanceAsync(int sourceTransactionId, PostMaintenanceViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var source = await _transactionReps.GetByIdRawAsync(sourceTransactionId);
            if (source == null) return ServiceResult<AssetTransactionDetailView>.NotFound($"Source transaction with id {sourceTransactionId} not found");

            if (source.TransactionType != TransactionTypeConstants.MAINTENANCE)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Only MAINTENANCE can create POST_MAINTENANCE");

            if (source.Approved != true)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction must be approved");

            if (source.FromAssetTransactionId != null)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction already has a paired post-maintenance");

            var postEntity = new AssetTransactionEntity
            {
                AssetId = source.AssetId,
                TransactionType = TransactionTypeConstants.POST_MAINTENANCE,
                TransactionDate = model.CompletionDate,
                Notes = model.Notes,
                Condition = model.Condition,
                MaintenanceCost = model.MaintenanceCost,
                MaintenanceType = source.MaintenanceType,
                FromAssetTransactionId = sourceTransactionId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };

            postEntity.AssetTransactionCode = GenerateTransactionCode(TransactionTypeConstants.POST_MAINTENANCE);

            var id = await _transactionReps.InsertAsync(postEntity);

            source.FromAssetTransactionId = id;
            source.ModifiedDate = DateTime.Now;
            source.ModifiedBy = _currentUserService.GetDisplayName();
            await _transactionReps.UpdateAsync(source);

            var created = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogCreateAsync(
                TableNames.AssetTransaction, id,
                $"POST_MAINTENANCE created from transaction {sourceTransactionId}",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailView>.Success(created!, "Post-maintenance transaction created successfully");
        }, "create post-maintenance shortcut");
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypeAddListAsync()
    {
        var result = await _masterDataService.GetTransactionTypesAsync();
        if (!result.IsSuccess || result.Data == null)
            return ServiceResult<IEnumerable<MasterDataDto>>.Failure("Failed to get transaction types");

        var filtered = result.Data.Where(d => _addTypes.Contains(d.Code));
        return ServiceResult<IEnumerable<MasterDataDto>>.Success(filtered);
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypeReturnListAsync()
    {
        var result = await _masterDataService.GetTransactionTypesAsync();
        if (!result.IsSuccess || result.Data == null)
            return ServiceResult<IEnumerable<MasterDataDto>>.Failure("Failed to get transaction types");

        var returnTypes = new[] {
            TransactionTypeConstants.RETURN,
            TransactionTypeConstants.LOAN_RETURN,
            TransactionTypeConstants.POST_MAINTENANCE
        };

        var filtered = result.Data.Where(d => returnTypes.Contains(d.Code));
        return ServiceResult<IEnumerable<MasterDataDto>>.Success(filtered);
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMaintenanceTypeListAsync()
    {
        return await _masterDataService.GetMaintenanceTypesAsync();
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetConditionListAsync()
    {
        return await _masterDataService.GetAssetConditionsAsync();
    }

    public async Task<ServiceResult<IEnumerable<ApproveDocumentTypeDto>>> GetApproveDocumentTypeListAsync()
    {
        var result = await _masterDataService.GetByReferenceNameAsync("ApproveDocumentType");
        if (!result.IsSuccess || result.Data == null)
            return ServiceResult<IEnumerable<ApproveDocumentTypeDto>>.Failure("Failed to get approve document types");

        var dtos = result.Data.Select(d => new ApproveDocumentTypeDto
        {
            DocumentType = d.Code,
            DocumentTypeName = d.Name,
            IsRequired = true
        });

        return ServiceResult<IEnumerable<ApproveDocumentTypeDto>>.Success(dtos);
    }

    public async Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetEmployeeListAsync()
    {
        return await _employeeService.GetDropdownListAsync();
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionAssetDto>>> GetAssetListAsync(int transactionTypeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assetsResult = await _assetService.GetDropdownListByTransactionTypeAsync(transactionTypeId);
            if (!assetsResult.IsSuccess || assetsResult.Data == null)
                return ServiceResult<IEnumerable<AssetTransactionAssetDto>>.Failure("Failed to get assets");

            var result = assetsResult.Data.Select(a => new AssetTransactionAssetDto
            {
                AssetId = a.AssetId,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName
            });
            return ServiceResult<IEnumerable<AssetTransactionAssetDto>>.Success(result);
        }, "get asset list");
    }

    // ============================================================
    // ATTACHMENTS (standalone retrieval)
    // ============================================================

    public async Task<ServiceResult<IEnumerable<AssetTransactionAttachmentView>>> GetAttachmentsAsync(int transactionId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(transactionId);
            if (transaction == null)
                return ServiceResult<IEnumerable<AssetTransactionAttachmentView>>.NotFound($"Transaction with id {transactionId} not found");

            var attachments = await _transactionReps.GetAttachmentsByTransactionIdAsync(transactionId);
            return ServiceResult<IEnumerable<AssetTransactionAttachmentView>>.Success(attachments);
        }, "get transaction attachments");
    }

    public async Task<ServiceResult<IEnumerable<AssetTransactionApproveAttachmentView>>> GetApproveAttachmentsAsync(int transactionId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(transactionId);
            if (transaction == null)
                return ServiceResult<IEnumerable<AssetTransactionApproveAttachmentView>>.NotFound($"Transaction with id {transactionId} not found");

            var attachments = await _transactionReps.GetApproveAttachmentsByTransactionIdAsync(transactionId);
            return ServiceResult<IEnumerable<AssetTransactionApproveAttachmentView>>.Success(attachments);
        }, "get transaction approve attachments");
    }

    public async Task<ServiceResult<FileContentDto>> GetAttachmentFileAsync(int transactionId, int fileAttachmentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(transactionId);
            if (transaction == null) return ServiceResult<FileContentDto>.NotFound($"Transaction with id {transactionId} not found");

            var attachments = await _transactionReps.GetAttachmentsByTransactionIdAsync(transactionId);
            if (!attachments.Any(a => a.FileAttachmentId == fileAttachmentId))
                return ServiceResult<FileContentDto>.NotFound($"Attachment {fileAttachmentId} not found for transaction {transactionId}");

            var downloadResult = await _fileAttachmentService.DownloadAsync(fileAttachmentId);
            if (!downloadResult.IsSuccess || downloadResult.Data == null)
                return ServiceResult<FileContentDto>.NotFound($"Attachment file {fileAttachmentId} could not be retrieved");

            return downloadResult;
        }, "get transaction attachment file");
    }

    public async Task<ServiceResult<FileContentDto>> GetApproveAttachmentFileAsync(int transactionId, int fileAttachmentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(transactionId);
            if (transaction == null) return ServiceResult<FileContentDto>.NotFound($"Transaction with id {transactionId} not found");

            var attachments = await _transactionReps.GetApproveAttachmentsByTransactionIdAsync(transactionId);
            if (!attachments.Any(a => a.FileAttachmentId == fileAttachmentId))
                return ServiceResult<FileContentDto>.NotFound($"Approve attachment {fileAttachmentId} not found for transaction {transactionId}");

            var downloadResult = await _fileAttachmentService.DownloadAsync(fileAttachmentId);
            if (!downloadResult.IsSuccess || downloadResult.Data == null)
                return ServiceResult<FileContentDto>.NotFound($"Approve attachment file {fileAttachmentId} could not be retrieved");

            return downloadResult;
        }, "get transaction approve attachment file");
    }

    // ============================================================
    // APPROVAL DOCUMENT WORKFLOW
    // ============================================================

    public async Task<ServiceResult<IEnumerable<RequiredDocumentTypeDto>>> GetRequiredDocumentsAsync(int transactionId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(transactionId);
            if (transaction == null)
                return ServiceResult<IEnumerable<RequiredDocumentTypeDto>>.NotFound($"Transaction with id {transactionId} not found");

            var required = await _transactionReps.GetRequiredDocumentTypesAsync(transactionId);
            return ServiceResult<IEnumerable<RequiredDocumentTypeDto>>.Success(required);
        }, "get required documents");
    }

    public async Task<ServiceResult<AssetTransactionDetailView>> SetApproveAttachmentActionAsync(
        int transactionId, int attachmentId, ApproveAttachmentActionDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Action))
            return ServiceResult<AssetTransactionDetailView>.BadRequest("Action is required");

        var action = dto.Action.Trim().ToLowerInvariant();
        if (action != "approve" && action != "revision")
            return ServiceResult<AssetTransactionDetailView>.BadRequest(
                $"Invalid action '{dto.Action}'. Supported: approve, revision");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var transaction = await _transactionReps.GetByIdRawAsync(transactionId);
            if (transaction == null) return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {transactionId} not found");

            var attachment = await _transactionReps.GetApproveAttachmentByIdAsync(attachmentId);
            if (attachment == null) return ServiceResult<AssetTransactionDetailView>.NotFound($"Approve attachment with id {attachmentId} not found");

            if (attachment.AssetTransactionId != transactionId)
                return ServiceResult<AssetTransactionDetailView>.BadRequest(
                    $"Approve attachment {attachmentId} does not belong to transaction {transactionId}");

            if (action == "approve")
            {
                if (attachment.Approved)
                    return ServiceResult<AssetTransactionDetailView>.BadRequest("Document is already approved");
                if (attachment.NeedToChange)
                    return ServiceResult<AssetTransactionDetailView>.BadRequest(
                        "Document is marked for revision. Upload the revised document before approving.");
                attachment.Approved = true;
                attachment.NeedToChange = false;
            }
            else
            {
                if (attachment.Approved)
                    return ServiceResult<AssetTransactionDetailView>.BadRequest(
                        "Cannot mark an approved document for revision. Re-upload the document to replace it.");
                attachment.Approved = false;
                attachment.NeedToChange = true;
            }

            attachment.ModifiedDate = DateTime.Now;
            attachment.ModifiedBy = _currentUserService.GetDisplayName();
            await _transactionReps.UpdateApproveAttachmentAsync(attachment);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction, transactionId,
                $"Approval document {attachmentId} (Type {attachment.DocumentType}) action: {action}",
                _currentUserService.GetDisplayName());

            await ReevaluateCompletedAsync(transactionId);

            var updated = await _transactionReps.GetDetailByIdAsync(transactionId);
            return ServiceResult<AssetTransactionDetailView>.Success(updated!, $"Document action '{action}' applied successfully");
        }, "set approve attachment action");
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private async Task ReevaluateCompletedAsync(int transactionId)
    {
        var transaction = await _transactionReps.GetByIdRawAsync(transactionId);
        if (transaction == null) return;

        bool shouldBeCompleted = false;

        if (transaction.IsActive == true && transaction.Approved == true)
        {
            var requiredDocs = (await _transactionReps.GetRequiredDocumentTypesAsync(transactionId)).ToList();

            if (requiredDocs.Count == 0)
            {
                _logger.LogWarning(
                    "Transaction {TransactionId}: no ApproveDocumentType entries configured in MasterData; Completed will not be set.",
                    transactionId);
            }
            else
            {
                var allUploaded = requiredDocs.All(d => d.IsUploaded);
                if (allUploaded)
                {
                    var allApproved = await _transactionReps.IsAllRequiredDocumentsApprovedAsync(transactionId);
                    shouldBeCompleted = allApproved;
                }
            }
        }

        if (transaction.Completed != shouldBeCompleted)
        {
            transaction.Completed = shouldBeCompleted;
            transaction.CompletedBy = shouldBeCompleted ? _currentUserService.GetDisplayName() : null;
            transaction.ModifiedDate = DateTime.Now;
            transaction.ModifiedBy = _currentUserService.GetDisplayName();
            await _transactionReps.UpdateAsync(transaction);
        }
    }

    private async Task<ServiceResult<AssetTransactionDetailView>?> ValidateTransactionReferencesAsync(
        int assetId,
        int transactionType,
        int? employeeId,
        int? officeId,
        int? condition,
        int? maintenanceType,
        DateTime transactionDate,
        DateTime? expectedLoanReturnDate)
    {
        if (!_addTypes.Contains(transactionType))
            return ServiceResult<AssetTransactionDetailView>.BadRequest(
                $"Transaction type {transactionType} cannot be set manually. Use shortcut endpoint.");

        var assetResult = await _assetService.GetByIdAsync(assetId);
        if (!assetResult.IsSuccess || assetResult.Data == null)
            return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset with id {assetId} not found");

        var typeName = await _masterDataService.GetValueAsync("TransactionType", transactionType);
        if (!typeName.IsSuccess || typeName.Data == null)
            return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid TransactionType: {transactionType}");

        if (transactionType == TransactionTypeConstants.HANDOVER && !employeeId.HasValue)
            return ServiceResult<AssetTransactionDetailView>.BadRequest("HANDOVER requires EmployeeId");

        if (transactionType == TransactionTypeConstants.TRANSFER && !employeeId.HasValue)
            return ServiceResult<AssetTransactionDetailView>.BadRequest("TRANSFER requires EmployeeId (receiver)");

        if (transactionType == TransactionTypeConstants.LOAN)
        {
            if (!employeeId.HasValue)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("LOAN requires EmployeeId");
            if (!expectedLoanReturnDate.HasValue)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("LOAN requires ExpectedLoanReturnDate");
            if (expectedLoanReturnDate.Value <= transactionDate)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("ExpectedLoanReturnDate must be after TransactionDate");
        }

        if (transactionType == TransactionTypeConstants.MAINTENANCE && !maintenanceType.HasValue)
            return ServiceResult<AssetTransactionDetailView>.BadRequest("MAINTENANCE requires MaintenanceType");

        if (transactionType == TransactionTypeConstants.OPERATIONAL_OFFICE && !officeId.HasValue)
            return ServiceResult<AssetTransactionDetailView>.BadRequest("OPERATIONAL OFFICE requires OfficeId");

        if (employeeId.HasValue)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId.Value);
            if (!employee.IsSuccess || employee.Data == null)
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Employee with id {employeeId} not found");
        }

        if (officeId.HasValue)
        {
            var office = await _officeService.GetByIdAsync(officeId.Value);
            if (!office.IsSuccess || office.Data == null)
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Office with id {officeId} not found");
        }

        if (condition.HasValue)
        {
            var cond = await _masterDataService.GetValueAsync("AssetCondition", condition.Value);
            if (!cond.IsSuccess || cond.Data == null)
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid Condition: {condition}");
        }

        if (maintenanceType.HasValue)
        {
            var mt = await _masterDataService.GetValueAsync("MaintenanceType", maintenanceType.Value);
            if (!mt.IsSuccess || mt.Data == null)
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid MaintenanceType: {maintenanceType}");
        }

        return null;
    }

    private async Task<ServiceResult<AssetTransactionDetailView>?> ValidateTransactionAsync(
        AssetTransactionCreateViewModel model, AssetTransactionEntity? existing)
    {
        var shared = await ValidateTransactionReferencesAsync(
            model.AssetId,
            model.TransactionType,
            model.EmployeeId,
            model.OfficeId,
            model.Condition,
            model.MaintenanceType,
            model.TransactionDate,
            model.ExpectedLoanReturnDate);

        if (shared != null) return shared;

        if (model.TransactionType == TransactionTypeConstants.TRANSFER && !model.FromAssetTransactionId.HasValue)
            return ServiceResult<AssetTransactionDetailView>.BadRequest("TRANSFER requires FromAssetTransactionId");

        if (model.FromAssetTransactionId.HasValue)
        {
            var source = await _transactionReps.GetByIdRawAsync(model.FromAssetTransactionId.Value);
            if (source == null || source.IsActive == false)
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Source transaction {model.FromAssetTransactionId} not found");
            if (source.Approved != true)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction must be approved");
            if (source.FromAssetTransactionId != null)
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction already has a paired transaction");
        }

        if (!model.FromAssetTransactionId.HasValue)
        {
            var active = await _transactionReps.GetActiveTransactionByAssetIdAsync(model.AssetId);
            if (active != null)
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset is currently in active transaction: {active.TransactionTypeName}");
        }

        return null;
    }

    private string GenerateTransactionCode(int transactionType)
    {
        var prefix = transactionType switch
        {
            TransactionTypeConstants.HANDOVER => "HOV",
            TransactionTypeConstants.TRANSFER => "TRF",
            TransactionTypeConstants.LOAN => "LON",
            TransactionTypeConstants.RETURN => "RTN",
            TransactionTypeConstants.LOAN_RETURN => "LRN",
            TransactionTypeConstants.MAINTENANCE => "MTN",
            TransactionTypeConstants.POST_MAINTENANCE => "PMT",
            TransactionTypeConstants.DISPOSAL => "DSP",
            TransactionTypeConstants.OPERATIONAL_OFFICE => "OPO",
            _ => "TXN"
        };

        var datePart = DateTime.Now.ToString("yyMMdd");
        var randomPart = Random.Shared.Next(1000, 9999).ToString();

        return $"{prefix}-{datePart}-{randomPart}";
    }

    private async Task AddAttachmentFromBase64Async(int transactionId, string base64String, string fileName, int order)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64String);

            var fileResult = await _fileAttachmentService.UploadBytesAsync(
                referenceTable: "AssetTransaction",
                referenceId: transactionId,
                originalFileName: fileName,
                bytes: bytes,
                fileCategory: "AssetTransaction",
                description: $"Attachment: {fileName}",
                isPrimary: false);

            if (!fileResult.IsSuccess || fileResult.Data == null)
                throw new Exception($"Upload attachment failed: {string.Join(", ", fileResult.Errors)}");

            var attachmentEntity = new AssetTransactionAttachmentEntity
            {
                AssetTransactionId = transactionId,
                FileAttachmentId = fileResult.Data.FileAttachmentId,
                Order = order,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };
            await _transactionReps.InsertAttachmentAsync(attachmentEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding attachment for Transaction {TransactionId}", transactionId);
            throw;
        }
    }

    private async Task AddApproveAttachmentFromBase64Async(int transactionId, string base64String, string fileName, int documentType, int order)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64String);

            var fileResult = await _fileAttachmentService.UploadBytesAsync(
                referenceTable: "AssetTransactionApprove",
                referenceId: transactionId,
                originalFileName: fileName,
                bytes: bytes,
                fileCategory: "ApproveDocument",
                description: $"Approve document: {fileName}",
                isPrimary: false);

            if (!fileResult.IsSuccess || fileResult.Data == null)
                throw new Exception($"Upload approve attachment failed: {string.Join(", ", fileResult.Errors)}");

            var existingApproveAttachments = await _transactionReps.GetApproveAttachmentsByTransactionIdAsync(transactionId);
            foreach (var att in existingApproveAttachments.Where(a => a.DocumentType == documentType))
            {
                await _transactionReps.DeleteApproveAttachmentAsync(att.AssetTransactionApproveAttachmentId);
                await _fileAttachmentService.DeleteAsync(att.FileAttachmentId);
            }

            var attachmentEntity = new AssetTransactionApproveAttachmentEntity
            {
                AssetTransactionId = transactionId,
                FileAttachmentId = fileResult.Data.FileAttachmentId,
                Order = order,
                DocumentType = documentType,
                NeedToChange = false,
                Approved = false,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };
            await _transactionReps.InsertApproveAttachmentAsync(attachmentEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding approve attachment for Transaction {TransactionId}", transactionId);
            throw;
        }
    }
}