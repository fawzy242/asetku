namespace Whitebird.Domain.Features.Supplier;

/// <summary>
/// View model untuk Supplier dropdown (minimal data)
/// </summary>
public class SupplierDropdownView
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
}