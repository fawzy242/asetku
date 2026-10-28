namespace Whitebird.App.Features.Common;

/// <summary>
/// Provides a minimal (Id, Name[, Code]) view of a support entity, suitable for
/// select / autocomplete inputs in other menus' forms.
///
/// Kept separate from <see cref="ICommonService{TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM}"/>
/// because not every CRUD module needs a dropdown, and not every dropdown-providing
/// service implements full CRUD.
/// </summary>
public interface IDropdownProvider<TDropdownView>
    where TDropdownView : class
{
    Task<ServiceResult<IEnumerable<TDropdownView>>> GetDropdownListAsync();
}