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
    public async Task<IEnumerable<FileAttachmentEntity>> GetByReferenceAsync(string referenceTable, int referenceId)
    {
        const string sql = @"
            SELECT * FROM FileAttachment 
            WHERE ReferenceTable = @ReferenceTable AND ReferenceId = @ReferenceId AND IsActive = 1
            ORDER BY IsPrimary DESC, CreatedDate DESC";
        return await _context.QueryAsync<FileAttachmentEntity>(sql, new { ReferenceTable = referenceTable, ReferenceId = referenceId });
    }

    /// <inheritdoc />
    public async Task<FileAttachmentEntity?> GetPrimaryByReferenceAsync(string referenceTable, int referenceId)
    {
        const string sql = @"
            SELECT * FROM FileAttachment 
            WHERE ReferenceTable = @ReferenceTable AND ReferenceId = @ReferenceId AND IsPrimary = 1 AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<FileAttachmentEntity>(sql, new { ReferenceTable = referenceTable, ReferenceId = referenceId });
    }

    /// <inheritdoc />
    public async Task<int> InsertAsync(FileAttachmentEntity entity)
    {
        const string sql = @"
            INSERT INTO FileAttachment (
                ReferenceTable, ReferenceId, FileCategory, FileName, OriginalFileName, 
                FileExtension, FileMimeType, FilePath, FileSize, FileHash, Description, 
                VersionNumber, IsPrimary, IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @ReferenceTable, @ReferenceId, @FileCategory, @FileName, @OriginalFileName,
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
    public async Task<int> DeleteByReferenceAsync(string referenceTable, int referenceId)
    {
        const string sql = @"
            UPDATE FileAttachment 
            SET IsActive = 0, ModifiedDate = GETDATE() 
            WHERE ReferenceTable = @ReferenceTable AND ReferenceId = @ReferenceId";
        return await _context.ExecuteAsync(sql, new { ReferenceTable = referenceTable, ReferenceId = referenceId });
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int fileAttachmentId)
    {
        const string sql = "SELECT COUNT(1) FROM FileAttachment WHERE FileAttachmentId = @FileAttachmentId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { FileAttachmentId = fileAttachmentId }) > 0;
    }

    /// <inheritdoc />
    public async Task<int> GetCountByReferenceAsync(string referenceTable, int referenceId)
    {
        const string sql = "SELECT COUNT(*) FROM FileAttachment WHERE ReferenceTable = @ReferenceTable AND ReferenceId = @ReferenceId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { ReferenceTable = referenceTable, ReferenceId = referenceId });
    }

    /// <inheritdoc />
    public async Task<int> SetPrimaryAsync(int fileAttachmentId, string referenceTable, int referenceId)
    {
        const string clearSql = @"
            UPDATE FileAttachment 
            SET IsPrimary = 0, ModifiedDate = GETDATE() 
            WHERE ReferenceTable = @ReferenceTable AND ReferenceId = @ReferenceId AND IsActive = 1";

        await _context.ExecuteAsync(clearSql, new { ReferenceTable = referenceTable, ReferenceId = referenceId });

        if (fileAttachmentId > 0)
        {
            const string setSql = @"
                UPDATE FileAttachment 
                SET IsPrimary = 1, ModifiedDate = GETDATE() 
                WHERE FileAttachmentId = @FileAttachmentId AND IsActive = 1";
            return await _context.ExecuteAsync(setSql, new { FileAttachmentId = fileAttachmentId });
        }

        return 0;
    }
}