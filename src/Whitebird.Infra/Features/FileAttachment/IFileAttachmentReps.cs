using Whitebird.Domain.Features.FileAttachment;

namespace Whitebird.Infra.Features.FileAttachment;

public interface IFileAttachmentReps
{
    Task<FileAttachmentEntity?> GetByIdAsync(int fileAttachmentId);
    Task<IEnumerable<FileAttachmentEntity>> GetByReferenceAsync(string referenceTable, int referenceId);
    Task<FileAttachmentEntity?> GetPrimaryByReferenceAsync(string referenceTable, int referenceId);
    Task<int> InsertAsync(FileAttachmentEntity entity);
    Task<int> UpdateAsync(FileAttachmentEntity entity);
    Task<int> DeleteAsync(int fileAttachmentId);
    Task<int> DeleteByReferenceAsync(string referenceTable, int referenceId);
    Task<bool> ExistsAsync(int fileAttachmentId);
    Task<int> GetCountByReferenceAsync(string referenceTable, int referenceId);
    Task<int> SetPrimaryAsync(int fileAttachmentId, string referenceTable, int referenceId);
}