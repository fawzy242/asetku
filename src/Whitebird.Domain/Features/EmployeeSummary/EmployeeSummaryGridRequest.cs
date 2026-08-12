using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// Grid request for Employee Summary
/// </summary>
public class EmployeeSummaryGridRequest : GridRequestBase
{
    /// <summary>
    /// Employee ID (required, passed in path)
    /// </summary>
    public int EmployeeId { get; set; }
}