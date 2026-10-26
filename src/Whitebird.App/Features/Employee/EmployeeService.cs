using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Department;
using Whitebird.App.Features.FileAttachment;
using Whitebird.App.Features.MasterData;
using Whitebird.App.Features.Office;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Employee;

namespace Whitebird.App.Features.Employee;

/// <summary>
/// Service implementation for Employee business logic
/// </summary>
public class EmployeeService : BaseService, IEmployeeService
{
    private readonly IGenericRepository<EmployeeEntity> _repository;
    private readonly IEmployeeReps _employeeReps;
    private readonly IOfficeService _officeService;
    private readonly IDepartmentService _departmentService;
    private readonly IMasterDataService _masterDataService;
    private readonly IStorageService _storageService;
    private readonly IFileAttachmentService _fileAttachmentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public EmployeeService(
        IGenericRepository<EmployeeEntity> repository,
        IEmployeeReps employeeReps,
        IOfficeService officeService,
        IDepartmentService departmentService,
        IMasterDataService masterDataService,
        IStorageService storageService,
        IFileAttachmentService fileAttachmentService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<EmployeeService> logger) : base(logger)
    {
        _repository = repository;
        _employeeReps = employeeReps;
        _officeService = officeService;
        _departmentService = departmentService;
        _masterDataService = masterDataService;
        _storageService = storageService;
        _fileAttachmentService = fileAttachmentService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    // ============================================================
    // BASIC CRUD
    // ============================================================

    public async Task<ServiceResult<EmployeeDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetDetailByIdAsync(id);
            if (employee == null)
            {
                return ServiceResult<EmployeeDetailView>.NotFound($"Employee with id {id} not found");
            }
            return ServiceResult<EmployeeDetailView>.Success(employee);
        }, "get employee by id");
    }

    public async Task<ServiceResult<IEnumerable<EmployeeListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<EmployeeListView>>.Success(employees);
        }, "get all employees");
    }

    public async Task<ServiceResult<EmployeeDetailView>> CreateAsync(EmployeeCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            return ServiceResult<EmployeeDetailView>.BadRequest("Full name is required");
        }

        if (string.IsNullOrWhiteSpace(model.EmployeeCode))
        {
            return ServiceResult<EmployeeDetailView>.BadRequest("Employee code is required");
        }

        var codeExists = await _employeeReps.IsEmployeeCodeExistsAsync(model.EmployeeCode);
        if (codeExists)
        {
            return ServiceResult<EmployeeDetailView>.Conflict($"Employee code '{model.EmployeeCode}' already exists");
        }

        if (model.DepartmentId.HasValue && model.DepartmentId.Value > 0)
        {
            var department = await _departmentService.GetByIdAsync(model.DepartmentId.Value);
            if (!department.IsSuccess || department.Data == null)
            {
                return ServiceResult<EmployeeDetailView>.BadRequest($"Department with id {model.DepartmentId} does not exist");
            }
        }

        if (model.OfficeId.HasValue && model.OfficeId.Value > 0)
        {
            var office = await _officeService.GetByIdAsync(model.OfficeId.Value);
            if (!office.IsSuccess || office.Data == null)
            {
                return ServiceResult<EmployeeDetailView>.BadRequest($"Office with id {model.OfficeId} does not exist");
            }
        }

        if (model.Position.HasValue)
        {
            var positionExists = await _masterDataService.GetValueAsync("Position", model.Position.Value);
            if (!positionExists.IsSuccess || positionExists.Data == null)
            {
                return ServiceResult<EmployeeDetailView>.BadRequest($"Invalid Position value: {model.Position}");
            }
        }

        if (model.EmploymentStatus.HasValue)
        {
            var statusExists = await _masterDataService.GetValueAsync("EmployeeStatus", model.EmploymentStatus.Value);
            if (!statusExists.IsSuccess || statusExists.Data == null)
            {
                return ServiceResult<EmployeeDetailView>.BadRequest($"Invalid EmploymentStatus value: {model.EmploymentStatus}");
            }
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<EmployeeEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _employeeReps.GetDetailByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.Employee,
                    created.EmployeeId,
                    $"Employee '{created.EmployeeCode}' - '{created.FullName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<EmployeeDetailView>.Failure("Failed to retrieve created employee")
                : ServiceResult<EmployeeDetailView>.Success(created, "Employee created successfully");
        }, "create employee");
    }

    public async Task<ServiceResult<EmployeeDetailView>> UpdateAsync(int id, EmployeeUpdateFormDto model)
    {
        var codeExists = await _employeeReps.IsEmployeeCodeExistsAsync(model.EmployeeCode, id);
        if (codeExists)
        {
            return ServiceResult<EmployeeDetailView>.Conflict($"Employee code '{model.EmployeeCode}' already exists");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _employeeReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<EmployeeDetailView>.NotFound($"Employee with id {id} not found");
            }

            existing.EmployeeCode = model.EmployeeCode;
            existing.FullName = model.FullName;
            existing.Address = model.Address;
            existing.DepartmentId = model.DepartmentId;
            existing.Position = model.Position;
            existing.EmploymentStatus = model.EmploymentStatus;
            existing.PhoneNumber = model.PhoneNumber;
            existing.Email = model.Email;
            existing.OfficeId = model.OfficeId;
            existing.JoinDate = model.JoinDate;
            existing.ResignDate = model.ResignDate;
            existing.IsActive = model.IsActive;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            await _repository.UpdateAsync(existing);

            if (!string.IsNullOrEmpty(model.PhotoFileBase64))
            {
                var oldPhoto = await _employeeReps.GetPhotoProfileByEmployeeIdAsync(id);
                if (oldPhoto != null)
                {
                    await _employeeReps.DeletePhotoProfileByEmployeeIdAsync(id);
                    await _fileAttachmentService.DeleteAsync(oldPhoto.FileAttachmentId);
                }

                await AddPhotoProfileFromBase64Async(id, model.PhotoFileBase64, model.PhotoFileName ?? "photo.jpg");
            }

            if (model.ReplaceAttachments || (model.AttachmentFilesBase64 != null && model.AttachmentFilesBase64.Any()))
            {
                var existingAttachments = await _employeeReps.GetAttachmentsByEmployeeIdAsync(id);
                var keepIds = model.AttachmentIdsToKeep ?? new List<int>();

                foreach (var att in existingAttachments)
                {
                    if (!keepIds.Contains(att.FileAttachmentId))
                    {
                        await _employeeReps.DeleteAttachmentAsync(att.EmployeeAttachmentId);
                        await _fileAttachmentService.DeleteAsync(att.FileAttachmentId);
                    }
                }

                if (model.AttachmentFilesBase64 != null && model.AttachmentFilesBase64.Any())
                {
                    int order = existingAttachments.Count(a => keepIds.Contains(a.FileAttachmentId));
                    for (int i = 0; i < model.AttachmentFilesBase64.Count; i++)
                    {
                        var base64 = model.AttachmentFilesBase64[i];
                        var fileName = model.AttachmentFileNames?[i] ?? $"attachment_{i}.pdf";
                        if (!string.IsNullOrEmpty(base64))
                        {
                            await AddAttachmentFromBase64Async(id, base64, fileName, order);
                            order++;
                        }
                    }
                }
            }

            var updated = await _employeeReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.Employee,
                id,
                $"Employee updated: Code '{model.EmployeeCode}', Name '{model.FullName}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<EmployeeDetailView>.Success(updated!, "Employee updated successfully");
        }, "update employee");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _employeeReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Employee with id {id} not found");
            }

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    TableNames.Employee,
                    id,
                    $"Employee '{existing.EmployeeCode}' - '{existing.FullName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete employee")
                : ServiceResult.Success("Employee deleted successfully");
        }, "delete employee");
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _employeeReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Employee with id {id} not found");
            }

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    TableNames.Employee,
                    id,
                    $"Employee '{existing.EmployeeCode}' - '{existing.FullName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete employee")
                : ServiceResult.Success("Employee soft deleted successfully");
        }, "soft delete employee");
    }

    // ============================================================
    // GRID
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<EmployeeListView>>> GetGridDataAsync(EmployeeGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<EmployeeListView>>.Success(result);
        }, "get employee grid data");
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    public async Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<EmployeeDropdownView>>.Success(employees);
        }, "get employee dropdown list");
    }

    public async Task<ServiceResult<IEnumerable<OfficeDropdownView>>> GetOfficeListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            return await _officeService.GetDropdownListAsync();
        }, "get office list for employee form");
    }

    public async Task<ServiceResult<IEnumerable<DepartmentDropdownView>>> GetDepartmentListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            return await _departmentService.GetDropdownListAsync();
        }, "get department list for employee form");
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMasterDataListAsync(string referenceName)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(referenceName))
            {
                return ServiceResult<IEnumerable<MasterDataDto>>.BadRequest("referenceName is required");
            }

            var validReferences = new[] { "Position", "EmployeeStatus" };
            if (!validReferences.Contains(referenceName))
            {
                return ServiceResult<IEnumerable<MasterDataDto>>.BadRequest($"Invalid referenceName: {referenceName}. Valid: {string.Join(", ", validReferences)}");
            }

            return await _masterDataService.GetByReferenceNameAsync(referenceName);
        }, "get masterdata list for employee form");
    }

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No employee IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var activatedCount = 0;

            foreach (var id in request.Ids)
            {
                var employee = await _employeeReps.GetByIdRawAsync(id);
                if (employee != null && !employee.IsActive)
                {
                    employee.IsActive = true;
                    employee.ModifiedDate = DateTime.Now;
                    employee.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(employee);
                    activatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Employee,
                        id,
                        $"Employee '{employee.EmployeeCode}' activated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(activatedCount,
                $"{activatedCount} employee(s) activated successfully");
        }, "bulk activate employees");
    }

    public async Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No employee IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deactivatedCount = 0;

            foreach (var id in request.Ids)
            {
                var employee = await _employeeReps.GetByIdRawAsync(id);
                if (employee != null && employee.IsActive)
                {
                    employee.IsActive = false;
                    employee.ModifiedDate = DateTime.Now;
                    employee.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(employee);
                    deactivatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Employee,
                        id,
                        $"Employee '{employee.EmployeeCode}' deactivated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(deactivatedCount,
                $"{deactivatedCount} employee(s) deactivated successfully");
        }, "bulk deactivate employees");
    }

    public async Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No employee IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deletedCount = 0;

            foreach (var id in request.Ids)
            {
                var existing = await _employeeReps.GetByIdRawAsync(id);
                if (existing == null)
                {
                    continue;
                }

                var result = await _repository.DeleteAsync(id);
                if (result > 0)
                {
                    deletedCount++;

                    await _activityLogService.LogDeleteAsync(
                        TableNames.Employee,
                        id,
                        $"Employee '{existing.EmployeeCode}' - '{existing.FullName}' deleted via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            var message = deletedCount > 0
                ? $"{deletedCount} employee(s) deleted successfully"
                : "No employees were deleted.";

            return ServiceResult<int>.Success(deletedCount, message);
        }, "bulk delete employees");
    }

    // ============================================================
    // PHOTO
    // ============================================================

    public async Task<ServiceResult<FileContentDto>> GetPhotoAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetByIdRawAsync(employeeId);
            if (employee == null)
            {
                return ServiceResult<FileContentDto>.NotFound($"Employee with id {employeeId} not found");
            }

            var photo = await _employeeReps.GetPhotoProfileByEmployeeIdAsync(employeeId);
            if (photo == null)
            {
                return ServiceResult<FileContentDto>.NotFound("Profile photo not found");
            }

            var fileResult = await _fileAttachmentService.GetByIdAsync(photo.FileAttachmentId);
            if (!fileResult.IsSuccess || fileResult.Data == null)
            {
                return ServiceResult<FileContentDto>.NotFound("Profile photo file not found");
            }

            var fileBytes = await _storageService.ReadFileAsync(fileResult.Data.FilePath);
            var contentType = _storageService.GetMimeType(fileResult.Data.OriginalFileName);

            return ServiceResult<FileContentDto>.Success(new FileContentDto
            {
                Bytes = fileBytes,
                ContentType = contentType,
                FileName = fileResult.Data.OriginalFileName
            });
        }, "get employee photo");
    }

    public async Task<ServiceResult<string>> UploadPhotoAsync(int employeeId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return ServiceResult<string>.BadRequest("No file provided");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileSizeConstants.IsAllowedProfilePhotoExtension(extension))
        {
            return ServiceResult<string>.BadRequest(
                $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", FileSizeConstants.AllowedProfilePhotoExtensions)}");
        }

        if (file.Length > FileSizeConstants.MaxProfilePhotoBytes)
        {
            return ServiceResult<string>.BadRequest($"File size exceeds {FileSizeConstants.MaxProfilePhotoMB}MB limit");
        }

        var employee = await _employeeReps.GetByIdRawAsync(employeeId);
        if (employee == null)
        {
            return ServiceResult<string>.NotFound($"Employee with id {employeeId} not found");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            // Soft-delete the previous photo if present.
            var oldPhoto = await _employeeReps.GetPhotoProfileByEmployeeIdAsync(employeeId);
            if (oldPhoto != null)
            {
                await _employeeReps.DeletePhotoProfileByEmployeeIdAsync(employeeId);
                await _fileAttachmentService.DeleteAsync(oldPhoto.FileAttachmentId);
            }

            using var stream = file.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var uploadResult = await _fileAttachmentService.UploadBytesAsync(
                referenceTable: "EmployeePhotoProfile",
                referenceId: employeeId,
                originalFileName: file.FileName,
                bytes: bytes,
                fileCategory: "ProfilePhoto",
                description: $"Profile photo for Employee {employeeId}",
                isPrimary: true);

            if (!uploadResult.IsSuccess || uploadResult.Data == null)
            {
                return ServiceResult<string>.Failure(uploadResult.Errors);
            }

            var entity = new EmployeePhotoProfileAttachmentEntity
            {
                EmployeeId = employeeId,
                FileAttachmentId = uploadResult.Data.FileAttachmentId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };
            await _employeeReps.InsertPhotoProfileAsync(entity);

            await _activityLogService.LogUpdateAsync(
                TableNames.Employee,
                employeeId,
                $"Profile photo uploaded",
                _currentUserService.GetDisplayName());

            return ServiceResult<string>.Success(uploadResult.Data.FileUrl ?? string.Empty, "Profile photo uploaded successfully");
        }, "upload employee photo");
    }

    public async Task<ServiceResult> DeletePhotoAsync(int employeeId)
    {
        var employee = await _employeeReps.GetByIdRawAsync(employeeId);
        if (employee == null)
        {
            return ServiceResult.NotFound($"Employee with id {employeeId} not found");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var photo = await _employeeReps.GetPhotoProfileByEmployeeIdAsync(employeeId);
            if (photo == null)
            {
                return ServiceResult.Success("No profile photo to delete");
            }

            await _employeeReps.DeletePhotoProfileByEmployeeIdAsync(employeeId);
            await _fileAttachmentService.DeleteAsync(photo.FileAttachmentId);

            await _activityLogService.LogUpdateAsync(
                TableNames.Employee,
                employeeId,
                $"Profile photo deleted",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Profile photo deleted successfully");
        }, "delete employee photo");
    }

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    public async Task<ServiceResult<IEnumerable<EmployeeAttachmentViewDto>>> GetAttachmentsAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            // GetDetailByIdAsync already returns the attachment list (joined with FileAttachment).
            // No redundant repo call needed.
            var detail = await _employeeReps.GetDetailByIdAsync(employeeId);
            if (detail == null)
            {
                return ServiceResult<IEnumerable<EmployeeAttachmentViewDto>>.NotFound($"Employee with id {employeeId} not found");
            }

            return ServiceResult<IEnumerable<EmployeeAttachmentViewDto>>.Success(detail.Attachments);
        }, "get employee attachments");
    }

    public async Task<ServiceResult<EmployeeAttachmentViewDto>> UploadAttachmentAsync(int employeeId, AttachmentUploadDto model)
    {
        if (model.File == null || model.File.Length == 0)
        {
            return ServiceResult<EmployeeAttachmentViewDto>.BadRequest("No file provided");
        }

        var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
        if (!FileSizeConstants.IsAllowedAssetExtension(extension))
        {
            return ServiceResult<EmployeeAttachmentViewDto>.BadRequest(
                $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", FileSizeConstants.AllowedAssetExtensions)}");
        }

        if (model.File.Length > FileSizeConstants.MaxAssetAttachmentBytes)
        {
            return ServiceResult<EmployeeAttachmentViewDto>.BadRequest($"File size exceeds {FileSizeConstants.MaxAssetAttachmentMB}MB limit");
        }

        var employee = await _employeeReps.GetByIdRawAsync(employeeId);
        if (employee == null)
        {
            return ServiceResult<EmployeeAttachmentViewDto>.NotFound($"Employee with id {employeeId} not found");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            using var stream = model.File.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var uploadResult = await _fileAttachmentService.UploadBytesAsync(
                referenceTable: "Employee",
                referenceId: employeeId,
                originalFileName: model.File.FileName,
                bytes: bytes,
                fileCategory: model.FileCategory ?? "EmployeeAttachment",
                description: model.Description ?? $"Attachment for Employee {employeeId}",
                isPrimary: false);

            if (!uploadResult.IsSuccess || uploadResult.Data == null)
            {
                return ServiceResult<EmployeeAttachmentViewDto>.Failure(uploadResult.Errors);
            }

            var attachmentEntity = new EmployeeAttachmentEntity
            {
                EmployeeId = employeeId,
                FileAttachmentId = uploadResult.Data.FileAttachmentId,
                Order = model.Order,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };
            var attachmentId = await _employeeReps.InsertAttachmentAsync(attachmentEntity);

            await _activityLogService.LogCreateAsync(
                TableNames.Employee,
                employeeId,
                $"Attachment uploaded: {model.File.FileName}",
                _currentUserService.GetDisplayName());

            var dto = new EmployeeAttachmentViewDto
            {
                EmployeeAttachmentId = attachmentId,
                FileAttachmentId = uploadResult.Data.FileAttachmentId,
                FileName = uploadResult.Data.FileName,
                OriginalFileName = uploadResult.Data.OriginalFileName,
                FileUrl = uploadResult.Data.FileUrl,
                FileCategory = uploadResult.Data.FileCategory,
                IsPrimary = false,
                Order = model.Order,
                CreatedDate = DateTime.Now
            };

            return ServiceResult<EmployeeAttachmentViewDto>.Success(dto, "Attachment uploaded successfully");
        }, "upload employee attachment");
    }

    public async Task<ServiceResult> DeleteAttachmentAsync(int employeeId, int fileAttachmentId)
    {
        var employee = await _employeeReps.GetByIdRawAsync(employeeId);
        if (employee == null)
        {
            return ServiceResult.NotFound($"Employee with id {employeeId} not found");
        }

        var attachments = await _employeeReps.GetAttachmentsByEmployeeIdAsync(employeeId);
        var target = attachments.FirstOrDefault(a => a.FileAttachmentId == fileAttachmentId);
        if (target == null)
        {
            return ServiceResult.NotFound($"Attachment {fileAttachmentId} not found for employee {employeeId}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            await _employeeReps.DeleteAttachmentAsync(target.EmployeeAttachmentId);
            await _fileAttachmentService.DeleteAsync(fileAttachmentId);

            await _activityLogService.LogDeleteAsync(
                TableNames.Employee,
                employeeId,
                $"Attachment deleted: {fileAttachmentId}",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Attachment deleted successfully");
        }, "delete employee attachment");
    }

    public async Task<ServiceResult<FileContentDto>> GetAttachmentFileAsync(int employeeId, int fileAttachmentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetByIdRawAsync(employeeId);
            if (employee == null)
            {
                return ServiceResult<FileContentDto>.NotFound($"Employee with id {employeeId} not found");
            }

            // Ownership check: the attachment row must belong to this employee.
            // Prevents downloading arbitrary FileAttachment ids through a foreign
            // employee route.
            var attachments = await _employeeReps.GetAttachmentsByEmployeeIdAsync(employeeId);
            if (!attachments.Any(a => a.FileAttachmentId == fileAttachmentId))
            {
                return ServiceResult<FileContentDto>.NotFound($"Attachment {fileAttachmentId} not found for employee {employeeId}");
            }

            // Delegate to the internal FileAttachment service. Shared service, no HTTP
            // surface of its own — used by every menu that owns attachments.
            var downloadResult = await _fileAttachmentService.DownloadAsync(fileAttachmentId);
            if (!downloadResult.IsSuccess || downloadResult.Data == null)
            {
                return ServiceResult<FileContentDto>.NotFound($"Attachment file {fileAttachmentId} could not be retrieved");
            }

            return downloadResult;
        }, "get employee attachment file");
    }

    // ============================================================
    // PRIVATE HELPERS — BASE64 UPLOAD
    // ============================================================

    private async Task AddPhotoProfileFromBase64Async(int employeeId, string base64String, string fileName)
    {
        var bytes = Convert.FromBase64String(base64String);

        var result = await _fileAttachmentService.UploadBytesAsync(
            referenceTable: "EmployeePhotoProfile",
            referenceId: employeeId,
            originalFileName: fileName,
            bytes: bytes,
            fileCategory: "ProfilePhoto",
            description: $"Profile photo for Employee {employeeId}",
            isPrimary: true);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new Exception($"Upload profile photo failed: {string.Join(", ", result.Errors)}");
        }

        var entity = new EmployeePhotoProfileAttachmentEntity
        {
            EmployeeId = employeeId,
            FileAttachmentId = result.Data.FileAttachmentId,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedBy = _currentUserService.GetDisplayName()
        };
        await _employeeReps.InsertPhotoProfileAsync(entity);

        await _activityLogService.LogUpdateAsync(
            TableNames.Employee,
            employeeId,
            $"Profile photo uploaded",
            _currentUserService.GetDisplayName());
    }

    private async Task AddAttachmentFromBase64Async(int employeeId, string base64String, string fileName, int order)
    {
        var bytes = Convert.FromBase64String(base64String);

        var fileResult = await _fileAttachmentService.UploadBytesAsync(
            referenceTable: "Employee",
            referenceId: employeeId,
            originalFileName: fileName,
            bytes: bytes,
            fileCategory: "EmployeeAttachment",
            description: $"Attachment for Employee {employeeId}",
            isPrimary: false);

        if (!fileResult.IsSuccess || fileResult.Data == null)
        {
            throw new Exception($"Upload attachment failed: {string.Join(", ", fileResult.Errors)}");
        }

        var attachmentEntity = new EmployeeAttachmentEntity
        {
            EmployeeId = employeeId,
            FileAttachmentId = fileResult.Data.FileAttachmentId,
            Order = order,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedBy = _currentUserService.GetDisplayName()
        };

        await _employeeReps.InsertAttachmentAsync(attachmentEntity);

        await _activityLogService.LogCreateAsync(
            TableNames.Employee,
            employeeId,
            $"Attachment added: {fileName}",
            _currentUserService.GetDisplayName());
    }
}