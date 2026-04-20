using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Employee.Interfaces;
using Whitebird.Domain.Features.Employee.View;
using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.Employee.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService) => _employeeService = employeeService;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) => this.HandleResult(await _employeeService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll() => this.HandleResult(await _employeeService.GetAllAsync());

    [HttpGet("department/{department}")]
    public async Task<IActionResult> GetByDepartment(string department) => this.HandleResult(await _employeeService.GetByDepartmentAsync(department));

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status) => this.HandleResult(await _employeeService.GetByStatusAsync(status));

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
    public async Task<IActionResult> Delete(int id) => this.HandleResult(await _employeeService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id) => this.HandleResult(await _employeeService.SoftDeleteAsync(id));
}