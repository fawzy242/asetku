using System.Data;
using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common.Import;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Employee;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Department;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.Employee;

public class EmployeeImportService : BaseService, IImportService<EmployeeImportDto>
{
    private readonly IGenericRepository<EmployeeEntity> _repository;
    private readonly IEmployeeReps _employeeReps;
    private readonly IDepartmentReps _departmentReps;
    private readonly IOfficeReps _officeReps;
    private readonly IMasterDataService _masterDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    private readonly Dictionary<string, string> _templateColumns = new()
    {
        { "EmployeeCode", "Required. Unique employee code (max 50 chars)" },
        { "FullName", "Required. Employee full name (max 100 chars)" },
        { "Address", "Optional. Employee address" },
        { "DepartmentName", "Optional. Department name - will lookup" },
        { "PositionName", "Optional. Position name from MasterData (Director, Manager, etc)" },
        { "EmploymentStatusName", "Optional. Status name (Permanent Employee, Contract, etc)" },
        { "PhoneNumber", "Optional. Phone number" },
        { "Email", "Optional. Email address" },
        { "OfficeName", "Optional. Office name - will lookup" },
        { "JoinDate", "Optional. Format: YYYY-MM-DD" },
        { "ResignDate", "Optional. Format: YYYY-MM-DD" }
    };

