using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Category;

public class CategoryEntity : AuditableEntity
{
    public int CategoryId { get; set; }
    public string? CategoryCode { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }

    [NotMapped]
    public string? ParentCategoryName { get; set; }

    [NotMapped]
    public int ChildCount { get; set; }
}