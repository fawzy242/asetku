namespace Whitebird.Domain.Features.Category;

/// <summary>
/// View model untuk Category dropdown (minimal data)
/// </summary>
public class CategoryDropdownView
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryCode { get; set; }
    public int? ParentCategoryId { get; set; }
}