namespace Whitebird.Domain.Features.Office;

public class OfficeListView
{
    public int OfficeId { get; set; }
    public string? OfficeCode { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public int? OfficeType { get; set; }
    public string? OfficeTypeName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int? ParentOfficeId { get; set; }
    public string? ParentOfficeName { get; set; }
    public int ChildCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }

    // Action flags untuk grid
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class OfficeDetailView
{
    public int OfficeId { get; set; }
    public string? OfficeCode { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public int? OfficeType { get; set; }
    public string? OfficeTypeName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int? ParentOfficeId { get; set; }
    public string? ParentOfficeName { get; set; }
    public int ChildCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class OfficeDropdownView
{
    public int OfficeId { get; set; }
    public string OfficeName { get; set; } = string.Empty;
    public string? OfficeCode { get; set; }
    public int? ParentOfficeId { get; set; }
}