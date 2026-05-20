using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Employee.Import;
using Whitebird.App.Features.Employee.Interfaces;
using Whitebird.Domain.Features.Employee;

namespace Whitebird.App.Features.Employee.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly EmployeeImportService _employeeImportService;

    public EmployeeController(IEmployeeService employeeService, EmployeeImportService employeeImportService)
    {
        _employeeService = employeeService;
        _employeeImportService = employeeImportService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _employeeService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _employeeService.GetAllAsync());

    [HttpGet("department/{departmentId:int}")]
    public async Task<IActionResult> GetByDepartment(int departmentId)
        => this.HandleResult(await _employeeService.GetByDepartmentAsync(departmentId));

    [HttpGet("status/{employmentStatus:int}")]
    public async Task<IActionResult> GetByStatus(int employmentStatus)
        => this.HandleResult(await _employeeService.GetByStatusAsync(employmentStatus));

    [HttpGet("{id:int}/asset-summary")]
    public async Task<IActionResult> GetAssetSummary(int id)
        => this.HandleResult(await _employeeService.GetAssetSummaryAsync(id));

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] bool sortDescending = false)
        => this.HandleResult(await _employeeService.GetGridDataAsync(page, pageSize, search, sortBy, sortDescending));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _employeeService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _employeeService.UpdateAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _employeeService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
        => this.HandleResult(await _employeeService.SoftDeleteAsync(id));

    [HttpPost("activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _employeeService.BulkActivateAsync(request));
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        using var stream = file.OpenReadStream();
        var result = await _employeeImportService.ImportFromExcelAsync(stream);
        return this.HandleResult(result);
    }

    [HttpGet("import/template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _employeeImportService.GenerateTemplateAsync();
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Employee_Import_Template.xlsx");
    }
}