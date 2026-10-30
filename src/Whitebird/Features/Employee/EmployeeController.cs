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
    //
    // P1-1 FIX: EPPlus reads only the OpenXML .xlsx format. The previous
    // "(.xlsx or .xls)" gate never matched reality — a valid .xls file reached
    // ExcelPackage and threw, exposing the raw exception to the client. The
    // gate is narrowed to .xlsx only, matching AssetController.Import and
    // AssetTransactionController.Import.

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx")
        {
            return BadRequest("Unsupported file format. Please upload an Excel Workbook (.xlsx).");
        }

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

    [HttpGet("{id:int}/photo")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var result = await _employeeService.GetPhotoAsync(id);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data.Bytes, result.Data.ContentType);
    }

    [HttpPost("{id:int}/photo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ServiceResult.Failure("No file provided"));

        return this.HandleResult(await _employeeService.UploadPhotoAsync(id, file));
    }

    [HttpDelete("{id:int}/photo")]
    public async Task<IActionResult> DeletePhoto(int id)
        => this.HandleResult(await _employeeService.DeletePhotoAsync(id));

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    [HttpGet("{id:int}/attachments")]
    public async Task<IActionResult> GetAttachments(int id)
        => this.HandleResult(await _employeeService.GetAttachmentsAsync(id));

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

    [HttpDelete("{id:int}/attachments/{fileAttachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(int id, int fileAttachmentId)
        => this.HandleResult(await _employeeService.DeleteAttachmentAsync(id, fileAttachmentId));

    [HttpGet("{id:int}/attachments/{fileAttachmentId:int}/download")]
    public async Task<IActionResult> DownloadAttachment(int id, int fileAttachmentId)
    {
        var result = await _employeeService.GetAttachmentFileAsync(id, fileAttachmentId);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data.Bytes, result.Data.ContentType, result.Data.FileName);
    }
}