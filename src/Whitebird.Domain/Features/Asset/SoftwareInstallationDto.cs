using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Asset;

/// <summary>
/// DTO for Software Installation Create/Update
/// </summary>
public class SoftwareInstallationDto
{
    [StringLength(50, ErrorMessage = "UserPassword cannot exceed 50 characters")]
    public string? UserPassword { get; set; }

    [StringLength(50, ErrorMessage = "AdminOperationalPassword cannot exceed 50 characters")]
    public string? AdminOperationalPassword { get; set; }

    public int? OperatingSystem { get; set; }
    public int? MicrosoftOffice { get; set; }
    public int? VisualStudio { get; set; }
    public int? DBMS { get; set; }
    public bool ConnectToNAS { get; set; }
}