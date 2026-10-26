using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Employee;
using Whitebird.Domain.Features.Employee;
using Whitebird.Features.Common;

namespace Whitebird.Features.Employee.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly EmployeeImportService _employeeImportService;

    public EmployeeController(IEmployeeService employeeService, EmployeeImportService employeeImportService)
    {
        _employeeService = employeeService;
        _employeeImportService = employeeImportService;
    }

    // ============================================================
    // CRUD
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _employeeService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _employeeService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _employeeService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateFormDto model)
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

    // ============================================================
    // GRID
    // ============================================================

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] EmployeeGridRequest request)
        => this.HandleResult(await _employeeService.GetGridDataAsync(request));

    // ============================================================
    // DROPDOWN
    // ============================================================

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdownList()
        => this.HandleResult(await _employeeService.GetDropdownListAsync());

    [HttpGet("office-list")]
    public async Task<IActionResult> GetOfficeList()
        => this.HandleResult(await _employeeService.GetOfficeListAsync());

    [HttpGet("department-list")]
    public async Task<IActionResult> GetDepartmentList()
        => this.HandleResult(await _employeeService.GetDepartmentListAsync());

    [HttpGet("masterdata-list")]
    public async Task<IActionResult> GetMasterDataList([FromQuery] string referenceName)
        => this.HandleResult(await _employeeService.GetMasterDataListAsync(referenceName));

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    [HttpPost("activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _employeeService.BulkActivateAsync(request));
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> BulkDeactivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _employeeService.BulkDeactivateAsync(request));
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _employeeService.BulkDeleteAsync(request));
    }

    // ============================================================
    // IMPORT
    // ============================================================

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        using var stream = file.OpenReadStream();
        var result = await _employeeImportService.ImportFromFileAsync(stream);
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

    // ============================================================
    // PHOTO PROFILE
    // ============================================================

    /// <summary>
    /// Get employee profile photo. Returns the image bytes with the correct Content-Type.
    /// </summary>
    [HttpGet("{id:int}/photo")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var result = await _employeeService.GetPhotoAsync(id);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data.Bytes, result.Data.ContentType);
    }

    /// <summary>
    /// Upload (or replace) the employee profile photo.
    /// </summary>
    [HttpPost("{id:int}/photo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ServiceResult.Failure("No file provided"));

        return this.HandleResult(await _employeeService.UploadPhotoAsync(id, file));
    }

    /// <summary>
    /// Delete the employee profile photo (soft-delete).
    /// </summary>
    [HttpDelete("{id:int}/photo")]
    public async Task<IActionResult> DeletePhoto(int id)
        => this.HandleResult(await _employeeService.DeletePhotoAsync(id));

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    /// <summary>
    /// Get employee attachment list.
    /// </summary>
    [HttpGet("{id:int}/attachments")]
    public async Task<IActionResult> GetAttachments(int id)
        => this.HandleResult(await _employeeService.GetAttachmentsAsync(id));

    /// <summary>
    /// Upload a standalone attachment (does not require sending the whole employee).
    /// </summary>
    [HttpPost("{id:int}/attachments")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAttachment(int id, [FromForm] AttachmentUploadDto model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;

        if (model.File == null || model.File.Length == 0)
            return BadRequest(ServiceResult.Failure("No file provided"));

        return this.HandleResult(await _employeeService.UploadAttachmentAsync(id, model));
    }

    /// <summary>
    /// Delete a single attachment (soft-delete).
    /// </summary>
    [HttpDelete("{id:int}/attachments/{fileAttachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(int id, int fileAttachmentId)
        => this.HandleResult(await _employeeService.DeleteAttachmentAsync(id, fileAttachmentId));

    /// <summary>
    /// Download a specific attachment belonging to this employee.
    /// Streams the raw bytes with the correct Content-Type and original file name.
    /// </summary>
    [HttpGet("{id:int}/attachments/{fileAttachmentId:int}/download")]
    public async Task<IActionResult> DownloadAttachment(int id, int fileAttachmentId)
    {
        var result = await _employeeService.GetAttachmentFileAsync(id, fileAttachmentId);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data.Bytes, result.Data.ContentType, result.Data.FileName);
    }
}