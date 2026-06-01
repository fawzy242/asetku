namespace Whitebird.Domain.Features.ActivityLog;

/// <summary>
/// DTO for creating a new activity log entry
/// </summary>
public class CreateActivityLogDto
{
    /// <summary>
    /// The name of the reference table (e.g., "Asset", "Employee")
    /// </summary>
    public string ReferenceTable { get; set; } = default!;
    
    /// <summary>
    /// The ID of the record in the reference table
    /// </summary>
    public int ReferenceId { get; set; }
    
    /// <summary>
    /// The type of activity (CREATE, UPDATE, DELETE, SOFT_DELETE, ERROR, etc.)
    /// </summary>
    public string ActivityType { get; set; } = default!;
    
    /// <summary>
    /// Human-readable description of the activity
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// The user who performed the activity
    /// </summary>
    public string? CreatedBy { get; set; }
}