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
    // UPDATE WITH ATTACHMENTS
    // ============================================================

    /// <summary>
    /// Update transaction with attachments (base64) - for standard grid data.
    /// Approval / completion flags are NOT accepted from this DTO; they are
    /// managed exclusively through the dedicated endpoints (BulkApproved/
    /// BulkPending/BulkRejected/Complete/Uncomplete/document-action).
    /// After the update, the backend re-evaluates the transaction Completed state.
    /// </summary>
    Task<ServiceResult<AssetTransactionDetailView>> UpdateWithAttachmentsAsync(int id, AssetTransactionUpdateFormDto model);

    /// <summary>
    /// Update transaction approval with attachments (base64) - for pending approval grid data.
    /// Accepts Approved / ApprovedBy (approval-flow endpoint), but NOT Completed /
    /// CompletedBy — completion is derived from the document state.
    /// After the update, the backend re-evaluates the transaction Completed state.
    /// </summary>
    Task<ServiceResult<AssetTransactionDetailView>> UpdateApprovalWithAttachmentsAsync(int id, AssetTransactionUpdateApprovalFormDto model);

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
    // COMPLETE TRANSACTIONS
    // ============================================================

    Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetCompleteTransactionsAsync(AssetTransactionGridRequest request);

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
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypeAddListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypeReturnListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMaintenanceTypeListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetConditionListAsync();
    Task<ServiceResult<IEnumerable<ApproveDocumentTypeDto>>> GetApproveDocumentTypeListAsync();
    Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetEmployeeListAsync();
    Task<ServiceResult<IEnumerable<AssetTransactionAssetDto>>> GetAssetListAsync(int transactionTypeId);

    // ============================================================
    // ATTACHMENTS (standalone retrieval)
    // ============================================================

    Task<ServiceResult<IEnumerable<AssetTransactionAttachmentView>>> GetAttachmentsAsync(int transactionId);
    Task<ServiceResult<IEnumerable<AssetTransactionApproveAttachmentView>>> GetApproveAttachmentsAsync(int transactionId);

    /// <summary>
    /// Streams the raw bytes of an attachment belonging to the given transaction.
    /// Verifies ownership before delegating to the internal FileAttachmentService.
    /// </summary>
    Task<ServiceResult<FileContentDto>> GetAttachmentFileAsync(int transactionId, int fileAttachmentId);

    /// <summary>
    /// Streams the raw bytes of an approve attachment belonging to the given transaction.
    /// Verifies ownership before delegating to the internal FileAttachmentService.
    /// </summary>
    Task<ServiceResult<FileContentDto>> GetApproveAttachmentFileAsync(int transactionId, int fileAttachmentId);

    // ============================================================
    // APPROVAL DOCUMENT WORKFLOW
    // ============================================================

    /// <summary>
    /// Returns the required approval-document checklist for the transaction,
    /// derived from MasterData.ReferenceName = 'ApproveDocumentType' joined
    /// with the transaction's current approval attachments.
    /// </summary>
    Task<ServiceResult<IEnumerable<RequiredDocumentTypeDto>>> GetRequiredDocumentsAsync(int transactionId);

    /// <summary>
    /// Applies an action (approve / revision) to a single approval document.
    /// After the change, the transaction Completed flag is auto re-evaluated.
    /// </summary>
    Task<ServiceResult<AssetTransactionDetailView>> SetApproveAttachmentActionAsync(
        int transactionId, int attachmentId, ApproveAttachmentActionDto dto);
}