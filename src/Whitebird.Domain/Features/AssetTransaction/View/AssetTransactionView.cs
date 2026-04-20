using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.AssetTransaction.View;

public class AssetTransactionListViewModel
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string TransactionType { get; set; } = default!;
    public string? FromEmployeeName { get; set; }
    public string? ToEmployeeName { get; set; }
    public string? FromLocationName { get; set; }
    public string? ToLocationName { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionStatus { get; set; } = default!;
    public DateTime? ExpectedReturnDate { get; set; }
}

public class AssetTransactionDetailViewModel
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string TransactionType { get; set; } = default!;
    public int? FromEmployeeId { get; set; }
    public string? FromEmployeeName { get; set; }
    public int? ToEmployeeId { get; set; }
    public string? ToEmployeeName { get; set; }
    public int? FromLocationId { get; set; }
    public string? FromLocationName { get; set; }
    public int? ToLocationId { get; set; }
    public string? ToLocationName { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string? Notes { get; set; }
    public string? ConditionBefore { get; set; }
    public string? ConditionAfter { get; set; }
    public string TransactionStatus { get; set; } = default!;
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public string? MaintenanceType { get; set; }
    public decimal? MaintenanceCost { get; set; }
    public string? VendorName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class AssetTransactionCreateViewModel
{
    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "TransactionType is required")]
    [StringLength(50, ErrorMessage = "TransactionType cannot exceed 50 characters")]
    public string TransactionType { get; set; } = default!;

    [Range(1, int.MaxValue, ErrorMessage = "FromEmployeeId must be greater than 0 if provided")]
    public int? FromEmployeeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ToEmployeeId must be greater than 0 if provided")]
    public int? ToEmployeeId { get; set; }

    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }

    [Required(ErrorMessage = "TransactionDate is required")]
    public DateTime TransactionDate { get; set; }

    public DateTime? ExpectedReturnDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    [StringLength(20, ErrorMessage = "ConditionBefore cannot exceed 20 characters")]
    public string? ConditionBefore { get; set; }

    [StringLength(20, ErrorMessage = "ConditionAfter cannot exceed 20 characters")]
    public string? ConditionAfter { get; set; }

    [StringLength(20, ErrorMessage = "TransactionStatus cannot exceed 20 characters")]
    public string TransactionStatus { get; set; } = "Pending";

    public int? ApprovedBy { get; set; }

    [StringLength(50, ErrorMessage = "MaintenanceType cannot exceed 50 characters")]
    public string? MaintenanceType { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaintenanceCost must be positive")]
    public decimal? MaintenanceCost { get; set; }

    [StringLength(100, ErrorMessage = "VendorName cannot exceed 100 characters")]
    public string? VendorName { get; set; }
}

public class AssetTransactionUpdateViewModel
{
    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "TransactionType is required")]
    [StringLength(50, ErrorMessage = "TransactionType cannot exceed 50 characters")]
    public string TransactionType { get; set; } = default!;

    [Range(1, int.MaxValue, ErrorMessage = "FromEmployeeId must be greater than 0 if provided")]
    public int? FromEmployeeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ToEmployeeId must be greater than 0 if provided")]
    public int? ToEmployeeId { get; set; }

    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }

    [Required(ErrorMessage = "TransactionDate is required")]
    public DateTime TransactionDate { get; set; }

    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    [StringLength(20, ErrorMessage = "ConditionBefore cannot exceed 20 characters")]
    public string? ConditionBefore { get; set; }

    [StringLength(20, ErrorMessage = "ConditionAfter cannot exceed 20 characters")]
    public string? ConditionAfter { get; set; }

    [Required(ErrorMessage = "TransactionStatus is required")]
    [StringLength(20, ErrorMessage = "TransactionStatus cannot exceed 20 characters")]
    public string TransactionStatus { get; set; } = default!;

    public int? ApprovedBy { get; set; }

    [StringLength(50, ErrorMessage = "MaintenanceType cannot exceed 50 characters")]
    public string? MaintenanceType { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaintenanceCost must be positive")]
    public decimal? MaintenanceCost { get; set; }

    [StringLength(100, ErrorMessage = "VendorName cannot exceed 100 characters")]
    public string? VendorName { get; set; }
}

public class AssetTransactionApproveViewModel
{
    public int AssetTransactionId { get; set; }

    [Required(ErrorMessage = "Approval decision is required")]
    public bool IsApproved { get; set; }

    [StringLength(500, ErrorMessage = "Approval notes cannot exceed 500 characters")]
    public string? ApprovalNotes { get; set; }
}

public class AssetReturnViewModel
{
    [Required(ErrorMessage = "AssetTransactionId is required")]
    public int AssetTransactionId { get; set; }

    [Required(ErrorMessage = "ActualReturnDate is required")]
    public DateTime ActualReturnDate { get; set; }

    [StringLength(20, ErrorMessage = "ConditionAfter cannot exceed 20 characters")]
    public string? ConditionAfter { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}