namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// Complete employee summary view
/// </summary>
public class EmployeeSummaryView
{
    // Employee Info
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string? EmploymentStatusName { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }
    public bool IsActive { get; set; }

    // Photo Profile
    public string? PhotoProfileUrl { get; set; }

    // Attachments
    public List<EmployeeSummaryAttachmentDto> Attachments { get; set; } = new();

    // Statistics
    public EmployeeSummaryStatsDto Stats { get; set; } = new();

    // Current Holding
    public List<EmployeeCurrentHoldingDto> CurrentHolding { get; set; } = new();

    // Assets History (previously held, now with another employee)
    public List<EmployeeAssetsHistoryDto> AssetsHistory { get; set; } = new();

    // Transaction History
    public List<EmployeeTransactionHistoryDto> TransactionHistory { get; set; } = new();
}

/// <summary>
/// Attachment DTO for employee summary
/// </summary>
public class EmployeeSummaryAttachmentDto
{
    public int FileAttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? FileCategory { get; set; }
    public bool IsPrimary { get; set; }
}