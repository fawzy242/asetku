namespace Whitebird.Domain.Features.Home;

/// <summary>
/// Search result item DTO for home search
/// </summary>
public class HomeSearchItemDto
{
    public string Type { get; set; } = string.Empty; // "Asset" atau "Employee"
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
}