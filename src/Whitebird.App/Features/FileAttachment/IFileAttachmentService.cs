using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.FileAttachment;

namespace Whitebird.App.Features.FileAttachment;

public interface IFileAttachmentService
{
    Task<ServiceResult<FileAttachmentDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<FileAttachmentListViewModel>>> GetByReferenceAsync(string referenceTable, int referenceId);
    Task<ServiceResult<FileAttachmentResponseDto>> UploadAsync(string referenceTable, int referenceId, FileAttachmentUploadViewModel model);
    Task<ServiceResult<List<FileAttachmentResponseDto>>> UploadMultipleAsync(string referenceTable, int referenceId, FileAttachmentMultipleUploadViewModel model);
    Task<ServiceResult<FileAttachmentResponseDto>> UpdateAsync(int id, FileAttachmentUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult<byte[]>> DownloadAsync(int id);
    Task<ServiceResult<string>> GetFileUrlAsync(int id);
    Task<ServiceResult<bool>> IsImageFileAsync(int id);
}