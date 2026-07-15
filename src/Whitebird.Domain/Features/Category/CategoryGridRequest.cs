using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Category;

/// <summary>
/// Grid request for Category
/// </summary>
public class CategoryGridRequest : GridRequestBase
{
    /// <summary>
    /// Filter by ParentCategoryId
    /// </summary>
    public int? ParentCategoryId { get; set; }
}