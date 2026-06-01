using System;

namespace Whitebird.Domain.Features.Category;

/// <summary>
/// View model untuk Category grid/list (bukan Entity!)
/// Data dari JOIN: ParentCategory (self-join), ChildCount (subquery)
/// </summary>
public class CategoryListView
{
    public int CategoryId { get; set; }
    public string? CategoryCode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int ChildCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}