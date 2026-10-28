using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Office;

namespace Whitebird.App.Features.Office;

public interface IOfficeService :
    ICommonService<OfficeDetailView, OfficeListView, OfficeGridRequest, OfficeCreateViewModel, OfficeUpdateViewModel>,
    IDropdownProvider<OfficeDropdownView>
{
}