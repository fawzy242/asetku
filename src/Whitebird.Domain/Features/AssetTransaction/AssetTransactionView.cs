using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.AssetTransaction;

public class AssetTransactionListViewModel
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = default!;
    public string? FromEmployeeName { get; set; }
    public string? ToEmployeeName { get; set; }
    public string? ToLocationName { get; set; }
    public DateTime TransactionDate { get; set; }
    public bool? Approved { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public int? FromAssetTransactionId { get; set; }
    public bool IsOverdue { get; set; }
}

public class AssetTransactionDetailViewModel
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = default!;
    public int? FromEmployeeId { get; set; }
    public string? FromEmployeeName { get; set; }
    public int? ToEmployeeId { get; set; }
    public string? ToEmployeeName { get; set; }
    public int? ToLocationId { get; set; }
    public string? ToLocationName { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string? Notes { get; set; }
    public int? ConditionBefore { get; set; }
    public string? ConditionBeforeName { get; set; }
    public int? ConditionAfter { get; set; }
    public string? ConditionAfterName { get; set; }
    public bool? Approved { get; set; }
    public string? ApprovedBy { get; set; }
    public int? MaintenanceType { get; set; }
    public string? MaintenanceTypeName { get; set; }
    public decimal? MaintenanceCost { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public int? FromAssetTransactionId { get; set; }
}

public class AssetTransactionCreateViewModel
{
    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "TransactionType is required")]
    public int TransactionType { get; set; }

    public int? FromEmployeeId { get; set; }
    public int? ToEmployeeId { get; set; }
    public int? ToLocationId { get; set; }

    [Required(ErrorMessage = "TransactionDate is required")]
    public DateTime TransactionDate { get; set; }

    public DateTime? ExpectedReturnDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    public int? ConditionBefore { get; set; }
    public int? ConditionAfter { get; set; }

    public int? MaintenanceType { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaintenanceCost must be positive")]
    public decimal? MaintenanceCost { get; set; }

    public int? FromAssetTransactionId { get; set; }
}

public class AssetTransactionUpdateViewModel
{
    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "TransactionType is required")]
    public int TransactionType { get; set; }

    public int? FromEmployeeId { get; set; }
    public int? ToEmployeeId { get; set; }
    public int? ToLocationId { get; set; }

    [Required(ErrorMessage = "TransactionDate is required")]
    public DateTime TransactionDate { get; set; }

    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    public int? ConditionBefore { get; set; }
    public int? ConditionAfter { get; set; }

    public bool? Approved { get; set; }
    public string? ApprovedBy { get; set; }

    public int? MaintenanceType { get; set; }
    public decimal? MaintenanceCost { get; set; }
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

    public int? ConditionAfter { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}