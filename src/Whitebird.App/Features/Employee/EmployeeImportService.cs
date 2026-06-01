using System.Data;
using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common.Import;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Department;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.Employee;

public class EmployeeImportService : ImportServiceBase<EmployeeImportDto, EmployeeEntity>
{
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
        ILogger<EmployeeImportService> logger)
        : base(repository, currentUserService, activityLogService, logger, TableNames.Employee, "Employee")
    {
        _employeeReps = employeeReps;
        _departmentReps = departmentReps;
        _officeReps = officeReps;
        _masterDataService = masterDataService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    protected override Dictionary<string, string> GetTemplateColumns() => _templateColumns;

    protected override async Task InitializeCachesAsync(Dictionary<string, object> cache)
    {
        var departmentDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var allDepartments = await _departmentReps.GetAllListViewAsync();
        foreach (var d in allDepartments)
        {
            departmentDict[d.DepartmentName] = d.DepartmentId;
        }
        cache["Departments"] = departmentDict;

        var officeDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var allOffices = await _officeReps.GetAllListViewAsync();
        foreach (var o in allOffices)
        {
            officeDict[o.OfficeName] = o.OfficeId;
        }
        cache["Offices"] = officeDict;

        var positionCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var positions = await _masterDataService.GetEmployeePositionsAsync();
        if (positions.IsSuccess && positions.Data != null)
        {
            foreach (var p in positions.Data)
                positionCache[p.Name] = p.Code;
        }
        cache["Positions"] = positionCache;

        var statusCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var statuses = await _masterDataService.GetEmployeeStatusesAsync();
        if (statuses.IsSuccess && statuses.Data != null)
        {
            foreach (var s in statuses.Data)
                statusCache[s.Name] = s.Code;
        }
        cache["Statuses"] = statusCache;
    }

    protected override async Task<EmployeeEntity?> ProcessRowAsync(
        DataRow row,
        int rowNumber,
        ImportResult result,
        Dictionary<string, object> cache,
        string createdBy)
    {
        var departmentDict = cache["Departments"] as Dictionary<string, int>;
        var officeDict = cache["Offices"] as Dictionary<string, int>;
        var positionCache = cache["Positions"] as Dictionary<string, int>;
        var statusCache = cache["Statuses"] as Dictionary<string, int>;

        var employeeCode = ExcelDataReader.GetString(row, "EmployeeCode");
        var fullName = ExcelDataReader.GetString(row, "FullName");
        var address = ExcelDataReader.GetString(row, "Address");
        var departmentName = ExcelDataReader.GetString(row, "DepartmentName");
        var positionName = ExcelDataReader.GetString(row, "PositionName");
        var employmentStatusName = ExcelDataReader.GetString(row, "EmploymentStatusName");
        var phoneNumber = ExcelDataReader.GetString(row, "PhoneNumber");
        var email = ExcelDataReader.GetString(row, "Email");
        var officeName = ExcelDataReader.GetString(row, "OfficeName");
        var joinDate = ExcelDataReader.GetNullableDateTime(row, "JoinDate");
        var resignDate = ExcelDataReader.GetNullableDateTime(row, "ResignDate");

        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            result.AddError(rowNumber, "EmployeeCode", "EmployeeCode is required");
            return null;
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            result.AddError(rowNumber, "FullName", "FullName is required");
            return null;
        }

        if (await _employeeReps.IsEmployeeCodeExistsAsync(employeeCode))
        {
            result.AddError(rowNumber, "EmployeeCode", $"EmployeeCode '{employeeCode}' already exists", employeeCode);
            return null;
        }

        int? departmentId = null;
        if (!string.IsNullOrWhiteSpace(departmentName) && departmentDict != null)
        {
            if (!departmentDict.TryGetValue(departmentName, out int deptId))
                result.AddError(rowNumber, "DepartmentName", $"Department '{departmentName}' not found", departmentName);
            else
                departmentId = deptId;
        }

        int? officeId = null;
        if (!string.IsNullOrWhiteSpace(officeName) && officeDict != null)
        {
            if (!officeDict.TryGetValue(officeName, out int offId))
                result.AddError(rowNumber, "OfficeName", $"Office '{officeName}' not found", officeName);
            else
                officeId = offId;
        }

        int? positionId = null;
        if (!string.IsNullOrWhiteSpace(positionName) && positionCache != null)
        {
            if (!positionCache.TryGetValue(positionName, out int posId))
                result.AddError(rowNumber, "PositionName", $"Invalid Position '{positionName}'", positionName);
            else
                positionId = posId;
        }

        int? statusId = null;
        if (!string.IsNullOrWhiteSpace(employmentStatusName) && statusCache != null)
        {
            if (!statusCache.TryGetValue(employmentStatusName, out int statId))
                result.AddError(rowNumber, "EmploymentStatusName", $"Invalid EmploymentStatus '{employmentStatusName}'", employmentStatusName);
            else
                statusId = statId;
        }

        if (!string.IsNullOrWhiteSpace(email) && !ExcelDataReader.IsValidEmail(email))
        {
            result.AddError(rowNumber, "Email", $"Invalid email format: {email}", email);
            return null;
        }

        if (result.Errors.Any(e => e.RowNumber == rowNumber))
            return null;

        return new EmployeeEntity
        {
            EmployeeCode = employeeCode,
            FullName = fullName,
            Address = address,
            DepartmentId = departmentId,
            Position = positionId,
            EmploymentStatus = statusId,
            PhoneNumber = phoneNumber,
            Email = email,
            OfficeId = officeId,
            JoinDate = joinDate,
            ResignDate = resignDate,
            IsActive = false,
            CreatedDate = DateTime.Now,
            CreatedBy = createdBy
        };
    }

    protected override async Task<bool> IsEntityUniqueAsync(EmployeeEntity entity, ImportResult result, int rowNumber)
    {
        var exists = await _employeeReps.IsEmployeeCodeExistsAsync(entity.EmployeeCode);
        if (exists)
        {
            result.AddError(rowNumber, "EmployeeCode", $"EmployeeCode '{entity.EmployeeCode}' already exists", entity.EmployeeCode);
            return false;
        }
        return true;
    }

    protected override string GetEntityIdentifier(EmployeeEntity entity) => entity.EmployeeCode;
}