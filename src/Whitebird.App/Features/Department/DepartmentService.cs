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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<DepartmentListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var departments = await _departmentReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<DepartmentListView>>.Success(departments);
        }, "get all departments");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<DepartmentListView>>> GetActiveOnlyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var departments = await _departmentReps.GetActiveOnlyListViewAsync();
            return ServiceResult<IEnumerable<DepartmentListView>>.Success(departments);
        }, "get active departments");
    }

    /// <inheritdoc />
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
        }, "create department", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Department, 0, "Create Department", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
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

            var oldName = existing.DepartmentName;
            var oldCode = existing.DepartmentCode;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

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
        }, "update department", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Department, id, "Update Department", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
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
        }, "delete department", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Department, id, "Delete Department", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
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
        }, "soft delete department", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Department, id, "Soft Delete Department", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaginatedResult<DepartmentListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, bool? isActive = null, Dictionary<string, object>? filters = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (filters == null)
            {
                filters = new Dictionary<string, object>();
            }
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                filters["search"] = search;
            }
            
            if (isActive.HasValue)
            {
                filters["isActive"] = isActive.Value;
            }

            var result = await _departmentReps.GetPagedListAsync(page, pageSize, search, "DepartmentName", false, filters);
            return ServiceResult<PaginatedResult<DepartmentListView>>.Success(result);
        }, "get department grid data");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<DepartmentDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var departments = await _departmentReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<DepartmentDropdownView>>.Success(departments);
        }, "get department dropdown list");
    }
}