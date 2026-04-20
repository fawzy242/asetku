using Whitebird.Domain.Features.Supplier.View;
using Whitebird.App.Features.Common.Service;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Supplier.Interfaces;

public interface ISupplierService
{
    Task<ServiceResult<SupplierDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<SupplierListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<SupplierListViewModel>>> GetActiveOnlyAsync();
    Task<ServiceResult<SupplierDetailViewModel>> CreateAsync(SupplierCreateViewModel model);
    Task<ServiceResult<SupplierDetailViewModel>> UpdateAsync(int id, SupplierUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);
    Task<ServiceResult<PaginatedResult<SupplierListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null);
}