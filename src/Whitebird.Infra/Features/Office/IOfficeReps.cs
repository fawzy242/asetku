using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Office;

/// <summary>
/// Repository interface for Office operations
/// </summary>
public interface IOfficeReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================
    
    Task<OfficeEntity?> GetByIdRawAsync(int officeId);
    Task<bool> IsOfficeCodeExistsAsync(string officeCode, int? excludeOfficeId = null);
    Task<bool> IsOfficeNameExistsAsync(string officeName, int? excludeOfficeId = null);
    Task<int> GetChildCountAsync(int officeId);
    
    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================
    
    Task<OfficeDetailView?> GetDetailByIdAsync(int officeId);
    Task<IEnumerable<OfficeListView>> GetAllListViewAsync();

    // ============================================================
    // PAGINATION
    // ============================================================
    
    Task<PaginatedResult<OfficeListView>> GetPagedListAsync(OfficeGridRequest request);
    
    // ============================================================
    // DROPDOWN
    // ============================================================
    
    Task<IEnumerable<OfficeDropdownView>> GetDropdownListAsync();
}