using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.MasterData;

namespace Whitebird.Infra.Features.MasterData;

public class MasterDataReps : IMasterDataReps
{
    private readonly DapperContext _context;

    public MasterDataReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<MasterDataEntity?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM MasterData WHERE MasterDataId = @Id AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<MasterDataEntity>(sql, new { Id = id });
    }

    public async Task<IEnumerable<MasterDataEntity>> GetAllAsync()
    {
        const string sql = "SELECT * FROM MasterData WHERE IsActive = 1 ORDER BY ReferenceName, ReferenceCode";
        return await _context.QueryAsync<MasterDataEntity>(sql);
    }

    public async Task<IEnumerable<MasterDataEntity>> GetByReferenceNameAsync(string referenceName)
    {
        const string sql = @"
            SELECT * FROM MasterData 
            WHERE ReferenceName = @ReferenceName AND IsActive = 1 
            ORDER BY ReferenceCode";
        return await _context.QueryAsync<MasterDataEntity>(sql, new { ReferenceName = referenceName });
    }

    public async Task<MasterDataEntity?> GetByReferenceNameAndCodeAsync(string referenceName, int code)
    {
        const string sql = @"
            SELECT * FROM MasterData 
            WHERE ReferenceName = @ReferenceName AND ReferenceCode = @Code AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<MasterDataEntity>(sql, new { ReferenceName = referenceName, Code = code });
    }

    public async Task<bool> IsExistsAsync(string referenceName, int code)
    {
        const string sql = @"
            SELECT COUNT(1) FROM MasterData 
            WHERE ReferenceName = @ReferenceName AND ReferenceCode = @Code AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { ReferenceName = referenceName, Code = code }) > 0;
    }

    public async Task<IEnumerable<string>> GetDistinctReferenceNamesAsync()
    {
        const string sql = "SELECT DISTINCT ReferenceName FROM MasterData WHERE IsActive = 1 ORDER BY ReferenceName";
        return await _context.QueryAsync<string>(sql);
    }
}