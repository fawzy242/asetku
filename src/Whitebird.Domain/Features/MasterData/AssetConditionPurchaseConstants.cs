namespace Whitebird.Domain.Features.MasterData;

/// <summary>
/// Asset condition purchase constants yang mereferensikan MasterData table.
/// ReferenceName = "AssetConditionPurchase"
/// 
/// NOTE: Nilai ini harus sinkron dengan data di MasterData table.
/// </summary>
public static class AssetConditionPurchaseConstants
{
    // Asset condition purchase values berdasarkan MasterData
    public const int NEW = 1;           // New
    public const int SECOND_HAND = 2;   // Second Hand
}