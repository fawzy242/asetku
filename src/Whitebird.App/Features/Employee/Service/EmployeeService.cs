using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Employee.Interfaces;
using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.Employee.Entities;
using Whitebird.Domain.Features.Employee.View;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Employee.Service;

public class EmployeeService : BaseService, IEmployeeService
{
    private readonly IGenericRepository<EmployeeEntity> _repository;
    private readonly IEmployeeReps _employeeReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public EmployeeService(
        IGenericRepository<EmployeeEntity> repository,
        IEmployeeReps employeeReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<EmployeeService> logger) : base(logger)
    {
        _repository = repository;
        _employeeReps = employeeReps;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<EmployeeDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetByIdAsync(id);
            if (employee == null)
                return ServiceResult<EmployeeDetailViewModel>.NotFound($"Employee with id {id} not found");

            var viewModel = employee.Adapt<EmployeeDetailViewModel>();
            viewModel.ActiveAssetsCount = await _employeeReps.GetActiveAssetsCountAsync(id);
            return ServiceResult<EmployeeDetailViewModel>.Success(viewModel);
        }, "get employee by id");
    }

    public async Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetAllAsync();
            return ServiceResult<IEnumerable<EmployeeListViewModel>>.Success(employees.Adapt<IEnumerable<EmployeeListViewModel>>());
        }, "get all employees");
    }

    public async Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetByDepartmentAsync(string department)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetByDepartmentAsync(department);
            return ServiceResult<IEnumerable<EmployeeListViewModel>>.Success(employees.Adapt<IEnumerable<EmployeeListViewModel>>());
        }, "get employees by department");
    }

    public async Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetByStatusAsync(string status)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetByStatusAsync(status);
            return ServiceResult<IEnumerable<EmployeeListViewModel>>.Success(employees.Adapt<IEnumerable<EmployeeListViewModel>>());
        }, "get employees by status");
    }

    public async Task<ServiceResult<EmployeeDetailViewModel>> CreateAsync(EmployeeCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FullName))
            return ServiceResult<EmployeeDetailViewModel>.BadRequest("Full name is required");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<EmployeeEntity>();
            entity.EmployeeCode = await _employeeReps.GenerateEmployeeCodeAsync();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _employeeReps.GetByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    "Employee",
                    created.EmployeeId,
                    $"Employee '{created.EmployeeCode}' - '{created.FullName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<EmployeeDetailViewModel>.Failure("Failed to retrieve created employee")
                : ServiceResult<EmployeeDetailViewModel>.Success(created.Adapt<EmployeeDetailViewModel>(), "Employee created successfully");
        }, "create employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Employee", 0, "Create Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<EmployeeDetailViewModel>> UpdateAsync(int id, EmployeeUpdateViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult<EmployeeDetailViewModel>.NotFound($"Employee with id {id} not found");

            var oldCode = existing.EmployeeCode;
            var oldName = existing.FullName;
            var oldStatus = existing.EmploymentStatus;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
                return ServiceResult<EmployeeDetailViewModel>.Failure("Failed to update employee");

            var updated = await _employeeReps.GetByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                "Employee",
                id,
                $"Employee updated: Code '{oldCode}' -> '{existing.EmployeeCode}', Name '{oldName}' -> '{existing.FullName}', Status '{oldStatus}' -> '{existing.EmploymentStatus}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<EmployeeDetailViewModel>.Success(updated!.Adapt<EmployeeDetailViewModel>(), "Employee updated successfully");
        }, "update employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Employee", id, "Update Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Employee with id {id} not found");

            if (await _employeeReps.GetActiveAssetsCountAsync(id) > 0)
                return ServiceResult.BadRequest("Cannot delete employee with active assets assigned");

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    "Employee",
                    id,
                    $"Employee '{existing.EmployeeCode}' - '{existing.FullName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete employee")
                : ServiceResult.Success("Employee deleted successfully");
        }, "delete employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Employee", id, "Delete Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Employee with id {id} not found");

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    "Employee",
                    id,
                    $"Employee '{existing.EmployeeCode}' - '{existing.FullName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete employee")
                : ServiceResult.Success("Employee soft deleted successfully");
        }, "soft delete employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Employee", id, "Soft Delete Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<PaginatedResult<EmployeeListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetAllAsync();
            var query = employees.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.EmployeeCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    e.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (e.Email != null && e.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Department != null && e.Department.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            var totalCount = query.Count();
            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModels = pagedData.Adapt<List<EmployeeListViewModel>>();

            return ServiceResult<PaginatedResult<EmployeeListViewModel>>.Success(new PaginatedResult<EmployeeListViewModel>
            {
                Data = viewModels,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }, "get employee grid data");
    }
}