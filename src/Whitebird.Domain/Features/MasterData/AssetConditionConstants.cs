namespace Whitebird.Domain.Features.MasterData;

/// <summary>
/// Asset condition constants yang mereferensikan MasterData table.
/// ReferenceName = "AssetCondition"
/// 
/// NOTE: Nilai ini harus sinkron dengan data di MasterData table.
/// </summary>
public static class AssetConditionConstants
{
    // Asset condition values berdasarkan MasterData
    public const int GOOD = 1;      // Good
    public const int NORMAL = 2;    // Normal
    public const int DAMAGED = 3;   // Damaged

    /// <summary>
    /// Check if condition is damaged
    /// </summary>
    public static bool IsDamaged(int condition) => condition == DAMAGED;
}