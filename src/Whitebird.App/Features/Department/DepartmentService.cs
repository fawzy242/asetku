using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Department;

namespace Whitebird.App.Features.Department;

public class DepartmentService
    : CommonService<DepartmentEntity, DepartmentDetailView, DepartmentListView, DepartmentGridRequest, DepartmentCreateViewModel, DepartmentUpdateViewModel>,
      IDepartmentService
{
    private readonly IDepartmentReps _departmentReps;

    public DepartmentService(
        ICommonReps repository,
        IDepartmentReps departmentReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<DepartmentService> logger)
        : base(repository, currentUserService, activityLogService, logger)
    {
        _departmentReps = departmentReps;
    }

    protected override string EntityLabel => "Department";
    protected override string TableName => TableNames.Department;

    protected override string BulkDeactivateEmptyMessage =>
        "No departments were deactivated. Departments with employees cannot be deactivated.";
    protected override string BulkDeleteEmptyMessage =>
        "No departments were deleted. Departments with employees cannot be deleted.";

    protected override async Task<DepartmentDetailView?> LoadDetailAsync(int id) =>
        await _departmentReps.GetDetailByIdAsync(id);

    protected override async Task<IEnumerable<DepartmentListView>> LoadAllAsync() =>
        await _departmentReps.GetAllListViewAsync();

    protected override async Task<PaginatedResult<DepartmentListView>> LoadPageAsync(DepartmentGridRequest request) =>
        await _departmentReps.GetPagedListAsync(request);

    protected override async Task<ServiceResult<DepartmentDetailView>?> ValidateCreateAsync(DepartmentCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.DepartmentName))
            return ServiceResult<DepartmentDetailView>.BadRequest("Department name is required");

        if (await _departmentReps.IsDepartmentNameExistsAsync(model.DepartmentName))
            return ServiceResult<DepartmentDetailView>.Conflict($"Department '{model.DepartmentName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.DepartmentCode) &&
            await _departmentReps.IsDepartmentCodeExistsAsync(model.DepartmentCode))
            return ServiceResult<DepartmentDetailView>.Conflict($"Department code '{model.DepartmentCode}' already exists");

        return null;
    }

    protected override async Task<ServiceResult<DepartmentDetailView>?> ValidateUpdateAsync(int id, DepartmentUpdateViewModel model)
    {
        if (await _departmentReps.IsDepartmentNameExistsAsync(model.DepartmentName, id))
            return ServiceResult<DepartmentDetailView>.Conflict($"Department '{model.DepartmentName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.DepartmentCode) &&
            await _departmentReps.IsDepartmentCodeExistsAsync(model.DepartmentCode, id))
            return ServiceResult<DepartmentDetailView>.Conflict($"Department code '{model.DepartmentCode}' already exists");

        return null;
    }

    protected override void MapCreate(DepartmentCreateViewModel model, DepartmentEntity entity) => model.Adapt(entity);
    protected override void MapUpdate(DepartmentUpdateViewModel model, DepartmentEntity entity) => model.Adapt(entity);
    protected override string Describe(DepartmentEntity entity) => entity.DepartmentName;

    protected override async Task<(bool CanProceed, string? Reason)> CanDeleteAsync(int id)
    {
        var employeeCount = await _departmentReps.GetEmployeeCountAsync(id);
        return employeeCount > 0
            ? (false, $"Cannot delete department with {employeeCount} employees assigned")
            : (true, (string?)null);
    }

    protected override async Task<(bool CanProceed, string? Reason)> CanDeactivateAsync(int id)
    {
        var employeeCount = await _departmentReps.GetEmployeeCountAsync(id);
        return employeeCount > 0 ? (false, (string?)null) : (true, (string?)null);
    }

    public async Task<ServiceResult<IEnumerable<DepartmentDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var departments = await _departmentReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<DepartmentDropdownView>>.Success(departments);
        }, "get department dropdown list");
    }
}