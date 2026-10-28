using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Common.Import;
using Whitebird.Domain.Features.Asset;
using Whitebird.Features.Common;

namespace Whitebird.Features.Asset.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AssetController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly AssetImportService _assetImportService;

    public AssetController(IAssetService assetService, AssetImportService assetImportService)
    {
        _assetService = assetService;
        _assetImportService = assetImportService;
    }

    // ============================================================
    // CRUD
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _assetService.GetByIdAsync(id));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.HandleResult(await _assetService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AssetCreateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _assetService.CreateAsync(model), nameof(GetById));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AssetUpdateFormDto model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _assetService.UpdateAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _assetService.DeleteAsync(id));

    [HttpDelete("{id:int}/soft")]
    public async Task<IActionResult> SoftDelete(int id)
        => this.HandleResult(await _assetService.SoftDeleteAsync(id));

    // ============================================================
    // GRID
    // ============================================================

    [HttpGet("grid")]
    public async Task<IActionResult> GetGridData([FromQuery] AssetGridRequest request)
        => this.HandleResult(await _assetService.GetGridDataAsync(request));

    // ============================================================
    // IMPORT
    // ============================================================
    //
    // H9: EPPlus reads only the OpenXML .xlsx format. It has never supported
    // the legacy BIFF8 .xls binary format, so the previous "(.xlsx or .xls)"
    // promise never matched reality — a valid .xls file reached ExcelPackage
    // and threw, exposing the raw exception to the client. Narrowing the gate
    // to .xlsx makes the promise truthful.

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
        var result = await _assetImportService.ImportFromFileAsync(stream);
        return this.HandleResult(result);
    }

    [HttpGet("import/template")]
    public async Task<IActionResult> DownloadImportTemplate()
    {
        var result = await _assetImportService.GenerateTemplateAsync();
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Asset_Import_Template.xlsx");
    }

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    [HttpPost("activate")]
    public async Task<IActionResult> BulkActivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _assetService.BulkActivateAsync(request));
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> BulkDeactivate([FromBody] BulkActivateRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _assetService.BulkDeactivateAsync(request));
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _assetService.BulkDeleteAsync(request));
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    [HttpGet("list-by-transaction")]
    public async Task<IActionResult> GetDropdownListByTransactionType([FromQuery] int transactionType)
        => this.HandleResult(await _assetService.GetDropdownListByTransactionTypeAsync(transactionType));

    // ============================================================
    // PHOTO PROFILE
    // ============================================================

    [HttpGet("{id:int}/photo")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var result = await _assetService.GetPhotoAsync(id);
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

        return this.HandleResult(await _assetService.UploadPhotoAsync(id, file));
    }

    [HttpDelete("{id:int}/photo")]
    public async Task<IActionResult> DeletePhoto(int id)
        => this.HandleResult(await _assetService.DeletePhotoAsync(id));

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    [HttpGet("{id:int}/attachments")]
    public async Task<IActionResult> GetAttachments(int id)
        => this.HandleResult(await _assetService.GetAttachmentsAsync(id));

    [HttpPost("{id:int}/attachments")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAttachment(int id, [FromForm] AttachmentUploadDto model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;

        if (model.File == null || model.File.Length == 0)
            return BadRequest(ServiceResult.Failure("No file provided"));

        return this.HandleResult(await _assetService.UploadAttachmentAsync(id, model));
    }

    [HttpDelete("{id:int}/attachments/{fileAttachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(int id, int fileAttachmentId)
        => this.HandleResult(await _assetService.DeleteAttachmentAsync(id, fileAttachmentId));

    [HttpGet("{id:int}/attachments/{fileAttachmentId:int}/download")]
    public async Task<IActionResult> DownloadAttachment(int id, int fileAttachmentId)
    {
        var result = await _assetService.GetAttachmentFileAsync(id, fileAttachmentId);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data.Bytes, result.Data.ContentType, result.Data.FileName);
    }
}