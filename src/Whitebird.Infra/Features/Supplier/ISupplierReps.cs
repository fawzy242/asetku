using Whitebird.Domain.Features.Supplier;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Supplier;

/// <summary>
/// Repository interface for Supplier operations
/// </summary>
public interface ISupplierReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================
    
    Task<SupplierEntity?> GetByIdRawAsync(int supplierId);
    Task<bool> IsSupplierNameExistsAsync(string supplierName, int? excludeSupplierId = null);
    Task<int> GetAssetCountAsync(int supplierId);
    
    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================
    
    Task<SupplierDetailView?> GetDetailByIdAsync(int supplierId);
    Task<IEnumerable<SupplierListView>> GetAllListViewAsync();

    // ============================================================
    // PAGINATION
    // ============================================================
    
    Task<PaginatedResult<SupplierListView>> GetPagedListAsync(SupplierGridRequest request);
    
    // ============================================================
    // DROPDOWN
    // ============================================================
    
    Task<IEnumerable<SupplierDropdownView>> GetDropdownListAsync();
}