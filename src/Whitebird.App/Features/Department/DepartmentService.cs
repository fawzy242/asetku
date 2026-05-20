using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Department;
using Whitebird.Domain.Features.Department;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Department;

namespace Whitebird.App.Features.Department;

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

    public async Task<ServiceResult<DepartmentDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var department = await _departmentReps.GetByIdAsync(id);
            if (department == null)
                return ServiceResult<DepartmentDetailViewModel>.NotFound($"Department with id {id} not found");

            var viewModel = department.Adapt<DepartmentDetailViewModel>();
            viewModel.EmployeeCount = await _departmentReps.GetEmployeeCountAsync(id);
            return ServiceResult<DepartmentDetailViewModel>.Success(viewModel);
        }, "get department by id");
    }

    public async Task<ServiceResult<IEnumerable<DepartmentListViewModel>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var departments = await _departmentReps.GetAllAsync();
            var viewModels = departments.Adapt<List<DepartmentListViewModel>>();

            foreach (var vm in viewModels)
            {
                vm.EmployeeCount = await _departmentReps.GetEmployeeCountAsync(vm.DepartmentId);
            }

            return ServiceResult<IEnumerable<DepartmentListViewModel>>.Success(viewModels);
        }, "get all departments");
    }

    public async Task<ServiceResult<IEnumerable<DepartmentListViewModel>>> GetActiveOnlyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var departments = await _departmentReps.GetActiveOnlyAsync();
            var viewModels = departments.Adapt<List<DepartmentListViewModel>>();

            foreach (var vm in viewModels)
            {
                vm.EmployeeCount = await _departmentReps.GetEmployeeCountAsync(vm.DepartmentId);
            }

            return ServiceResult<IEnumerable<DepartmentListViewModel>>.Success(viewModels);
        }, "get active departments");
    }

    public async Task<ServiceResult<DepartmentDetailViewModel>> CreateAsync(DepartmentCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.DepartmentName))
            return ServiceResult<DepartmentDetailViewModel>.BadRequest("Department name is required");

        if (await _departmentReps.IsDepartmentNameExistsAsync(model.DepartmentName))
            return ServiceResult<DepartmentDetailViewModel>.Conflict($"Department '{model.DepartmentName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.DepartmentCode) &&
            await _departmentReps.IsDepartmentCodeExistsAsync(model.DepartmentCode))
            return ServiceResult<DepartmentDetailViewModel>.Conflict($"Department code '{model.DepartmentCode}' already exists");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<DepartmentEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _departmentReps.GetByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    "Department",
                    created.DepartmentId,
                    $"Department '{created.DepartmentName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<DepartmentDetailViewModel>.Failure("Failed to retrieve created department")
                : ServiceResult<DepartmentDetailViewModel>.Success(created.Adapt<DepartmentDetailViewModel>(), "Department created successfully");
        }, "create department", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Department", 0, "Create Department", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<DepartmentDetailViewModel>> UpdateAsync(int id, DepartmentUpdateViewModel model)
    {
        if (await _departmentReps.IsDepartmentNameExistsAsync(model.DepartmentName, id))
            return ServiceResult<DepartmentDetailViewModel>.Conflict($"Department '{model.DepartmentName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.DepartmentCode) &&
            await _departmentReps.IsDepartmentCodeExistsAsync(model.DepartmentCode, id))
            return ServiceResult<DepartmentDetailViewModel>.Conflict($"Department code '{model.DepartmentCode}' already exists");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _departmentReps.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult<DepartmentDetailViewModel>.NotFound($"Department with id {id} not found");

            var oldName = existing.DepartmentName;
            var oldCode = existing.DepartmentCode;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
                return ServiceResult<DepartmentDetailViewModel>.Failure("Failed to update department");

            var updated = await _departmentReps.GetByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                "Department",
                id,
                $"Department updated: Name '{oldName}' -> '{model.DepartmentName}', Code '{oldCode}' -> '{model.DepartmentCode}'",
                _currentUserService.GetDisplayName());

            var viewModel = updated!.Adapt<DepartmentDetailViewModel>();
            viewModel.EmployeeCount = await _departmentReps.GetEmployeeCountAsync(id);

            return ServiceResult<DepartmentDetailViewModel>.Success(viewModel, "Department updated successfully");
        }, "update department", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Department", id, "Update Department", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _departmentReps.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Department with id {id} not found");

            var employeeCount = await _departmentReps.GetEmployeeCountAsync(id);
            if (employeeCount > 0)
                return ServiceResult.BadRequest($"Cannot delete department with {employeeCount} employees assigned");

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    "Department",
                    id,
                    $"Department '{existing.DepartmentName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete department")
                : ServiceResult.Success("Department deleted successfully");
        }, "delete department", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Department", id, "Delete Department", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _departmentReps.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Department with id {id} not found");

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    "Department",
                    id,
                    $"Department '{existing.DepartmentName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete department")
                : ServiceResult.Success("Department soft deleted successfully");
        }, "soft delete department", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Department", id, "Soft Delete Department", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<PaginatedResult<DepartmentListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var departments = await _departmentReps.GetAllAsync();
            var query = departments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    (d.DepartmentCode != null && d.DepartmentCode.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    d.DepartmentName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (d.Description != null && d.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            var totalCount = query.Count();
            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModels = pagedData.Adapt<List<DepartmentListViewModel>>();

            foreach (var vm in viewModels)
            {
                vm.EmployeeCount = await _departmentReps.GetEmployeeCountAsync(vm.DepartmentId);
            }

            return ServiceResult<PaginatedResult<DepartmentListViewModel>>.Success(new PaginatedResult<DepartmentListViewModel>
            {
                Data = viewModels,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }, "get department grid data");
    }
}