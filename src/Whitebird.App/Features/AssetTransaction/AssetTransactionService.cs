using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Department;
using Whitebird.App.Features.Employee;
using Whitebird.App.Features.FileAttachment;
using Whitebird.App.Features.MasterData;
using Whitebird.App.Features.Office;
using Whitebird.App.Features.Supplier;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.FileAttachment;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Supplier;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.AssetTransaction;

/// <summary>
/// Service implementation for Asset Transaction business logic
/// </summary>
public class AssetTransactionService : BaseService, IAssetTransactionService
{
    private readonly IGenericRepository<AssetTransactionEntity> _repository;
    private readonly IAssetTransactionReps _transactionReps;
    private readonly IAssetReps _assetReps;
    private readonly IEmployeeService _employeeService;
    private readonly IOfficeService _officeService;
    private readonly IDepartmentService _departmentService;
    private readonly ISupplierService _supplierService;
    private readonly IMasterDataService _masterDataService;
    private readonly IMasterDataLookupService _masterDataLookupService;
    private readonly IFileAttachmentService _fileAttachmentService;
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AssetTransactionService(
        IGenericRepository<AssetTransactionEntity> repository,
        IAssetTransactionReps transactionReps,
        IAssetReps assetReps,
        IEmployeeService employeeService,
        IOfficeService officeService,
        IDepartmentService departmentService,
        ISupplierService supplierService,
        IMasterDataService masterDataService,
        IMasterDataLookupService masterDataLookupService,
        IFileAttachmentService fileAttachmentService,
        IStorageService storageService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AssetTransactionService> logger) : base(logger)
    {
        _repository = repository;
        _transactionReps = transactionReps;
        _assetReps = assetReps;
        _employeeService = employeeService;
        _officeService = officeService;
        _departmentService = departmentService;
        _supplierService = supplierService;
        _masterDataService = masterDataService;
        _masterDataLookupService = masterDataLookupService;
        _fileAttachmentService = fileAttachmentService;
        _storageService = storageService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
        _httpContextAccessor = httpContextAccessor;
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
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {id} not found");
            }
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
        var addTypes = new[] {
            TransactionTypeConstants.HANDOVER,
            TransactionTypeConstants.TRANSFER,
            TransactionTypeConstants.LOAN,
            TransactionTypeConstants.MAINTENANCE,
            TransactionTypeConstants.DISPOSAL,
            TransactionTypeConstants.OPERATIONAL_OFFICE
        };

        if (!addTypes.Contains(model.TransactionType))
        {
            return ServiceResult<AssetTransactionDetailView>.BadRequest(
                $"Transaction type {model.TransactionType} cannot be created manually. Use shortcut endpoint.");
        }

