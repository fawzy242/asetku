using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Office;

/// <summary>
/// Grid request for Office
/// </summary>
public class OfficeGridRequest : GridRequestBase
{
    /// <summary>
    /// Filter by OfficeType
    /// </summary>
    public int? OfficeType { get; set; }

    /// <summary>
    /// Filter by ParentOfficeId
    /// </summary>
    public int? ParentOfficeId { get; set; }

    /// <summary>
    /// Filter by City
    /// </summary>
    public string? City { get; set; }
}