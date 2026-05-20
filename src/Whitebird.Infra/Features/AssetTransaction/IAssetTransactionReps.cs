using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTransaction;

public interface IAssetTransactionReps
{
    // Raw entity
    Task<AssetTransactionEntity?> GetByIdRawAsync(int transactionId);

    // With relations
    Task<AssetTransactionEntity?> GetByIdWithRelationsAsync(int transactionId);
    Task<IEnumerable<AssetTransactionEntity>> GetAllWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetByAssetIdWithRelationsAsync(int assetId);
    Task<IEnumerable<AssetTransactionEntity>> GetByEmployeeIdWithRelationsAsync(int employeeId);
    Task<IEnumerable<AssetTransactionEntity>> GetByApprovalStatusAsync(bool? approved);
    Task<IEnumerable<AssetTransactionEntity>> GetPendingApprovalsWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetByDateRangeWithRelationsAsync(DateTime startDate, DateTime endDate);
    Task<AssetTransactionEntity?> GetActiveTransactionByAssetIdAsync(int assetId);
    Task<int> GetTransactionCountByAssetAsync(int assetId);

    // Pagination
    Task<PaginatedResult<AssetTransactionEntity>> GetPagedWithRelationsAsync(int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null);

    // Pairing queries
    Task<AssetTransactionEntity?> GetPairedTransactionAsync(int transactionId);
    Task<bool> HasOpenPairedTransactionAsync(int assetId, int transactionType);
    Task<IEnumerable<AssetTransactionEntity>> GetActiveLoansWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetOverdueLoansWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetAssetTransactionHistoryAsync(int assetId);
    Task<IEnumerable<AssetTransactionEntity>> GetEmployeeTransactionHistoryAsync(int employeeId);
}