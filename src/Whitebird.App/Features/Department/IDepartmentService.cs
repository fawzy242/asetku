using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;

namespace Whitebird.App.Features.Department;

public interface IDepartmentService :
    ICommonService<DepartmentDetailView, DepartmentListView, DepartmentGridRequest, DepartmentCreateViewModel, DepartmentUpdateViewModel>,
    IDropdownProvider<DepartmentDropdownView>
{
}