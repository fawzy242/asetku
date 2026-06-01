namespace Whitebird.Domain.Features.MasterData;

/// <summary>
/// Employee status constants yang mereferensikan MasterData table.
/// ReferenceName = "EmployeeStatus"
/// 
/// NOTE: Nilai ini harus sinkron dengan data di MasterData table.
/// </summary>
public static class EmployeeStatusConstants
{
    // Employee status values berdasarkan MasterData
    public const int INTERNSHIP = 1;        // Internship
    public const int TRAINING = 2;          // Training
    public const int CONTRACT = 3;          // Contract
    public const int PERMANENT = 4;         // Permanent Employee
    public const int RESIGNED = 5;          // Resigned

    /// <summary>
    /// Check if employee is active (not resigned)
    /// </summary>
    public static bool IsActive(int status) => status != RESIGNED;
}