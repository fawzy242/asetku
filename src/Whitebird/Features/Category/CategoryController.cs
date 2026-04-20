using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Category.Interfaces;
using Whitebird.Domain.Features.Category.View;
using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.Category.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService) => _categoryService = categoryService;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => this.HandleResult(await _categoryService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll() => this.HandleResult(await _categoryService.GetAllAsync());

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveOnly() => this.HandleResult(await _categoryService.GetActiveOnlyAsync());

    [HttpGet("subcategories/{parentId:int}")]
    public async Task<IActionResult> GetSubCategories(int parentId) => this.HandleResult(await _categoryService.GetSubCategoriesAsync(parentId));

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        => this.HandleResult(await _categoryService.GetGridDataAsync(page, pageSize, search));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _categoryService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _categoryService.UpdateAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => this.HandleResult(await _categoryService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id) => this.HandleResult(await _categoryService.SoftDeleteAsync(id));
}