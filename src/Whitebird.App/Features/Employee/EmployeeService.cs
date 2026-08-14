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
    private readonly IHttpContextAccessor _httpContextAccessor;

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
        IHttpContextAccessor httpContextAccessor,
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
        _httpContextAccessor = httpContextAccessor;
    }

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

    // ============================================================
    // FIX 1: UPDATE — Handle base64 file upload dengan benar
    // ============================================================
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

            var oldCode = existing.EmployeeCode;
            var oldName = existing.FullName;
            var oldStatus = existing.EmploymentStatus;

            // Map manual
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

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<EmployeeDetailView>.Failure("Failed to update employee");
            }

            // ============================================================
            // FIX 1: PHOTO PROFILE - PASTIKAN UPLOAD BERHASIL
            // ============================================================
            if (!string.IsNullOrEmpty(model.PhotoFileBase64))
            {
                try
                {
                    // Delete existing photo
                    var existingPhotos = await _fileAttachmentService.GetByReferenceAsync("EmployeePhotoProfile", id);
                    if (existingPhotos.IsSuccess && existingPhotos.Data != null && existingPhotos.Data.Any())
                    {
                        foreach (var photo in existingPhotos.Data)
                        {
                            await _fileAttachmentService.DeleteAsync(photo.FileAttachmentId);
                        }
                    }

                    // Convert base64 to file and upload
                    await AddPhotoProfileFromBase64Async(id, model.PhotoFileBase64, model.PhotoFileName ?? "photo.jpg");

                    _logger.LogInformation("Photo profile uploaded successfully for Employee {EmployeeId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload photo profile for Employee {EmployeeId}", id);
                    // Jangan gagalkan update hanya karena photo gagal upload
                }
            }

            // ============================================================
            // ATTACHMENTS (base64)
            // ============================================================
            if (model.ReplaceAttachments || (model.AttachmentFilesBase64 != null && model.AttachmentFilesBase64.Any()))
            {
                var existingAttachments = await _employeeReps.GetAttachmentsByEmployeeIdAsync(id);
                var keepIds = model.AttachmentIdsToKeep ?? new List<int>();

                foreach (var att in existingAttachments)
                {
                    if (!keepIds.Contains(att.FileAttachmentId))
                    {
                        await _fileAttachmentService.DeleteAsync(att.FileAttachmentId);
                        await _employeeReps.DeleteAttachmentAsync(att.EmployeeAttachmentId);
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
                $"Employee updated: Code '{oldCode}', Name '{oldName}' -> '{model.FullName}', Status '{oldStatus}' -> '{model.EmploymentStatus}'",
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

    public async Task<ServiceResult<PaginatedResult<EmployeeListView>>> GetGridDataAsync(EmployeeGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<EmployeeListView>>.Success(result);
        }, "get employee grid data");
    }

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

    public async Task<ServiceResult<byte[]>> GetPhotoProfileAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetByIdRawAsync(employeeId);
            if (employee == null)
            {
                return ServiceResult<byte[]>.NotFound($"Employee with id {employeeId} not found");
            }

            var existingPhotos = await _fileAttachmentService.GetByReferenceAsync("EmployeePhotoProfile", employeeId);
            if (!existingPhotos.IsSuccess || existingPhotos.Data == null || !existingPhotos.Data.Any())
            {
                return ServiceResult<byte[]>.NotFound("Profile photo not found");
            }

            var primaryPhoto = existingPhotos.Data.FirstOrDefault(p => p.IsPrimary) ?? existingPhotos.Data.First();

            var fileResult = await _fileAttachmentService.GetByIdAsync(primaryPhoto.FileAttachmentId);
            if (!fileResult.IsSuccess || fileResult.Data == null)
            {
                return ServiceResult<byte[]>.NotFound("Profile photo file not found");
            }

            var fileBytes = await _storageService.ReadFileAsync(fileResult.Data.FilePath);
            return ServiceResult<byte[]>.Success(fileBytes);
        }, "get employee photo profile");
    }

    public async Task<ServiceResult<IEnumerable<EmployeeAttachmentEntity>>> GetAttachmentsAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetByIdRawAsync(employeeId);
            if (employee == null)
            {
                return ServiceResult<IEnumerable<EmployeeAttachmentEntity>>.NotFound($"Employee with id {employeeId} not found");
            }

            var attachments = await _employeeReps.GetAttachmentsByEmployeeIdAsync(employeeId);
            return ServiceResult<IEnumerable<EmployeeAttachmentEntity>>.Success(attachments);
        }, "get employee attachments");
    }

    // ============================================================
    // PRIVATE METHODS FOR BASE64 UPLOAD
    // ============================================================

    private async Task AddPhotoProfileFromBase64Async(int employeeId, string base64String, string fileName)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64String);
            using var stream = new MemoryStream(bytes);
            var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = GetContentType(fileName)
            };

            var subDirectory = Path.Combine("Employee", employeeId.ToString(), "Profile");
            var savedPath = await _storageService.SaveFileAsync(file, subDirectory);
            var fileHash = _storageService.ComputeFileHash(file);

            var uploadModel = new FileAttachmentUploadDto
            {
                File = file,
                FileCategory = "ProfilePhoto",
                Description = $"Profile photo for Employee {employeeId}",
                IsPrimary = true
            };

            var result = await _fileAttachmentService.UploadAsync("EmployeePhotoProfile", employeeId, uploadModel);
            if (result.IsSuccess)
            {
                await _activityLogService.LogUpdateAsync(
                    TableNames.Employee,
                    employeeId,
                    $"Profile photo uploaded",
                    _currentUserService.GetDisplayName());
                _logger.LogInformation("Photo uploaded successfully for Employee {EmployeeId}", employeeId);
            }
            else
            {
                _logger.LogWarning("Failed to upload photo for Employee {EmployeeId}: {Error}", employeeId, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for Employee {EmployeeId}", employeeId);
            throw;
        }
    }

    private async Task AddAttachmentFromBase64Async(int employeeId, string base64String, string fileName, int order)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64String);
            using var stream = new MemoryStream(bytes);
            var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = GetContentType(fileName)
            };

            var subDirectory = Path.Combine("Employee", employeeId.ToString(), "Attachments");
            var savedPath = await _storageService.SaveFileAsync(file, subDirectory);
            var fileHash = _storageService.ComputeFileHash(file);

            var uploadModel = new FileAttachmentUploadDto
            {
                File = file,
                FileCategory = "EmployeeAttachment",
                Description = $"Attachment for Employee {employeeId}",
                IsPrimary = false
            };

            var fileResult = await _fileAttachmentService.UploadAsync("Employee", employeeId, uploadModel);
            if (fileResult.IsSuccess && fileResult.Data != null)
            {
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding attachment for Employee {EmployeeId}", employeeId);
            throw;
        }
    }

    private string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}