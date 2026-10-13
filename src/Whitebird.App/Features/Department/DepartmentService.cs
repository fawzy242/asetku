using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Department;

namespace Whitebird.App.Features.Department;

/// <summary>
/// Service implementation for Department business logic
/// </summary>
public class DepartmentService : BaseService, IDepartmentService
{
    private readonly IGenericRepository<DepartmentEntity> _repository;
    private readonly IDepartmentReps _departmentReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public DepartmentService(
        IGenericRepository<DepartmentEntity> repository,
        IDepartmentReps departmentReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<DepartmentService> logger) : base(logger)
    {
        _repository = repository;
        _departmentReps = departmentReps;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    // ============================================================
    // BASIC CRUD
    // ============================================================

    public async Task<ServiceResult<DepartmentDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var department = await _departmentReps.GetDetailByIdAsync(id);
            if (department == null)
            {
                return ServiceResult<DepartmentDetailView>.NotFound($"Department with id {id} not found");
            }
            return ServiceResult<DepartmentDetailView>.Success(department);
        }, "get department by id");
    }

    public async Task<ServiceResult<IEnumerable<DepartmentListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var departments = await _departmentReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<DepartmentListView>>.Success(departments);
        }, "get all departments");
    }

    public async Task<ServiceResult<DepartmentDetailView>> CreateAsync(DepartmentCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.DepartmentName))
        {
            return ServiceResult<DepartmentDetailView>.BadRequest("Department name is required");
        }

        var nameExists = await _departmentReps.IsDepartmentNameExistsAsync(model.DepartmentName);
        if (nameExists)
        {
            return ServiceResult<DepartmentDetailView>.Conflict($"Department '{model.DepartmentName}' already exists");
        }

        if (!string.IsNullOrWhiteSpace(model.DepartmentCode))
        {
            var codeExists = await _departmentReps.IsDepartmentCodeExistsAsync(model.DepartmentCode);
            if (codeExists)
            {
                return ServiceResult<DepartmentDetailView>.Conflict($"Department code '{model.DepartmentCode}' already exists");
            }
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<DepartmentEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _departmentReps.GetDetailByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.Department,
                    created.DepartmentId,
                    $"Department '{created.DepartmentName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<DepartmentDetailView>.Failure("Failed to retrieve created department")
                : ServiceResult<DepartmentDetailView>.Success(created, "Department created successfully");
        }, "create department");
    }

    public async Task<ServiceResult<DepartmentDetailView>> UpdateAsync(int id, DepartmentUpdateViewModel model)
    {
        var nameExists = await _departmentReps.IsDepartmentNameExistsAsync(model.DepartmentName, id);
        if (nameExists)
        {
            return ServiceResult<DepartmentDetailView>.Conflict($"Department '{model.DepartmentName}' already exists");
        }

        if (!string.IsNullOrWhiteSpace(model.DepartmentCode))
        {
            var codeExists = await _departmentReps.IsDepartmentCodeExistsAsync(model.DepartmentCode, id);
            if (codeExists)
            {
                return ServiceResult<DepartmentDetailView>.Conflict($"Department code '{model.DepartmentCode}' already exists");
            }
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _departmentReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<DepartmentDetailView>.NotFound($"Department with id {id} not found");
            }

            var wasActive = existing.IsActive;
            var oldName = existing.DepartmentName;
            var oldCode = existing.DepartmentCode;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            // BR-01: if record was Active on entry, force Inactive after update.
            if (wasActive)
            {
                existing.IsActive = false;
            }

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<DepartmentDetailView>.Failure("Failed to update department");
            }

            var updated = await _departmentReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.Department,
                id,
                $"Department updated: Name '{oldName}' -> '{model.DepartmentName}', Code '{oldCode}' -> '{model.DepartmentCode}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<DepartmentDetailView>.Success(updated!, "Department updated successfully");
        }, "update department");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _departmentReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Department with id {id} not found");
            }

            var employeeCount = await _departmentReps.GetEmployeeCountAsync(id);
            if (employeeCount > 0)
            {
                return ServiceResult.BadRequest($"Cannot delete department with {employeeCount} employees assigned");
            }

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    TableNames.Department,
                    id,
                    $"Department '{existing.DepartmentName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete department")
                : ServiceResult.Success("Department deleted successfully");
        }, "delete department");
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _departmentReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Department with id {id} not found");
            }

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    TableNames.Department,
                    id,
                    $"Department '{existing.DepartmentName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete department")
                : ServiceResult.Success("Department soft deleted successfully");
        }, "soft delete department");
    }

    // ============================================================
    // GRID
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<DepartmentListView>>> GetGridDataAsync(DepartmentGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _departmentReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<DepartmentListView>>.Success(result);
        }, "get department grid data");
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    public async Task<ServiceResult<IEnumerable<DepartmentDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var departments = await _departmentReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<DepartmentDropdownView>>.Success(departments);
        }, "get department dropdown list");
    }

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No department IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var activatedCount = 0;

            foreach (var id in request.Ids)
            {
                var department = await _departmentReps.GetByIdRawAsync(id);
                if (department != null && !department.IsActive)
                {
                    department.IsActive = true;
                    department.ModifiedDate = DateTime.Now;
                    department.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(department);
                    activatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Department,
                        id,
                        $"Department '{department.DepartmentName}' activated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(activatedCount,
                $"{activatedCount} department(s) activated successfully");
        }, "bulk activate departments");
    }

    public async Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No department IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deactivatedCount = 0;

            foreach (var id in request.Ids)
            {
                var department = await _departmentReps.GetByIdRawAsync(id);
                if (department != null && department.IsActive)
                {
                    var employeeCount = await _departmentReps.GetEmployeeCountAsync(id);
                    if (employeeCount > 0)
                    {
                        continue;
                    }

                    department.IsActive = false;
                    department.ModifiedDate = DateTime.Now;
                    department.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(department);
                    deactivatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Department,
                        id,
                        $"Department '{department.DepartmentName}' deactivated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            var message = deactivatedCount > 0
                ? $"{deactivatedCount} department(s) deactivated successfully"
                : "No departments were deactivated. Departments with employees cannot be deactivated.";

            return ServiceResult<int>.Success(deactivatedCount, message);
        }, "bulk deactivate departments");
    }

    public async Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No department IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deletedCount = 0;

            foreach (var id in request.Ids)
            {
                var department = await _departmentReps.GetByIdRawAsync(id);
                if (department == null)
                {
                    continue;
                }

                // Reuse per-row business rule: refuse if it has employees assigned.
                var employeeCount = await _departmentReps.GetEmployeeCountAsync(id);
                if (employeeCount > 0)
                {
                    continue;
                }

                var result = await _repository.DeleteAsync(id);
                if (result > 0)
                {
                    deletedCount++;

                    await _activityLogService.LogDeleteAsync(
                        TableNames.Department,
                        id,
                        $"Department '{department.DepartmentName}' deleted via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            var message = deletedCount > 0
                ? $"{deletedCount} department(s) deleted successfully"
                : "No departments were deleted. Departments with employees cannot be deleted.";

            return ServiceResult<int>.Success(deletedCount, message);
        }, "bulk delete departments");
    }
}