namespace Whitebird.Domain.Features.MasterData;

/// <summary>
/// Office type constants yang mereferensikan MasterData table.
/// ReferenceName = "OfficeType"
/// 
/// NOTE: Nilai ini harus sinkron dengan data di MasterData table.
/// </summary>
public static class OfficeTypeConstants
{
    // Office type values berdasarkan MasterData
    public const int HEAD_OFFICE = 1;    // Head Office
    public const int BRANCH_OFFICE = 2;  // Branch Office
}