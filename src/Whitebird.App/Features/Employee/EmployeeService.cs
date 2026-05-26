using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Department;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Domain.Features.Common;

namespace Whitebird.App.Features.Employee;

public class EmployeeService : BaseService, IEmployeeService
{
    private readonly IGenericRepository<EmployeeEntity> _repository;
    private readonly IEmployeeReps _employeeReps;
    private readonly IDepartmentReps _departmentReps;
    private readonly IOfficeReps _officeReps;
    private readonly IAssetReps _assetReps;
    private readonly IAssetTransactionReps _transactionReps;
    private readonly IMasterDataService _masterDataService;
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
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<EmployeeDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetByIdWithRelationsAsync(id);
            if (employee == null)
                return ServiceResult<EmployeeDetailViewModel>.NotFound($"Employee with id {id} not found");

            var viewModel = MapToDetailViewModel(employee);
            viewModel.ActiveAssetsCount = await _employeeReps.GetActiveAssetsCountAsync(id);
            return ServiceResult<EmployeeDetailViewModel>.Success(viewModel);
        }, "get employee by id");
    }

    public async Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetAllAsync();
            var viewModels = MapToViewModels(employees);
            return ServiceResult<IEnumerable<EmployeeListViewModel>>.Success(viewModels);
        }, "get all employees");
    }

    public async Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetByDepartmentAsync(int departmentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetByDepartmentIdAsync(departmentId);
            var viewModels = MapToViewModels(employees);
            return ServiceResult<IEnumerable<EmployeeListViewModel>>.Success(viewModels);
        }, "get employees by department");
    }

    public async Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetByStatusAsync(int employmentStatus)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employees = await _employeeReps.GetByEmploymentStatusAsync(employmentStatus);
            var viewModels = MapToViewModels(employees);
            return ServiceResult<IEnumerable<EmployeeListViewModel>>.Success(viewModels);
        }, "get employees by status");
    }

    public async Task<ServiceResult<EmployeeDetailViewModel>> CreateAsync(EmployeeCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FullName))
            return ServiceResult<EmployeeDetailViewModel>.BadRequest("Full name is required");

        if (string.IsNullOrWhiteSpace(model.EmployeeCode))
            return ServiceResult<EmployeeDetailViewModel>.BadRequest("Employee code is required");

        if (await _employeeReps.IsEmployeeCodeExistsAsync(model.EmployeeCode))
            return ServiceResult<EmployeeDetailViewModel>.Conflict($"Employee code '{model.EmployeeCode}' already exists");

        if (model.DepartmentId.HasValue && model.DepartmentId.Value > 0)
        {
            var department = await _departmentReps.GetByIdAsync(model.DepartmentId.Value);
            if (department == null)
                return ServiceResult<EmployeeDetailViewModel>.BadRequest($"Department with id {model.DepartmentId} does not exist");
        }

        if (model.OfficeId.HasValue && model.OfficeId.Value > 0)
        {
            var office = await _officeReps.GetByIdAsync(model.OfficeId.Value);
            if (office == null)
                return ServiceResult<EmployeeDetailViewModel>.BadRequest($"Office with id {model.OfficeId} does not exist");
        }

        if (model.Position.HasValue)
        {
            var positionExists = await _masterDataService.GetValueAsync("Position", model.Position.Value);
            if (!positionExists.IsSuccess || positionExists.Data == null)
                return ServiceResult<EmployeeDetailViewModel>.BadRequest($"Invalid Position value: {model.Position}");
        }

        if (model.EmploymentStatus.HasValue)
        {
            var statusExists = await _masterDataService.GetValueAsync("EmployeeStatus", model.EmploymentStatus.Value);
            if (!statusExists.IsSuccess || statusExists.Data == null)
                return ServiceResult<EmployeeDetailViewModel>.BadRequest($"Invalid EmploymentStatus value: {model.EmploymentStatus}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<EmployeeEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _employeeReps.GetByIdWithRelationsAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.Employee,
                    created.EmployeeId,
                    $"Employee '{created.EmployeeCode}' - '{created.FullName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<EmployeeDetailViewModel>.Failure("Failed to retrieve created employee")
                : ServiceResult<EmployeeDetailViewModel>.Success(MapToDetailViewModel(created), "Employee created successfully");
        }, "create employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Employee, 0, "Create Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<EmployeeDetailViewModel>> UpdateAsync(int id, EmployeeUpdateViewModel model)
    {
        if (await _employeeReps.IsEmployeeCodeExistsAsync(model.EmployeeCode, id))
            return ServiceResult<EmployeeDetailViewModel>.Conflict($"Employee code '{model.EmployeeCode}' already exists");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _employeeReps.GetByIdAsync(id);
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

            var updated = await _employeeReps.GetByIdWithRelationsAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.Employee,
                id,
                $"Employee updated: Code '{oldCode}', Name '{oldName}' -> '{model.FullName}', Status '{oldStatus}' -> '{model.EmploymentStatus}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<EmployeeDetailViewModel>.Success(MapToDetailViewModel(updated!), "Employee updated successfully");
        }, "update employee", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Employee, id, "Update Employee", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _employeeReps.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Employee with id {id} not found");

            var activeAssets = await _employeeReps.GetActiveAssetsCountAsync(id);
            if (activeAssets > 0)
                return ServiceResult.BadRequest($"Cannot delete employee with {activeAssets} active assets assigned");

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

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _employeeReps.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Employee with id {id} not found");

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

    // FIXED: Update GetGridDataAsync to use repository pagination
    public async Task<ServiceResult<PaginatedResult<EmployeeListViewModel>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _employeeReps.GetPagedWithRelationsAsync(page, pageSize, search, sortBy, sortDescending, filters);
            var viewModels = result.Data.Adapt<List<EmployeeListViewModel>>();

            return ServiceResult<PaginatedResult<EmployeeListViewModel>>.Success(new PaginatedResult<EmployeeListViewModel>
            {
                Data = viewModels,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                Filters = filters,
                SortBy = sortBy,
                SortDescending = sortDescending
            });
        }, "get employee grid data");
    }

    public async Task<ServiceResult<EmployeeAssetSummaryViewModel>> GetAssetSummaryAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetByIdWithRelationsAsync(employeeId);
            if (employee == null)
                return ServiceResult<EmployeeAssetSummaryViewModel>.NotFound($"Employee with id {employeeId} not found");

            var currentAssets = await _assetReps.GetByHolderWithRelationsAsync(employeeId);
            var allTransactions = await _transactionReps.GetEmployeeTransactionHistoryAsync(employeeId);

            var summary = new EmployeeAssetSummaryViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                FullName = employee.FullName,
                DepartmentName = employee.DepartmentName,
                EmploymentStatusName = employee.EmploymentStatusName,
                CurrentlyHeldAssets = currentAssets.Count(),
                AssetsOnLoan = await _employeeReps.GetAssetsOnLoanCountAsync(employeeId),
                OverdueLoans = await _employeeReps.GetOverdueLoansCountAsync(employeeId),
                TotalHistoricalAssets = await _employeeReps.GetTotalHistoricalAssetsAsync(employeeId),
                ReturnedAssets = await _employeeReps.GetReturnedAssetsCountAsync(employeeId),
                DamagedReturns = await _employeeReps.GetDamagedReturnsCountAsync(employeeId)
            };

            foreach (var asset in currentAssets)
            {
                var lastTxn = allTransactions
                    .Where(t => t.AssetId == asset.AssetId)
                    .OrderByDescending(t => t.TransactionDate)
                    .FirstOrDefault();

                summary.CurrentAssets.Add(new EmployeeAssetDetail
                {
                    AssetId = asset.AssetId,
                    AssetCode = asset.AssetCode,
                    AssetName = asset.AssetName,
                    CategoryName = asset.CategoryName ?? "Unknown",
                    Status = DeriveAssetStatusFromTransaction(lastTxn),
                    AssociationType = lastTxn?.TransactionType == 3 ? "On Loan" : "Assigned",
                    SinceDate = lastTxn?.TransactionDate ?? asset.CreatedDate,
                    ExpectedReturnDate = lastTxn?.ExpectedReturnDate,
                    IsOverdue = lastTxn?.TransactionType == 3 && lastTxn.ExpectedReturnDate.HasValue && lastTxn.ExpectedReturnDate.Value < DateTime.Now,
                    ConditionName = asset.AssetConditionName
                });
            }

            foreach (var txn in allTransactions.OrderByDescending(t => t.TransactionDate))
            {
                summary.AssetHistory.Add(new EmployeeAssetHistory
                {
                    AssetTransactionId = txn.AssetTransactionId,
                    AssetId = txn.AssetId,
                    AssetCode = txn.AssetCode ?? "Unknown",
                    AssetName = txn.AssetName ?? "Unknown",
                    TransactionTypeName = txn.TransactionTypeName ?? txn.TransactionType.ToString(),
                    TransactionDate = txn.TransactionDate,
                    FromEmployeeName = txn.FromEmployeeName,
                    ToEmployeeName = txn.ToEmployeeName,
                    ConditionAfterName = txn.ConditionAfterName,
                    Notes = txn.Notes
                });
            }

            return ServiceResult<EmployeeAssetSummaryViewModel>.Success(summary);
        }, "get employee asset summary");
    }

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
            return ServiceResult<int>.BadRequest("No employee IDs provided");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var activatedCount = 0;

            foreach (var id in request.Ids)
            {
                var employee = await _employeeReps.GetByIdAsync(id);
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

    #region Private Helpers

    private EmployeeDetailViewModel MapToDetailViewModel(EmployeeEntity entity)
    {
        var viewModel = entity.Adapt<EmployeeDetailViewModel>();

        viewModel.DepartmentName = entity.DepartmentName;
        viewModel.OfficeName = entity.OfficeName;
        viewModel.PositionName = entity.PositionName;
        viewModel.EmploymentStatusName = entity.EmploymentStatusName;

        return viewModel;
    }

    private IEnumerable<EmployeeListViewModel> MapToViewModels(IEnumerable<EmployeeEntity> entities)
    {
        var viewModels = entities.Adapt<List<EmployeeListViewModel>>();

        foreach (var vm in viewModels)
        {
            var entity = entities.FirstOrDefault(e => e.EmployeeId == vm.EmployeeId);
            if (entity != null)
            {
                vm.DepartmentName = entity.DepartmentName;
                vm.OfficeName = entity.OfficeName;
                vm.PositionName = entity.PositionName;
                vm.EmploymentStatusName = entity.EmploymentStatusName;
            }
        }

        return viewModels;
    }

    private string DeriveAssetStatusFromTransaction(AssetTransactionEntity? transaction)
    {
        if (transaction == null)
            return "Available";

        return transaction.TransactionType switch
        {
            1 or 2 => "Assigned",
            3 => "On Loan",
            6 => "In Maintenance",
            _ => "Available"
        };
    }

    #endregion
}