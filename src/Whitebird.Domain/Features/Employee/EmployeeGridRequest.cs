using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Employee;

/// <summary>
/// Grid request for Employee
/// </summary>
public class EmployeeGridRequest : GridRequestBase
{
    /// <summary>
    /// Filter by DepartmentId
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Filter by OfficeId
    /// </summary>
    public int? OfficeId { get; set; }

    /// <summary>
    /// Filter by EmploymentStatus
    /// </summary>
    public int? EmploymentStatus { get; set; }
}