        var validationResult = await ValidateTransactionAsync(model, null);
        if (validationResult != null)
        {
            return validationResult;
        }

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
                TableNames.AssetTransaction,
                id,
                $"Transaction created: Type {model.TransactionType}, Asset {model.AssetId}",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailView>.Success(created!, "Transaction created successfully");
        }, "create transaction");
    }

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
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Cannot update approved/rejected transaction");
            }

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            await _transactionReps.UpdateAsync(existing);

            var updated = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                id,
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
            if (existing == null)
            {
                return ServiceResult.NotFound($"Transaction with id {id} not found");
            }

            var paired = await _transactionReps.GetPairedTransactionAsync(id);
            if (paired != null)
            {
                return ServiceResult.BadRequest("Cannot delete transaction that has a paired transaction");
            }

            await _transactionReps.DeleteAsync(id);

            await _activityLogService.LogDeleteAsync(
                TableNames.AssetTransaction,
                id,
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
            if (existing == null)
            {
                return ServiceResult.NotFound($"Transaction with id {id} not found");
            }

            var paired = await _transactionReps.GetPairedTransactionAsync(id);
            if (paired != null)
            {
                return ServiceResult.BadRequest("Cannot soft delete transaction that has a paired transaction");
            }

            await _transactionReps.SoftDeleteAsync(id);

            await _activityLogService.LogSoftDeleteAsync(
                TableNames.AssetTransaction,
                id,
                $"Transaction soft deleted",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Transaction soft deleted successfully");
        }, "soft delete transaction");
    }

    // ============================================================
    // UPDATE WITH ATTACHMENTS (Standar)
    // ============================================================

    public async Task<ServiceResult<AssetTransactionDetailView>> UpdateWithAttachmentsAsync(
        int id, AssetTransactionUpdateFormDto model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {id} not found");
            }

            // Validate if can update
            if (existing.Approved != null && model.Approved == null)
            {
                // Can't un-approve if already approved/rejected
            }

            // Update basic fields
            existing.AssetId = model.AssetId;
            existing.TransactionType = model.TransactionType;
            existing.EmployeeId = model.EmployeeId;
            existing.OfficeId = model.OfficeId;
            existing.TransactionDate = model.TransactionDate;
            existing.ExpectedLoanReturnDate = model.ExpectedLoanReturnDate;
            existing.Notes = model.Notes;
            existing.Condition = model.Condition;
            existing.Approved = model.Approved;
            existing.ApprovedBy = model.ApprovedBy;
            existing.Completed = model.Completed;
            existing.CompletedBy = model.CompletedBy;
            existing.MaintenanceType = model.MaintenanceType;
            existing.MaintenanceCost = model.MaintenanceCost;
            existing.IsActive = model.IsActive;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            await _transactionReps.UpdateAsync(existing);

            // ============================================================
            // HANDLE ATTACHMENTS (base64)
            // ============================================================
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
                        {
                            await _transactionReps.DeleteAttachmentAsync(attEntity.AssetTransactionAttachmentId);
                        }
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

            // ============================================================
            // HANDLE APPROVE ATTACHMENTS
            // ============================================================
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
                        {
                            await _transactionReps.DeleteApproveAttachmentAsync(attEntity.AssetTransactionApproveAttachmentId);
                        }
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

            var updated = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                id,
                $"Transaction updated with attachments",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetTransactionDetailView>.Success(updated!, "Transaction updated successfully");
        }, "update transaction with attachments");
    }

    // ============================================================
    // UPDATE APPROVAL WITH ATTACHMENTS
    // ============================================================

    public async Task<ServiceResult<AssetTransactionDetailView>> UpdateApprovalWithAttachmentsAsync(
        int id, AssetTransactionUpdateApprovalFormDto model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _transactionReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Transaction with id {id} not found");
            }

            // Update basic fields
            existing.AssetId = model.AssetId;
            existing.TransactionType = model.TransactionType;
            existing.EmployeeId = model.EmployeeId;
            existing.OfficeId = model.OfficeId;
            existing.TransactionDate = model.TransactionDate;
            existing.ExpectedLoanReturnDate = model.ExpectedLoanReturnDate;
            existing.Notes = model.Notes;
            existing.Condition = model.Condition;
            existing.Approved = model.Approved;
            existing.ApprovedBy = model.ApprovedBy;
            existing.Completed = model.Completed;
            existing.CompletedBy = model.CompletedBy;
            existing.MaintenanceType = model.MaintenanceType;
            existing.MaintenanceCost = model.MaintenanceCost;
            existing.IsActive = model.IsActive;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            await _transactionReps.UpdateAsync(existing);

            // ============================================================
            // HANDLE APPROVE ATTACHMENTS
            // ============================================================
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
                        {
                            await _transactionReps.DeleteApproveAttachmentAsync(attEntity.AssetTransactionApproveAttachmentId);
                        }
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

            var updated = await _transactionReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                id,
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
        {
            return ServiceResult<int>.BadRequest("No transaction IDs provided");
        }

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
                }
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                0,
                $"{count} transaction(s) approved via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(count, $"{count} transaction(s) approved successfully");
        }, "bulk approved");
    }

    public async Task<ServiceResult<int>> BulkPendingAsync(BulkApprovalRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No transaction IDs provided");
        }

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
                }
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                0,
                $"{count} transaction(s) moved to pending via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(count, $"{count} transaction(s) moved to pending successfully");
        }, "bulk pending");
    }

    public async Task<ServiceResult<int>> BulkRejectedAsync(BulkApprovalRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No transaction IDs provided");
        }

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
                    transaction.ModifiedDate = DateTime.Now;
                    transaction.ModifiedBy = _currentUserService.GetDisplayName();
                    await _transactionReps.UpdateAsync(transaction);
                    count++;
                }
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                0,
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
            if (transaction == null)
            {
                return ServiceResult.NotFound($"Transaction with id {id} not found");
            }

            if (transaction.Approved != true)
            {
                return ServiceResult.BadRequest("Transaction must be approved before complete");
            }

            if (transaction.Completed == true)
            {
                return ServiceResult.BadRequest("Transaction already completed");
            }

            var allUploaded = await _transactionReps.IsAllRequiredDocumentsUploadedAsync(id);
            if (!allUploaded)
            {
                return ServiceResult.BadRequest("All required documents must be uploaded before complete");
            }

            var allApproved = await _transactionReps.IsAllRequiredDocumentsApprovedAsync(id);
            if (!allApproved)
            {
                return ServiceResult.BadRequest("All uploaded documents must be approved before complete");
            }

            transaction.Completed = true;
            transaction.CompletedBy = _currentUserService.GetDisplayName();
            transaction.ModifiedDate = DateTime.Now;
            transaction.ModifiedBy = _currentUserService.GetDisplayName();

            await _transactionReps.UpdateAsync(transaction);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                id,
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
            if (transaction == null)
            {
                return ServiceResult.NotFound($"Transaction with id {id} not found");
            }

            if (transaction.Completed != true)
            {
                return ServiceResult.BadRequest("Transaction is not completed");
            }

            transaction.Completed = null;
            transaction.CompletedBy = null;
            transaction.ModifiedDate = DateTime.Now;
            transaction.ModifiedBy = _currentUserService.GetDisplayName();

            await _transactionReps.UpdateAsync(transaction);

            await _activityLogService.LogUpdateAsync(
                TableNames.AssetTransaction,
                id,
                $"Transaction uncompleted (reverted to approved)",
                _currentUserService.GetDisplayName());

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

    // ============================================================
    // OUTSTANDING (VIEW ONLY)
    // ============================================================

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
            if (source == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Source transaction with id {sourceTransactionId} not found");
            }

            var returnableTypes = new[] {
                TransactionTypeConstants.HANDOVER,
                TransactionTypeConstants.LOAN,
                TransactionTypeConstants.OPERATIONAL_OFFICE
            };

            if (!returnableTypes.Contains(source.TransactionType))
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest(
                    $"Transaction type {source.TransactionType} cannot be returned. Allowed: HANDOVER, LOAN, OPERATIONAL OFFICE");
            }

            if (source.Approved != true)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction must be approved");
            }

            if (source.FromAssetTransactionId != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction already has a paired return");
            }

            int returnType;
            if (source.TransactionType == TransactionTypeConstants.LOAN)
            {
                returnType = TransactionTypeConstants.LOAN_RETURN;
            }
            else
            {
                returnType = TransactionTypeConstants.RETURN;
            }

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
                TableNames.AssetTransaction,
                id,
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
            if (source == null)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Source transaction with id {sourceTransactionId} not found");
            }

            if (source.TransactionType != TransactionTypeConstants.MAINTENANCE)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Only MAINTENANCE can create POST_MAINTENANCE");
            }

            if (source.Approved != true)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction must be approved");
            }

            if (source.FromAssetTransactionId != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction already has a paired post-maintenance");
            }

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
                TableNames.AssetTransaction,
                id,
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
        {
            return ServiceResult<IEnumerable<MasterDataDto>>.Failure("Failed to get transaction types");
        }

        var addTypes = new[] {
            TransactionTypeConstants.HANDOVER,
            TransactionTypeConstants.TRANSFER,
            TransactionTypeConstants.LOAN,
            TransactionTypeConstants.MAINTENANCE,
            TransactionTypeConstants.DISPOSAL,
            TransactionTypeConstants.OPERATIONAL_OFFICE
        };

        var filtered = result.Data.Where(d => addTypes.Contains(d.Code));
        return ServiceResult<IEnumerable<MasterDataDto>>.Success(filtered);
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypeReturnListAsync()
    {
        var result = await _masterDataService.GetTransactionTypesAsync();
        if (!result.IsSuccess || result.Data == null)
        {
            return ServiceResult<IEnumerable<MasterDataDto>>.Failure("Failed to get transaction types");
        }

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
        {
            return ServiceResult<IEnumerable<ApproveDocumentTypeDto>>.Failure("Failed to get approve document types");
        }

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
            var assets = await _assetReps.GetDropdownListByTransactionTypeAsync(transactionTypeId);
            var result = assets.Select(a => new AssetTransactionAssetDto
            {
                AssetId = a.AssetId,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName
            });
            return ServiceResult<IEnumerable<AssetTransactionAssetDto>>.Success(result);
        }, "get asset list");
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private async Task<ServiceResult<AssetTransactionDetailView>?> ValidateTransactionAsync(
        AssetTransactionCreateViewModel model, AssetTransactionEntity? existing)
    {
        var asset = await _assetReps.GetByIdRawAsync(model.AssetId);
        if (asset == null)
        {
            return ServiceResult<AssetTransactionDetailView>.NotFound($"Asset with id {model.AssetId} not found");
        }

        var typeName = await _masterDataLookupService.GetTransactionTypeNameAsync(model.TransactionType);
        if (typeName.Data == null)
        {
            return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid TransactionType: {model.TransactionType}");
        }

        var type = model.TransactionType;

        if (type == TransactionTypeConstants.HANDOVER && !model.EmployeeId.HasValue)
        {
            return ServiceResult<AssetTransactionDetailView>.BadRequest("HANDOVER requires EmployeeId");
        }

        if (type == TransactionTypeConstants.TRANSFER)
        {
            if (!model.EmployeeId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("TRANSFER requires EmployeeId (receiver)");
            }
            if (!model.FromAssetTransactionId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("TRANSFER requires FromAssetTransactionId");
            }
        }

        if (type == TransactionTypeConstants.LOAN)
        {
            if (!model.EmployeeId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("LOAN requires EmployeeId");
            }
            if (!model.ExpectedLoanReturnDate.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("LOAN requires ExpectedLoanReturnDate");
            }
            if (model.ExpectedLoanReturnDate.Value <= model.TransactionDate)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("ExpectedLoanReturnDate must be after TransactionDate");
            }
        }

        if (type == TransactionTypeConstants.MAINTENANCE)
        {
            if (!model.MaintenanceType.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("MAINTENANCE requires MaintenanceType");
            }
        }

        if (type == TransactionTypeConstants.OPERATIONAL_OFFICE)
        {
            if (!model.OfficeId.HasValue)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("OPERATIONAL OFFICE requires OfficeId");
            }
        }

        if (type == TransactionTypeConstants.LOAN_RETURN || type == TransactionTypeConstants.POST_MAINTENANCE)
        {
            return ServiceResult<AssetTransactionDetailView>.BadRequest($"Transaction type {type} cannot be created manually. Use shortcut endpoint.");
        }

        if (model.EmployeeId.HasValue)
        {
            var employee = await _employeeService.GetByIdAsync(model.EmployeeId.Value);
            if (!employee.IsSuccess || employee.Data == null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Employee with id {model.EmployeeId} not found");
            }
        }

        if (model.OfficeId.HasValue)
        {
            var office = await _officeService.GetByIdAsync(model.OfficeId.Value);
            if (!office.IsSuccess || office.Data == null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Office with id {model.OfficeId} not found");
            }
        }

        if (model.Condition.HasValue)
        {
            var condition = await _masterDataLookupService.GetAssetConditionNameAsync(model.Condition.Value);
            if (condition.Data == null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid Condition: {model.Condition}");
            }
        }

        if (model.MaintenanceType.HasValue)
        {
            var maintType = await _masterDataLookupService.GetMaintenanceTypeNameAsync(model.MaintenanceType.Value);
            if (maintType.Data == null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Invalid MaintenanceType: {model.MaintenanceType}");
            }
        }

        if (model.FromAssetTransactionId.HasValue)
        {
            var source = await _transactionReps.GetByIdRawAsync(model.FromAssetTransactionId.Value);
            if (source == null || source.IsActive == false)
            {
                return ServiceResult<AssetTransactionDetailView>.NotFound($"Source transaction {model.FromAssetTransactionId} not found");
            }
            if (source.Approved != true)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction must be approved");
            }
            if (source.FromAssetTransactionId != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest("Source transaction already has a paired transaction");
            }
        }

        if (!model.FromAssetTransactionId.HasValue)
        {
            var active = await _transactionReps.GetActiveTransactionByAssetIdAsync(model.AssetId);
            if (active != null)
            {
                return ServiceResult<AssetTransactionDetailView>.BadRequest($"Asset is currently in active transaction: {active.TransactionTypeName}");
            }
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
        var randomPart = new Random().Next(1000, 9999).ToString();

        return $"{prefix}-{datePart}-{randomPart}";
    }

    private async Task AddAttachmentFromBase64Async(int transactionId, string base64String, string fileName, int order)
    {
        var bytes = Convert.FromBase64String(base64String);
        using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(fileName)
        };

        var uploadModel = new FileAttachmentUploadDto
        {
            File = file,
            FileCategory = "AssetTransaction",
            Description = $"Attachment: {fileName}",
            IsPrimary = false
        };

        var fileResult = await _fileAttachmentService.UploadAsync("AssetTransaction", transactionId, uploadModel);
        if (fileResult.IsSuccess && fileResult.Data != null)
        {
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
    }

    private async Task AddApproveAttachmentFromBase64Async(int transactionId, string base64String, string fileName, int documentType, int order)
    {
        var bytes = Convert.FromBase64String(base64String);
        using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(fileName)
        };

        var uploadModel = new FileAttachmentUploadDto
        {
            File = file,
            FileCategory = "ApproveDocument",
            Description = $"Approve document: {fileName}",
            IsPrimary = false
        };

        var fileResult = await _fileAttachmentService.UploadAsync("AssetTransactionApprove", transactionId, uploadModel);
        if (fileResult.IsSuccess && fileResult.Data != null)
        {
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
    }

    private string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}