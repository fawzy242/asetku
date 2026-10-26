using Microsoft.AspNetCore.Http;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Employee;

/// <summary>
/// Service interface for Employee business logic
/// </summary>
public interface IEmployeeService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<EmployeeDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<EmployeeListView>>> GetAllAsync();
    Task<ServiceResult<EmployeeDetailView>> CreateAsync(EmployeeCreateViewModel model);
    Task<ServiceResult<EmployeeDetailView>> UpdateAsync(int id, EmployeeUpdateFormDto model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<EmployeeListView>>> GetGridDataAsync(EmployeeGridRequest request);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetDropdownListAsync();
    Task<ServiceResult<IEnumerable<OfficeDropdownView>>> GetOfficeListAsync();
    Task<ServiceResult<IEnumerable<DepartmentDropdownView>>> GetDepartmentListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMasterDataListAsync(string referenceName);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request);

    // ============================================================
    // PHOTO & ATTACHMENTS
    // ============================================================

    /// <summary>
    /// Gets profile photo bytes with the correct Content-Type.
    /// </summary>
    Task<ServiceResult<FileContentDto>> GetPhotoAsync(int employeeId);

    /// <summary>
    /// Uploads (or replaces) the profile photo from multipart form data.
    /// Returns the photo URL on success.
    /// </summary>
    Task<ServiceResult<string>> UploadPhotoAsync(int employeeId, IFormFile file);

    /// <summary>
    /// Soft-deletes the current profile photo (physical file is retained).
    /// </summary>
    Task<ServiceResult> DeletePhotoAsync(int employeeId);

    /// <summary>
    /// Gets attachment list for an employee.
    /// </summary>
    Task<ServiceResult<IEnumerable<EmployeeAttachmentViewDto>>> GetAttachmentsAsync(int employeeId);

    /// <summary>
    /// Uploads a standalone attachment.
    /// </summary>
    Task<ServiceResult<EmployeeAttachmentViewDto>> UploadAttachmentAsync(int employeeId, AttachmentUploadDto model);

    /// <summary>
    /// Soft-deletes a single attachment (physical file is retained).
    /// </summary>
    Task<ServiceResult> DeleteAttachmentAsync(int employeeId, int fileAttachmentId);

    /// <summary>
    /// Streams the raw bytes of an attachment belonging to the given employee.
    /// Verifies ownership before delegating to the internal FileAttachmentService.
    /// </summary>
    Task<ServiceResult<FileContentDto>> GetAttachmentFileAsync(int employeeId, int fileAttachmentId);
}