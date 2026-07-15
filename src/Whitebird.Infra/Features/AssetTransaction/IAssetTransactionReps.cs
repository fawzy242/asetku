using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTransaction;

public interface IAssetTransactionReps
{
    // ============================================================
    // ENTITY
    // ============================================================

    Task<AssetTransactionEntity?> GetByIdRawAsync(int id);
    Task<int> GetTransactionCountByAssetAsync(int assetId);
    Task<bool> HasOpenPairedTransactionAsync(int assetId, int transactionType);

    // ============================================================
    // DETAIL VIEW
    // ============================================================

    Task<AssetTransactionDetailView?> GetDetailByIdAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<PaginatedResult<AssetTransactionListView>> GetGridAddAsync(AssetTransactionGridRequest request);
    Task<PaginatedResult<AssetTransactionListView>> GetGridReturnAsync(AssetTransactionGridRequest request);
    Task<PaginatedResult<AssetTransactionPendingApprovalView>> GetGridPendingApprovalAsync(AssetTransactionGridRequest request);

    // ============================================================
    // OUTSTANDING (VIEW ONLY)
    // ============================================================

    Task<PaginatedResult<AssetTransactionListView>> GetActiveLoansAsync(AssetTransactionGridRequest request);
    Task<PaginatedResult<AssetTransactionListView>> GetOverdueLoansAsync(AssetTransactionGridRequest request);
    Task<PaginatedResult<AssetTransactionListView>> GetUnderMaintenanceAsync(AssetTransactionGridRequest request);

    // ============================================================
    // SHORTCUT / PAIRING
    // ============================================================

    Task<AssetTransactionListView?> GetActiveTransactionByAssetIdAsync(int assetId);
    Task<AssetTransactionEntity?> GetPairedTransactionAsync(int transactionId);

    // ============================================================
    // CRUD
    // ============================================================

    Task<int> InsertAsync(AssetTransactionEntity entity);
    Task<int> UpdateAsync(AssetTransactionEntity entity);
    Task<int> DeleteAsync(int id);
    Task<int> SoftDeleteAsync(int id);

    // ============================================================
    // ATTACHMENT (UMUM)
    // ============================================================

    Task<int> InsertAttachmentAsync(AssetTransactionAttachmentEntity entity);
    Task<IEnumerable<AssetTransactionAttachmentView>> GetAttachmentsByTransactionIdAsync(int transactionId);
    Task<AssetTransactionAttachmentEntity?> GetAttachmentByIdAsync(int attachmentId);
    Task<int> DeleteAttachmentAsync(int attachmentId);
    Task<int> DeleteAttachmentsByTransactionIdAsync(int transactionId);

    // ============================================================
    // APPROVE ATTACHMENT
    // ============================================================

    Task<int> InsertApproveAttachmentAsync(AssetTransactionApproveAttachmentEntity entity);
    Task<IEnumerable<AssetTransactionApproveAttachmentView>> GetApproveAttachmentsByTransactionIdAsync(int transactionId);
    Task<AssetTransactionApproveAttachmentEntity?> GetApproveAttachmentByIdAsync(int attachmentId);
    Task<int> UpdateApproveAttachmentAsync(AssetTransactionApproveAttachmentEntity entity);
    Task<int> DeleteApproveAttachmentAsync(int attachmentId);
    Task<int> DeleteApproveAttachmentsByTransactionIdAsync(int transactionId);
    Task<IEnumerable<RequiredDocumentTypeDto>> GetRequiredDocumentTypesAsync(int transactionId);

    // ============================================================
    // VALIDATION
    // ============================================================

    Task<bool> IsAllRequiredDocumentsUploadedAsync(int transactionId);
    Task<bool> IsAllRequiredDocumentsApprovedAsync(int transactionId);
}