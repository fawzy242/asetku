using Whitebird.Domain.Features.Common.Entities;

namespace Whitebird.Domain.Features.Supplier.Entities;

public class SupplierEntity : AuditableEntity
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = default!;
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}