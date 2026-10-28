using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Employee;

public interface IEmployeeReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================

    Task<EmployeeEntity?> GetByIdRawAsync(int employeeId);
    Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null);

    /// <summary>
    /// Counts AssetTransaction rows pointing at this employee (EmployeeId or
    /// MaintenanceEmployeeId). Hard-delete FK guard.
    /// </summary>
    Task<int> GetAssetTransactionCountAsync(int employeeId);

    /// <summary>
    /// Counts ScheduleHandover rows pointing at this employee. Hard-delete FK guard.
    /// </summary>
    Task<int> GetScheduleHandoverCountAsync(int employeeId);

    /// <summary>
    /// Counts ScheduleReturn rows pointing at this employee. Hard-delete FK guard.
    /// </summary>
    Task<int> GetScheduleReturnCountAsync(int employeeId);

    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================

    Task<EmployeeDetailView?> GetDetailByIdAsync(int employeeId);
    Task<IEnumerable<EmployeeListView>> GetAllListViewAsync();

    // ============================================================
    // PAGINATION
    // ============================================================

    Task<PaginatedResult<EmployeeListView>> GetPagedListAsync(EmployeeGridRequest request);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<IEnumerable<EmployeeDropdownView>> GetDropdownListAsync();

    // ============================================================
    // EMPLOYEE ATTACHMENT
    // ============================================================

    Task<int> InsertAttachmentAsync(EmployeeAttachmentEntity entity);
    Task<IEnumerable<EmployeeAttachmentEntity>> GetAttachmentsByEmployeeIdAsync(int employeeId);
    Task<EmployeeAttachmentEntity?> GetAttachmentByIdAsync(int attachmentId);
    Task<int> DeleteAttachmentAsync(int attachmentId);
    Task<int> DeleteAttachmentsByEmployeeIdAsync(int employeeId);

    // ============================================================
    // EMPLOYEE PHOTO PROFILE
    // ============================================================

    Task<int> InsertPhotoProfileAsync(EmployeePhotoProfileAttachmentEntity entity);
    Task<EmployeePhotoProfileAttachmentEntity?> GetPhotoProfileByEmployeeIdAsync(int employeeId);
    Task<int> DeletePhotoProfileByEmployeeIdAsync(int employeeId);
}