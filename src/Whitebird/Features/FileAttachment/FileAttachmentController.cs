using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.FileAttachment;
using Whitebird.Features.Common;

namespace Whitebird.Features.FileAttachment;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FileAttachmentController : ControllerBase
{
    private readonly IFileAttachmentService _fileAttachmentService;

    public FileAttachmentController(IFileAttachmentService fileAttachmentService)
    {
        _fileAttachmentService = fileAttachmentService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => this.HandleResult(await _fileAttachmentService.GetByIdAsync(id));

    [HttpGet("reference/{referenceTable}/{referenceId:int}")]
    public async Task<IActionResult> GetByReference(string referenceTable, int referenceId)
        => this.HandleResult(await _fileAttachmentService.GetByReferenceAsync(referenceTable, referenceId));

    [HttpPost("upload/{referenceTable}/{referenceId:int}")]
    public async Task<IActionResult> Upload(string referenceTable, int referenceId, [FromForm] FileAttachmentUploadViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _fileAttachmentService.UploadAsync(referenceTable, referenceId, model));
    }

    [HttpPost("upload-multiple/{referenceTable}/{referenceId:int}")]
    public async Task<IActionResult> UploadMultiple(string referenceTable, int referenceId, [FromForm] FileAttachmentMultipleUploadViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _fileAttachmentService.UploadMultipleAsync(referenceTable, referenceId, model));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FileAttachmentUpdateViewModel model)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _fileAttachmentService.UpdateAsync(id, model));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
        => this.HandleResult(await _fileAttachmentService.DeleteAsync(id));

    [HttpGet("download/{id:int}")]
    public async Task<IActionResult> Download(int id)
    {
        var result = await _fileAttachmentService.DownloadAsync(id);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        var attachment = await _fileAttachmentService.GetByIdAsync(id);
        var fileName = attachment.IsSuccess && attachment.Data != null
            ? attachment.Data.OriginalFileName
            : $"file_{id}";

        return File(result.Data, "application/octet-stream", fileName);
    }

    [HttpGet("preview/{id:int}")]
    public async Task<IActionResult> Preview(int id)
    {
        var isImageResult = await _fileAttachmentService.IsImageFileAsync(id);
        if (!isImageResult.IsSuccess || !isImageResult.Data)
            return NotFound("Preview only available for image files");

        var result = await _fileAttachmentService.DownloadAsync(id);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        var attachment = await _fileAttachmentService.GetByIdAsync(id);
        var contentType = attachment.IsSuccess && attachment.Data != null
            ? attachment.Data.FileMimeType ?? "image/jpeg"
            : "image/jpeg";

        return File(result.Data, contentType);
    }
}