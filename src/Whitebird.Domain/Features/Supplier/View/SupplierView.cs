using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Supplier.View;

public class SupplierListViewModel
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = default!;
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public int AssetCount { get; set; }
}

public class SupplierDetailViewModel
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = default!;
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public int AssetCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
}

public class SupplierCreateViewModel
{
    [Required(ErrorMessage = "SupplierName is required")]
    [StringLength(100, ErrorMessage = "SupplierName cannot exceed 100 characters")]
    public string SupplierName { get; set; } = default!;

    [StringLength(100, ErrorMessage = "ContactPerson cannot exceed 100 characters")]
    public string? ContactPerson { get; set; }

    [StringLength(20, ErrorMessage = "PhoneNumber cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
}

public class SupplierUpdateViewModel
{
    [Required(ErrorMessage = "SupplierName is required")]
    [StringLength(100, ErrorMessage = "SupplierName cannot exceed 100 characters")]
    public string SupplierName { get; set; } = default!;

    [StringLength(100, ErrorMessage = "ContactPerson cannot exceed 100 characters")]
    public string? ContactPerson { get; set; }

    [StringLength(20, ErrorMessage = "PhoneNumber cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}