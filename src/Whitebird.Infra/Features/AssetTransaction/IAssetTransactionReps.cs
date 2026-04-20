using Whitebird.Domain.Features.AssetTransaction.Entities;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTransaction;

public interface IAssetTransactionReps
{
    // Raw entity untuk CRUD operations
    Task<AssetTransactionEntity?> GetByIdRawAsync(int transactionId);

    // Methods with relations (navigation properties populated via JOIN)
    Task<AssetTransactionEntity?> GetByIdWithRelationsAsync(int transactionId);
    Task<IEnumerable<AssetTransactionEntity>> GetAllWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetByAssetIdWithRelationsAsync(int assetId);
    Task<IEnumerable<AssetTransactionEntity>> GetByEmployeeIdWithRelationsAsync(int employeeId);
    Task<IEnumerable<AssetTransactionEntity>> GetByStatusWithRelationsAsync(string status);
    Task<IEnumerable<AssetTransactionEntity>> GetPendingApprovalsWithRelationsAsync();
    Task<IEnumerable<AssetTransactionEntity>> GetByDateRangeWithRelationsAsync(DateTime startDate, DateTime endDate);
    Task<AssetTransactionEntity?> GetActiveTransactionByAssetIdWithRelationsAsync(int assetId);
    Task<int> GetTransactionCountByAssetAsync(int assetId);
    Task<PaginatedResult<AssetTransactionEntity>> GetPagedWithRelationsAsync(int page, int pageSize, string? search = null, string? status = null, int? assetId = null);
}