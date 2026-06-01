using System;

namespace Whitebird.Domain.Features.Asset;

/// <summary>
/// View model untuk Asset grid/list (bukan Entity!)
/// Data dari JOIN: Category, Supplier, Office, MasterData (AssetCondition)
/// CurrentStatus dihitung dari active transaction
/// </summary>
public class AssetListView
{
    // Primary key
    public int AssetId { get; set; }
    
    // Core fields
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    
    // Category (from LEFT JOIN)
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    
    // Brand & Model
    public string? Brand { get; set; }
    public string? Model { get; set; }
    
    // Serial & IMEI
    public string? SerialNumber { get; set; }
    public string? Imei { get; set; }
    public string? MacAddress { get; set; }
    
    // Network info
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
    
    // Purchase info
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? InvoiceNumber { get; set; }
    
    // Supplier (from LEFT JOIN)
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    
    // Warranty
    public int? WarrantyPeriod { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    
    // Asset Condition (from MasterData LEFT JOIN)
    public int? AssetCondition { get; set; }
    public string? AssetConditionName { get; set; }
    
    // Asset Condition Purchase
    public int? AssetConditionPurchase { get; set; }
    public string? AssetConditionPurchaseName { get; set; }
    
    // Financial
    public decimal? ResidualValue { get; set; }
    public int? UsefulLife { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    
    // Location (Office from LEFT JOIN)
    public int? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    
    // Operational flag
    public bool? OperasionalOffice { get; set; }
    
    // Notes
    public string? Notes { get; set; }
    
    // Maintenance dates
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    
    // Current status (derived from active transaction)
    public string? CurrentStatus { get; set; }
    
    // Audit fields
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}