using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Office;

public class OfficeEntity : AuditableEntity
{
    public int OfficeId { get; set; }
    public string? OfficeCode { get; set; }
    public string OfficeName { get; set; } = default!;
    public int? OfficeType { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int? ParentOfficeId { get; set; }
}