using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Office;

namespace Whitebird.Infra.Features.Office;

public class OfficeReps : IOfficeReps
{
    private readonly DapperContext _context;

    public OfficeReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<OfficeEntity?> GetByIdAsync(int officeId)
    {
        const string sql = "SELECT * FROM Office WHERE OfficeId = @OfficeId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<OfficeEntity>(sql, new { OfficeId = officeId });
    }

    public async Task<OfficeEntity?> GetByIdWithRelationsAsync(int officeId)
    {
        const string sql = @"
            SELECT o.*, p.OfficeName as ParentOfficeName
            FROM Office o
            LEFT JOIN Office p ON o.ParentOfficeId = p.OfficeId
            WHERE o.OfficeId = @OfficeId AND o.IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<OfficeEntity>(sql, new { OfficeId = officeId });
    }

    public async Task<IEnumerable<OfficeEntity>> GetAllAsync()
    {
        const string sql = @"
            SELECT o.*, p.OfficeName as ParentOfficeName,
                   (SELECT COUNT(*) FROM Office WHERE ParentOfficeId = o.OfficeId AND IsActive = 1) as ChildCount
            FROM Office o
            LEFT JOIN Office p ON o.ParentOfficeId = p.OfficeId
            WHERE o.IsActive = 1
            ORDER BY o.OfficeName";
        return await _context.QueryAsync<OfficeEntity>(sql);
    }

    public async Task<IEnumerable<OfficeEntity>> GetActiveOnlyAsync()
    {
        const string sql = @"
            SELECT o.*, p.OfficeName as ParentOfficeName
            FROM Office o
            LEFT JOIN Office p ON o.ParentOfficeId = p.OfficeId
            WHERE o.IsActive = 1
            ORDER BY o.OfficeName";
        return await _context.QueryAsync<OfficeEntity>(sql);
    }

    public async Task<IEnumerable<OfficeEntity>> GetSubOfficesAsync(int parentOfficeId)
    {
        const string sql = @"
            SELECT * FROM Office 
            WHERE ParentOfficeId = @ParentOfficeId AND IsActive = 1 
            ORDER BY OfficeName";
        return await _context.QueryAsync<OfficeEntity>(sql, new { ParentOfficeId = parentOfficeId });
    }

    public async Task<bool> IsOfficeCodeExistsAsync(string officeCode, int? excludeOfficeId = null)
    {
        if (string.IsNullOrWhiteSpace(officeCode))
            return false;

        var sql = "SELECT COUNT(1) FROM Office WHERE OfficeCode = @OfficeCode AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@OfficeCode", officeCode);

        if (excludeOfficeId.HasValue)
        {
            sql += " AND OfficeId != @ExcludeOfficeId";
            parameters.Add("@ExcludeOfficeId", excludeOfficeId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<bool> IsOfficeNameExistsAsync(string officeName, int? excludeOfficeId = null)
    {
        var sql = "SELECT COUNT(1) FROM Office WHERE OfficeName = @OfficeName AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@OfficeName", officeName);

        if (excludeOfficeId.HasValue)
        {
            sql += " AND OfficeId != @ExcludeOfficeId";
            parameters.Add("@ExcludeOfficeId", excludeOfficeId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<int> GetChildCountAsync(int officeId)
    {
        const string sql = "SELECT COUNT(*) FROM Office WHERE ParentOfficeId = @OfficeId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { OfficeId = officeId });
    }
}