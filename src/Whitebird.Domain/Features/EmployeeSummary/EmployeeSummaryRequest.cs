namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// Request model for employee summary grids
/// </summary>
public class EmployeeSummaryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}