    public EmployeeImportService(
        IGenericRepository<EmployeeEntity> repository,
        IEmployeeReps employeeReps,
        IDepartmentReps departmentReps,
        IOfficeReps officeReps,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<EmployeeImportService> logger) : base(logger)
    {
        _repository = repository;
        _employeeReps = employeeReps;
        _departmentReps = departmentReps;
        _officeReps = officeReps;
        _masterDataService = masterDataService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<ImportResult>> ImportFromExcelAsync(Stream fileStream, string? createdBy = null)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var result = new ImportResult();
            var employeesToInsert = new List<EmployeeEntity>();
            var createdByUser = createdBy ?? _currentUserService.GetDisplayName();

            var dataTable = await ExcelHelper.ReadExcelToDataTableAsync(fileStream, true);
            result.TotalRows = dataTable.Rows.Count;

            var departmentCache = new Dictionary<string, int>();
            var officeCache = new Dictionary<string, int>();
            var positionCache = new Dictionary<string, int>();
            var statusCache = new Dictionary<string, int>();

            var positions = await _masterDataService.GetEmployeePositionsAsync();
            if (positions.IsSuccess)
            {
                foreach (var p in positions.Data!)
                    positionCache[p.Name.ToLowerInvariant()] = p.Code;
            }

            var statuses = await _masterDataService.GetEmployeeStatusesAsync();
            if (statuses.IsSuccess)
            {
                foreach (var s in statuses.Data!)
                    statusCache[s.Name.ToLowerInvariant()] = s.Code;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                var rowNumber = i + 2;
                var dto = new EmployeeImportDto();

                try
                {
                    dto.EmployeeCode = GetString(row, "EmployeeCode");
                    dto.FullName = GetString(row, "FullName");
                    dto.Address = GetString(row, "Address");
                    dto.DepartmentName = GetString(row, "DepartmentName");
                    dto.PositionName = GetString(row, "PositionName");
                    dto.EmploymentStatusName = GetString(row, "EmploymentStatusName");
                    dto.PhoneNumber = GetString(row, "PhoneNumber");
                    dto.Email = GetString(row, "Email");
                    dto.OfficeName = GetString(row, "OfficeName");
                    dto.JoinDate = GetNullableDateTime(row, "JoinDate");
                    dto.ResignDate = GetNullableDateTime(row, "ResignDate");

                    if (string.IsNullOrWhiteSpace(dto.EmployeeCode))
                    {
                        result.AddError(rowNumber, "EmployeeCode", "EmployeeCode is required");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(dto.FullName))
                    {
                        result.AddError(rowNumber, "FullName", "FullName is required");
                        continue;
                    }

                    if (await _employeeReps.IsEmployeeCodeExistsAsync(dto.EmployeeCode))
                    {
                        result.AddError(rowNumber, "EmployeeCode", $"EmployeeCode '{dto.EmployeeCode}' already exists", dto.EmployeeCode);
                        continue;
                    }

                    int? departmentId = null;
                    if (!string.IsNullOrWhiteSpace(dto.DepartmentName))
                    {
                        if (!departmentCache.ContainsKey(dto.DepartmentName.ToLowerInvariant()))
                        {
                            var departments = await _departmentReps.GetAllAsync();
                            var dept = departments.FirstOrDefault(d =>
                                d.DepartmentName.Equals(dto.DepartmentName, StringComparison.OrdinalIgnoreCase));
                            if (dept != null)
                                departmentCache[dto.DepartmentName.ToLowerInvariant()] = dept.DepartmentId;
                            else
                                result.AddError(rowNumber, "DepartmentName", $"Department '{dto.DepartmentName}' not found", dto.DepartmentName);
                        }

                        if (departmentCache.ContainsKey(dto.DepartmentName.ToLowerInvariant()))
                            departmentId = departmentCache[dto.DepartmentName.ToLowerInvariant()];
                    }

                    int? officeId = null;
                    if (!string.IsNullOrWhiteSpace(dto.OfficeName))
                    {
                        if (!officeCache.ContainsKey(dto.OfficeName.ToLowerInvariant()))
                        {
                            var offices = await _officeReps.GetAllAsync();
                            var office = offices.FirstOrDefault(o =>
                                o.OfficeName.Equals(dto.OfficeName, StringComparison.OrdinalIgnoreCase));
                            if (office != null)
                                officeCache[dto.OfficeName.ToLowerInvariant()] = office.OfficeId;
                            else
                                result.AddError(rowNumber, "OfficeName", $"Office '{dto.OfficeName}' not found", dto.OfficeName);
                        }

                        if (officeCache.ContainsKey(dto.OfficeName.ToLowerInvariant()))
                            officeId = officeCache[dto.OfficeName.ToLowerInvariant()];
                    }

                    int? positionId = null;
                    if (!string.IsNullOrWhiteSpace(dto.PositionName))
                    {
                        var key = dto.PositionName.ToLowerInvariant();
                        if (positionCache.ContainsKey(key))
                            positionId = positionCache[key];
                        else
                            result.AddError(rowNumber, "PositionName", $"Invalid Position '{dto.PositionName}'", dto.PositionName);
                    }

                    int? statusId = null;
                    if (!string.IsNullOrWhiteSpace(dto.EmploymentStatusName))
                    {
                        var key = dto.EmploymentStatusName.ToLowerInvariant();
                        if (statusCache.ContainsKey(key))
                            statusId = statusCache[key];
                        else
                            result.AddError(rowNumber, "EmploymentStatusName", $"Invalid EmploymentStatus '{dto.EmploymentStatusName}'", dto.EmploymentStatusName);
                    }

                    if (!string.IsNullOrWhiteSpace(dto.Email) && !IsValidEmail(dto.Email))
                    {
                        result.AddError(rowNumber, "Email", $"Invalid email format: {dto.Email}", dto.Email);
                        continue;
                    }

                    if (result.Errors.Any(e => e.RowNumber == rowNumber))
                        continue;

                    var entity = new EmployeeEntity
                    {
                        EmployeeCode = dto.EmployeeCode,
                        FullName = dto.FullName,
                        Address = dto.Address,
                        DepartmentId = departmentId,
                        Position = positionId,
                        EmploymentStatus = statusId,
                        PhoneNumber = dto.PhoneNumber,
                        Email = dto.Email,
                        OfficeId = officeId,
                        JoinDate = dto.JoinDate,
                        ResignDate = dto.ResignDate,
                        IsActive = false,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdByUser
                    };

                    employeesToInsert.Add(entity);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.AddError(rowNumber, "General", $"Error processing row: {ex.Message}");
                    _logger.LogError(ex, "Error processing employee import row {RowNumber}", rowNumber);
                }
            }

            if (employeesToInsert.Any())
            {
                await _repository.BulkInsertAsync(employeesToInsert);

                foreach (var employee in employeesToInsert)
                {
                    await _activityLogService.LogCreateAsync(
                        "Employee",
                        employee.EmployeeId,
                        $"Employee '{employee.EmployeeCode}' imported (INACTIVE - needs activation)",
                        createdByUser);
                }
            }

            return ServiceResult<ImportResult>.Success(result,
                $"Import completed: {result.SuccessCount} successful, {result.ErrorCount} errors");
        }, "import employees", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Employee", 0, "Import Employees", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<byte[]>> GenerateTemplateAsync()
    {
        return await ExecuteSafelyAsync(() =>
        {
            var template = ExcelHelper.GenerateTemplate("Employees", _templateColumns,
                "Employee Import Template - Fill in the data below. Employees will be imported as INACTIVE and need manual activation.");
            return Task.FromResult(ServiceResult<byte[]>.Success(template));
        }, "generate employee import template");
    }

    public async Task<ServiceResult<ImportResult>> ValidateOnlyAsync(Stream fileStream)
    {
        var result = new ImportResult();
        return ServiceResult<ImportResult>.Success(result);
    }

    #region Helper Methods

    private string? GetString(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private DateTime? GetNullableDateTime(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return DateTime.TryParse(value, out var result) ? result : null;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}