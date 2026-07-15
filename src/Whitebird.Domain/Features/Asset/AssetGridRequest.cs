using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Asset;

/// <summary>
/// Grid request for Asset
/// </summary>
public class AssetGridRequest : GridRequestBase
{
    /// <summary>
    /// Filter by status (Available, Assigned, On Loan, In Maintenance, Damaged, Active, Inactive)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by CategoryId
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Filter by OfficeId
    /// </summary>
    public int? OfficeId { get; set; }

    /// <summary>
    /// Filter by SupplierId
    /// </summary>
    public int? SupplierId { get; set; }
}