namespace Whitebird.Domain.Features.Home;

/// <summary>
/// Response DTO for home search
/// </summary>
public class HomeSearchResultDto
{
    public string Keyword { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public List<HomeSearchItemDto> Results { get; set; } = new();
}