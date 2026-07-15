namespace Whitebird.Domain.Features.Asset;

/// <summary>
/// View model for Software Installation detail
/// </summary>
public class SoftwareInstallationView
{
    public int SoftwareInstallationId { get; set; }
    public int AssetId { get; set; }
    public string? UserPassword { get; set; }
    public string? AdminOperationalPassword { get; set; }
    public int? OperatingSystem { get; set; }
    public string? OperatingSystemName { get; set; }
    public int? MicrosoftOffice { get; set; }
    public string? MicrosoftOfficeName { get; set; }
    public int? VisualStudio { get; set; }
    public string? VisualStudioName { get; set; }
    public int? DBMS { get; set; }
    public string? DBMSName { get; set; }
    public bool ConnectToNAS { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}