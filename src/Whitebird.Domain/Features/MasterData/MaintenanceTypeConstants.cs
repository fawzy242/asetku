namespace Whitebird.Domain.Features.MasterData;

/// <summary>
/// Maintenance type constants yang mereferensikan MasterData table.
/// ReferenceName = "MaintenanceType"
/// 
/// NOTE: Nilai ini harus sinkron dengan data di MasterData table.
/// </summary>
public static class MaintenanceTypeConstants
{
    // Maintenance type values berdasarkan MasterData
    public const int PREVENTIVE = 1;      // PREVENTIVE MAINTENANCE
    public const int CORRECTIVE = 2;      // CORRECTIVE MAINTENANCE
    public const int EMERGENCY = 3;       // EMERGENCY REPAIR
    public const int HARDWARE_REPLACEMENT = 4;  // HARDWARE REPLACEMENT
    public const int SOFTWARE_UPDATE = 5;       // SOFTWARE UPDATE
    public const int INSPECTION = 6;            // INSPECTION
    public const int CLEANING = 7;              // CLEANING
    public const int CALIBRATION = 8;           // CALIBRATION
}