using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.FileAttachment;

namespace Whitebird.Infra.Features.FileAttachment;

/// <summary>
/// Repository implementation for File Attachment operations using Dapper
/// </summary>
public class FileAttachmentReps : IFileAttachmentReps
{
    private readonly DapperContext _context;

    public FileAttachmentReps(DapperContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<FileAttachmentEntity?> GetByIdAsync(int fileAttachmentId)
    {
        const string sql = "SELECT * FROM FileAttachment WHERE FileAttachmentId = @FileAttachmentId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<FileAttachmentEntity>(sql, new { FileAttachmentId = fileAttachmentId });
    }

    /// <inheritdoc />
    public async Task<int> InsertAsync(FileAttachmentEntity entity)
    {
        const string sql = @"
            INSERT INTO FileAttachment (
                FileCategory, FileName, OriginalFileName, 
                FileExtension, FileMimeType, FilePath, FileSize, FileHash, Description, 
                VersionNumber, IsPrimary, IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @FileCategory, @FileName, @OriginalFileName,
                @FileExtension, @FileMimeType, @FilePath, @FileSize, @FileHash, @Description,
                @VersionNumber, @IsPrimary, 1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    /// <inheritdoc />
    public async Task<int> UpdateAsync(FileAttachmentEntity entity)
    {
        const string sql = @"
            UPDATE FileAttachment SET
                Description = @Description,
                IsPrimary = @IsPrimary,
                ModifiedDate = GETDATE(),
                ModifiedBy = @ModifiedBy
            WHERE FileAttachmentId = @FileAttachmentId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, entity);
    }

    /// <inheritdoc />
    public async Task<int> DeleteAsync(int fileAttachmentId)
    {
        const string sql = "UPDATE FileAttachment SET IsActive = 0, ModifiedDate = GETDATE() WHERE FileAttachmentId = @FileAttachmentId";
        return await _context.ExecuteAsync(sql, new { FileAttachmentId = fileAttachmentId });
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int fileAttachmentId)
    {
        const string sql = "SELECT COUNT(1) FROM FileAttachment WHERE FileAttachmentId = @FileAttachmentId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { FileAttachmentId = fileAttachmentId }) > 0;
    }
}