using Whitebird.Domain.Features.Supplier.Entities;

namespace Whitebird.Infra.Features.Supplier;

public interface ISupplierReps
{
    Task<SupplierEntity?> GetByIdAsync(int supplierId);
    Task<IEnumerable<SupplierEntity>> GetAllAsync();
    Task<IEnumerable<SupplierEntity>> GetActiveOnlyAsync();
    Task<bool> IsSupplierNameExistsAsync(string supplierName, int? excludeSupplierId = null);
    Task<int> GetAssetCountAsync(int supplierId);
}