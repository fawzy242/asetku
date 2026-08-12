using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Supplier;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.AssetTransaction;

/// <summary>
/// Service interface for Asset Transaction business logic
/// </summary>
public interface IAssetTransactionService
{
    // ============================================================
    // CRUD
    // ============================================================

    Task<ServiceResult<AssetTransactionDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetAllAsync();
    Task<ServiceResult<AssetTransactionDetailView>> CreateAsync(AssetTransactionCreateViewModel model);
    Task<ServiceResult<AssetTransactionDetailView>> UpdateAsync(int id, AssetTransactionUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // BULK APPROVAL
    // ============================================================

    Task<ServiceResult<int>> BulkApprovedAsync(BulkApprovalRequest request);
    Task<ServiceResult<int>> BulkPendingAsync(BulkApprovalRequest request);
    Task<ServiceResult<int>> BulkRejectedAsync(BulkApprovalRequest request);

    // ============================================================
    // COMPLETE / UNCOMPLETE
    // ============================================================

    Task<ServiceResult> CompleteAsync(int id);
    Task<ServiceResult> UncompleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetGridAddAsync(AssetTransactionGridRequest request);
    Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetGridReturnAsync(AssetTransactionGridRequest request);
    Task<ServiceResult<PaginatedResult<AssetTransactionPendingApprovalView>>> GetGridPendingApprovalAsync(AssetTransactionGridRequest request);

    // ============================================================
    // OUTSTANDING (VIEW ONLY)
    // ============================================================

    Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetActiveLoansAsync(AssetTransactionGridRequest request);
    Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetOverdueLoansAsync(AssetTransactionGridRequest request);
    Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetUnderMaintenanceAsync(AssetTransactionGridRequest request);

    // ============================================================
    // SHORTCUT
    // ============================================================

    Task<ServiceResult<AssetTransactionDetailView>> CreateReturnAsync(int sourceTransactionId, AssetReturnViewModel model);
    Task<ServiceResult<AssetTransactionDetailView>> CreatePostMaintenanceAsync(int sourceTransactionId, PostMaintenanceViewModel model);

    // ============================================================
    // ATTACHMENT (UMUM)
    // ============================================================

    Task<ServiceResult<AssetTransactionAttachmentView>> UploadAttachmentAsync(int transactionId, AssetTransactionAttachmentUploadDto model);
    Task<ServiceResult<IEnumerable<AssetTransactionAttachmentView>>> GetAttachmentsAsync(int transactionId);
    Task<ServiceResult> DeleteAttachmentAsync(int transactionId, int attachmentId);

    // ============================================================
    // APPROVE ATTACHMENT
    // ============================================================

    Task<ServiceResult<AssetTransactionApproveAttachmentView>> UploadApproveAttachmentAsync(int transactionId, AssetTransactionApproveAttachmentUploadDto model);
    Task<ServiceResult<IEnumerable<AssetTransactionApproveAttachmentView>>> GetApproveAttachmentsAsync(int transactionId);
    Task<ServiceResult> DeleteApproveAttachmentAsync(int transactionId, int attachmentId);
    Task<ServiceResult> ApproveAttachmentActionAsync(int transactionId, int attachmentId, ApproveAttachmentActionDto model);
    Task<ServiceResult<IEnumerable<RequiredDocumentTypeDto>>> GetRequiredDocumentTypesAsync(int transactionId);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypeAddListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypeReturnListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMaintenanceTypeListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetConditionListAsync();
    Task<ServiceResult<IEnumerable<ApproveDocumentTypeDto>>> GetApproveDocumentTypeListAsync();
    Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetEmployeeListAsync();
    Task<ServiceResult<IEnumerable<AssetTransactionAssetDto>>> GetAssetListAsync(int transactionTypeId);
}