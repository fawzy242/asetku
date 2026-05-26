using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTransaction;

public interface IAssetTransactionReps
{
    Task<AssetTransactionEntity?> GetByIdRawAsync(int transactionId);
    Task<AssetTransactionEntity?> GetByIdWithRelationsAsync(int transactionId);
    Task<IEnumerable<AssetTransactionEntity>> GetAllWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetByAssetIdWithRelationsAsync(int assetId);
    Task<IEnumerable<AssetTransactionEntity>> GetByEmployeeIdWithRelationsAsync(int employeeId);
    Task<IEnumerable<AssetTransactionEntity>> GetByApprovalStatusAsync(bool? approved);
    Task<IEnumerable<AssetTransactionEntity>> GetPendingApprovalsWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetApprovedWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetRejectedWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetActiveLoansWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetOverdueLoansWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetByDateRangeWithRelationsAsync(DateTime startDate, DateTime endDate);
    Task<AssetTransactionEntity?> GetActiveTransactionByAssetIdAsync(int assetId);
    Task<int> GetTransactionCountByAssetAsync(int assetId);
    Task<PaginatedResult<AssetTransactionEntity>> GetPagedWithRelationsAsync(int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null);
    Task<AssetTransactionEntity?> GetPairedTransactionAsync(int transactionId);
    Task<bool> HasOpenPairedTransactionAsync(int assetId, int transactionType);
    Task<IEnumerable<AssetTransactionEntity>> GetAssetTransactionHistoryAsync(int assetId);
    Task<IEnumerable<AssetTransactionEntity>> GetEmployeeTransactionHistoryAsync(int employeeId);
}