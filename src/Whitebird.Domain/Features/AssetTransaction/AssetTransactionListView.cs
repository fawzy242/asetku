using System;

namespace Whitebird.Domain.Features.AssetTransaction
{
    /// <summary>
    /// View model untuk AssetTransaction grid/list (bukan Entity!)
    /// Data dari JOIN: Asset, Employee, MasterData (TransactionType)
    /// </summary>
    public class AssetTransactionListView
    {
        // Primary key
        public int AssetTransactionId { get; set; }
        
        // Asset (from LEFT JOIN)
        public int AssetId { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        
        // Transaction Type (from MasterData LEFT JOIN)
        public int TransactionType { get; set; }
        public string? TransactionTypeName { get; set; }
        
        // From Employee (from LEFT JOIN)
        public int? FromEmployeeId { get; set; }
        public string? FromEmployeeName { get; set; }
        
        // To Employee (from LEFT JOIN)
        public int? ToEmployeeId { get; set; }
        public string? ToEmployeeName { get; set; }
        
        // To Location / Office (from LEFT JOIN)
        public int? ToLocationId { get; set; }
        public string? ToLocationName { get; set; }
        
        // Dates
        public DateTime TransactionDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public DateTime? ActualReturnDate { get; set; }
        
        // Notes
        public string? Notes { get; set; }
        
        // Conditions (from MasterData LEFT JOIN)
        public int? ConditionBefore { get; set; }
        public string? ConditionBeforeName { get; set; }
        public int? ConditionAfter { get; set; }
        public string? ConditionAfterName { get; set; }
        
        // Approval
        public bool? Approved { get; set; }
        public string? ApprovedBy { get; set; }
        
        // Maintenance
        public int? MaintenanceType { get; set; }
        public string? MaintenanceTypeName { get; set; }
        public decimal? MaintenanceCost { get; set; }
        
        // Pairing
        public int? FromAssetTransactionId { get; set; }
        
        // Calculated field (not from database)
        public bool IsOverdue { get; set; }
        
        // Audit fields
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = "System";
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
    }
}