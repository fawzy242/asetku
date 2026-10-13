using Whitebird.Domain.Features.FileAttachment;

namespace Whitebird.Infra.Features.FileAttachment;

/// <summary>
/// Repository interface for File Attachment operations
/// </summary>
public interface IFileAttachmentReps
{
    /// <summary>
    /// Gets a file attachment by ID
    /// </summary>
    Task<FileAttachmentEntity?> GetByIdAsync(int fileAttachmentId);

    /// <summary>
    /// Inserts a new file attachment
    /// </summary>
    Task<int> InsertAsync(FileAttachmentEntity entity);

    /// <summary>
    /// Updates file attachment metadata (description, isPrimary)
    /// </summary>
    Task<int> UpdateAsync(FileAttachmentEntity entity);

    /// <summary>
    /// Soft-deletes a file attachment (IsActive = 0)
    /// </summary>
    Task<int> DeleteAsync(int fileAttachmentId);

    /// <summary>
    /// Checks if an active file attachment exists
    /// </summary>
    Task<bool> ExistsAsync(int fileAttachmentId);
}