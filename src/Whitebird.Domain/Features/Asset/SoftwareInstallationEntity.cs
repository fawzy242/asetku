using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Asset;

public class SoftwareInstallationEntity : AuditableEntity
{
    public int SoftwareInstallationId { get; set; }
    public int AssetId { get; set; }
    public string? UserPassword { get; set; }
    public string? AdminOperationalPassword { get; set; }
    public int? OperatingSystem { get; set; }
    public int? MicrosoftOffice { get; set; }
    public int? VisualStudio { get; set; }
    public int? DBMS { get; set; }
    public bool ConnectToNAS { get; set; }
}