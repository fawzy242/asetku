using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Department;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;

namespace Whitebird.App.Features.Employee;

/// <summary>
/// Service implementation for Employee business logic
/// </summary>
public class EmployeeService : BaseService, IEmployeeService
{
    private readonly IGenericRepository<EmployeeEntity> _repository;
    private readonly IEmployeeReps _employeeReps;
    private readonly IDepartmentReps _departmentReps;
    private readonly IOfficeReps _officeReps;
    private readonly IAssetReps _assetReps;
    private readonly IAssetTransactionReps _transactionReps;
    private readonly IMasterDataService _masterDataService;
    private readonly IMasterDataLookupService _masterDataLookupService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public EmployeeService(
        IGenericRepository<EmployeeEntity> repository,
        IEmployeeReps employeeReps,
        IDepartmentReps departmentReps,
        IOfficeReps officeReps,
        IAssetReps assetReps,
        IAssetTransactionReps transactionReps,
        IMasterDataService masterDataService,
        IMasterDataLookupService masterDataLookupService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<EmployeeService> logger) : base(logger)
    {
        _repository = repository;
        _employeeReps = employeeReps;
        _departmentReps = departmentReps;
        _officeReps = officeReps;
        _assetReps = assetReps;
        _transactionReps = transactionReps;
        _masterDataService = masterDataService;
        _masterDataLookupService = masterDataLookupService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<EmployeeDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetDetailByIdAsync(id);
            if (employee == null)
            {
                return ServiceResult<EmployeeDetailView>.NotFound($"Employee with id {id} not found");
            }

            employee.ActiveAssetsCount = await _employeeReps.GetActiveAssetsCountAsync(id);
            return ServiceResult<EmployeeDetailView>.Success(employee);
        }, "get employee by id");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<EmployeeListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<EmployeeListView>>.Success(employees);
        }, "get all employees");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<EmployeeListView>>> GetByDepartmentAsync(int departmentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var department = await _departmentReps.GetByIdRawAsync(departmentId);
            if (department == null)
            {
                return ServiceResult<IEnumerable<EmployeeListView>>.NotFound($"Department with id {departmentId} not found");
            }

            var employees = await _employeeReps.GetByDepartmentIdListViewAsync(departmentId);
            return ServiceResult<IEnumerable<EmployeeListView>>.Success(employees);
        }, "get employees by department");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<EmployeeListView>>> GetByStatusAsync(int employmentStatus)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var statusExists = await _masterDataLookupService.GetEmployeeStatusNameAsync(employmentStatus);
            if (!statusExists.IsSuccess || statusExists.Data == null)
            {
                return ServiceResult<IEnumerable<EmployeeListView>>.BadRequest($"Invalid employment status: {employmentStatus}");
            }

            var employees = await _employeeReps.GetByEmploymentStatusListViewAsync(employmentStatus);
            return ServiceResult<IEnumerable<EmployeeListView>>.Success(employees);
        }, "get employees by status");
    }

    /// <inheritdoc />
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
            var department = await _departmentReps.GetByIdRawAsync(model.DepartmentId.Value);
            if (department == null)
            {
                return ServiceResult<EmployeeDetailView>.BadRequest($"Department with id {model.DepartmentId} does not exist");
            }
        }

        if (model.OfficeId.HasValue && model.OfficeId.Value > 0)
        {
            var office = await _officeReps.GetByIdRawAsync(model.OfficeId.Value);
            if (office == null)
            {
                return ServiceResult<EmployeeDetailView>.BadRequest($"Office with id {model.OfficeId} does not exist");
            }
        }

        if (model.Position.HasValue)
        {
            var positionExists = await _masterDataLookupService.GetPositionNameAsync(model.Position.Value);
            if (!positionExists.IsSuccess || positionExists.Data == null)
            {
                return ServiceResult<EmployeeDetailView>.BadRequest($"Invalid Position value: {model.Position}");
            }
        }

        if (model.EmploymentStatus.HasValue)
        {
            var statusExists = await _masterDataLookupService.GetEmployeeStatusNameAsync(model.EmploymentStatus.Value);
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
        }, "create employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Employee, 0, "Create Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<EmployeeDetailView>> UpdateAsync(int id, EmployeeUpdateViewModel model)
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

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<EmployeeDetailView>.Failure("Failed to update employee");
            }

            var updated = await _employeeReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.Employee,
                id,
                $"Employee updated: Code '{oldCode}', Name '{oldName}' -> '{model.FullName}', Status '{oldStatus}' -> '{model.EmploymentStatus}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<EmployeeDetailView>.Success(updated!, "Employee updated successfully");
        }, "update employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Employee, id, "Update Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _employeeReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Employee with id {id} not found");
            }

            var activeAssets = await _employeeReps.GetActiveAssetsCountAsync(id);
            if (activeAssets > 0)
            {
                return ServiceResult.BadRequest($"Cannot delete employee with {activeAssets} active assets assigned");
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
        }, "delete employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Employee, id, "Delete Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
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
        }, "soft delete employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Employee, id, "Soft Delete Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaginatedResult<EmployeeListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeReps.GetPagedListAsync(page, pageSize, search, sortBy, sortDescending, filters);
            return ServiceResult<PaginatedResult<EmployeeListView>>.Success(result);
        }, "get employee grid data");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<EmployeeAssetSummaryViewModel>> GetAssetSummaryAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var summary = await _employeeReps.GetAssetSummaryByEmployeeIdAsync(employeeId);
            if (summary == null)
            {
                return ServiceResult<EmployeeAssetSummaryViewModel>.NotFound($"Employee with id {employeeId} not found");
            }

            var result = summary.Adapt<EmployeeAssetSummaryViewModel>();
            return ServiceResult<EmployeeAssetSummaryViewModel>.Success(result);
        }, "get employee asset summary");
    }

    /// <inheritdoc />
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
                if (employee != null && employee.IsActive != request.Activate)
                {
                    employee.IsActive = request.Activate;
                    employee.ModifiedDate = DateTime.Now;
                    employee.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(employee);
                    activatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Employee,
                        id,
                        $"Employee '{employee.EmployeeCode}' {(request.Activate ? "activated" : "deactivated")} via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(activatedCount,
                $"{activatedCount} employee(s) {(request.Activate ? "activated" : "deactivated")} successfully");
        }, "bulk activate employees", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Employee, 0, "Bulk Activate Employees", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<EmployeeDropdownView>>.Success(employees);
        }, "get employee dropdown list");
    }
}