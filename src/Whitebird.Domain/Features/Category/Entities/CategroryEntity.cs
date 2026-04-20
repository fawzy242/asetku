using Whitebird.Domain.Features.Common.Entities;
using Whitebird.Domain.Features.Common.Attributes;

namespace Whitebird.Domain.Features.Category.Entities;

public class CategoryEntity : AuditableEntity
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }

    // Navigation properties - marked as NotMapped
    [NotMapped]
    public string? ParentCategoryName { get; set; }

    [NotMapped]
    public int ChildCount { get; set; }
}