using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Category;

namespace Whitebird.App.Features.Category;

/// <summary>
/// Service interface for Category business logic.
///
/// Inherits the standard CRUD surface from <see cref="ICommonService{TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM}"/>
/// and the dropdown surface from <see cref="IDropdownProvider{TDropdownView}"/>.
/// No Category-specific methods are declared here — the standard set covers the
/// menu exactly.
/// </summary>
public interface ICategoryService :
    ICommonService<CategoryDetailView, CategoryListView, CategoryGridRequest, CategoryCreateViewModel, CategoryUpdateViewModel>,
    IDropdownProvider<CategoryDropdownView>
{
}