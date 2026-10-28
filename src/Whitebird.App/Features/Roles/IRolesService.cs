using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Roles;

namespace Whitebird.App.Features.Roles;

public interface IRolesService :
    ICommonService<RolesDetailView, RolesListView, RolesGridRequest, RolesCreateViewModel, RolesUpdateViewModel>,
    IDropdownProvider<RolesDropdownView>
{
}