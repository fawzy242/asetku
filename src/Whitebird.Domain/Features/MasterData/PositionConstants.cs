namespace Whitebird.Domain.Features.MasterData;

/// <summary>
/// Position constants yang mereferensikan MasterData table.
/// ReferenceName = "Position"
/// 
/// NOTE: Nilai ini harus sinkron dengan data di MasterData table.
/// </summary>
public static class PositionConstants
{
    // Position values berdasarkan MasterData
    public const int DIRECTOR = 1;           // Director
    public const int VICE_PRESIDENT = 2;     // Vice President
    public const int HEAD_OF_DEPARTMENT = 3; // Head Of Department
    public const int MANAGER = 4;            // Manager
    public const int SUPERVISOR = 5;         // Supervisor
    public const int SENIOR_ASSOCIATE = 6;   // Senior Associate
    public const int JUNIOR_ASSOCIATE = 7;   // Junior Associate
}