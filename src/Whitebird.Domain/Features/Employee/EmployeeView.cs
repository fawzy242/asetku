using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Employee;

public class EmployeeListViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string? EmploymentStatusName { get; set; }
    public string? Email { get; set; }
    public string? OfficeName { get; set; }
    public bool IsActive { get; set; }
}

public class EmployeeDetailViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? Position { get; set; }
    public string? PositionName { get; set; }
    public int? EmploymentStatus { get; set; }
    public string? EmploymentStatusName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }
    public bool IsActive { get; set; }
    public int ActiveAssetsCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class EmployeeCreateViewModel
{
    [Required(ErrorMessage = "EmployeeCode is required")]
    [StringLength(50, ErrorMessage = "EmployeeCode cannot exceed 50 characters")]
    public string EmployeeCode { get; set; } = default!;

    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
    public string FullName { get; set; } = default!;

    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
    public string? Address { get; set; }

    public int? DepartmentId { get; set; }
    public int? Position { get; set; }
    public int? EmploymentStatus { get; set; }

    [StringLength(20, ErrorMessage = "PhoneNumber cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    public int? OfficeId { get; set; }

    public DateTime? JoinDate { get; set; }
}

public class EmployeeUpdateViewModel
{
    [Required(ErrorMessage = "EmployeeCode is required")]
    [StringLength(50, ErrorMessage = "EmployeeCode cannot exceed 50 characters")]
    public string EmployeeCode { get; set; } = default!;

    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
    public string FullName { get; set; } = default!;

    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
    public string? Address { get; set; }

    public int? DepartmentId { get; set; }
    public int? Position { get; set; }
    public int? EmploymentStatus { get; set; }

    [StringLength(20, ErrorMessage = "PhoneNumber cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    public int? OfficeId { get; set; }

    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}

public class EmployeeAssetSummaryViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string? EmploymentStatusName { get; set; }

    public int CurrentlyHeldAssets { get; set; }
    public int AssetsOnLoan { get; set; }
    public int OverdueLoans { get; set; }
    public int TotalHistoricalAssets { get; set; }
    public int ReturnedAssets { get; set; }
    public int DamagedReturns { get; set; }

    public List<EmployeeAssetDetail> CurrentAssets { get; set; } = new();
    public List<EmployeeAssetHistory> AssetHistory { get; set; } = new();
}

public class EmployeeAssetDetail
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string AssociationType { get; set; } = default!;
    public DateTime SinceDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public bool IsOverdue { get; set; }
    public string? ConditionName { get; set; }
}

public class EmployeeAssetHistory
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string TransactionTypeName { get; set; } = default!;
    public DateTime TransactionDate { get; set; }
    public string? FromEmployeeName { get; set; }
    public string? ToEmployeeName { get; set; }
    public string? ConditionAfterName { get; set; }
    public string? Notes { get; set; }
}