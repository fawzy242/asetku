using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Dashboard;

/// <summary>
/// Grid request for Dashboard detail stats (drilldown)
/// </summary>
public class DashboardDetailStatsRequest : GridRequestBase
{
    /// <summary>
    /// Type of detail stats: Available, Assigned, OnLoan, InMaintenance, Disposed, Damaged, ExpiredWarranty, OverdueLoans
    /// </summary>
    public string Type { get; set; } = string.Empty;
}