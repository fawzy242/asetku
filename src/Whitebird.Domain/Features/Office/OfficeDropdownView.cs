namespace Whitebird.Domain.Features.Office;

/// <summary>
/// View model untuk Office dropdown (minimal data)
/// </summary>
public class OfficeDropdownView
{
    public int OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public string? OfficeCode { get; set; }
    public int? ParentOfficeId { get; set; }
}