using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Supplier;

namespace Whitebird.App.Features.Supplier;

public interface ISupplierService :
    ICommonService<SupplierDetailView, SupplierListView, SupplierGridRequest, SupplierCreateViewModel, SupplierUpdateViewModel>,
    IDropdownProvider<SupplierDropdownView>
{
}