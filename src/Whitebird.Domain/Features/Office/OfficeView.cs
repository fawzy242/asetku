using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Office;

public class OfficeListViewModel
{
    public int OfficeId { get; set; }
    public string? OfficeCode { get; set; }
    public string OfficeName { get; set; } = default!;
    public int? OfficeType { get; set; }           // ← TAMBAHKAN
    public string? OfficeTypeName { get; set; }
    public string? City { get; set; }
    public int? ParentOfficeId { get; set; }
    public string? ParentOfficeName { get; set; }
    public bool IsActive { get; set; }
}

public class OfficeDetailViewModel
{
    public int OfficeId { get; set; }
    public string? OfficeCode { get; set; }
    public string OfficeName { get; set; } = default!;
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
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class OfficeCreateViewModel
{
    [StringLength(50, ErrorMessage = "OfficeCode cannot exceed 50 characters")]
    public string? OfficeCode { get; set; }

    [Required(ErrorMessage = "OfficeName is required")]
    [StringLength(100, ErrorMessage = "OfficeName cannot exceed 100 characters")]
    public string OfficeName { get; set; } = default!;

    public int? OfficeType { get; set; }

    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string? Phone { get; set; }

    public int? ParentOfficeId { get; set; }
}

public class OfficeUpdateViewModel
{
    [StringLength(50, ErrorMessage = "OfficeCode cannot exceed 50 characters")]
    public string? OfficeCode { get; set; }

    [Required(ErrorMessage = "OfficeName is required")]
    [StringLength(100, ErrorMessage = "OfficeName cannot exceed 100 characters")]
    public string OfficeName { get; set; } = default!;

    public int? OfficeType { get; set; }

    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string? Phone { get; set; }

    public int? ParentOfficeId { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}