using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Location.Entities;

namespace Whitebird.Infra.Features.Location;

public class LocationReps : ILocationReps
{
    private readonly DapperContext _context;

    public LocationReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<LocationEntity?> GetByIdRawAsync(int locationId)
    {
        const string sql = "SELECT * FROM Location WHERE LocationId = @LocationId";
        return await _context.QueryFirstOrDefaultAsync<LocationEntity>(sql, new { LocationId = locationId });
    }

    public async Task<LocationEntity?> GetByIdWithRelationsAsync(int locationId)
    {
        const string sql = @"
            SELECT l.*, p.LocationName as ParentLocationName
            FROM Location l
            LEFT JOIN Location p ON l.ParentLocationId = p.LocationId
            WHERE l.LocationId = @LocationId";

        return await _context.QueryFirstOrDefaultAsync<LocationEntity>(sql, new { LocationId = locationId });
    }

    public async Task<IEnumerable<LocationEntity>> GetAllWithRelationsAsync()
    {
        const string sql = @"
            SELECT l.*, p.LocationName as ParentLocationName,
                   (SELECT COUNT(*) FROM Location WHERE ParentLocationId = l.LocationId) as ChildCount
            FROM Location l
            LEFT JOIN Location p ON l.ParentLocationId = p.LocationId
            ORDER BY l.LocationName";

        return await _context.QueryAsync<LocationEntity>(sql);
    }

    public async Task<IEnumerable<LocationEntity>> GetActiveOnlyWithRelationsAsync()
    {
        const string sql = @"
            SELECT l.*, p.LocationName as ParentLocationName
            FROM Location l
            LEFT JOIN Location p ON l.ParentLocationId = p.LocationId
            WHERE l.IsActive = 1
            ORDER BY l.LocationName";

        return await _context.QueryAsync<LocationEntity>(sql);
    }

    public async Task<IEnumerable<LocationEntity>> GetSubLocationAsync(int parentLocationId)
    {
        const string sql = "SELECT * FROM Location WHERE ParentLocationId = @ParentLocationId AND IsActive = 1 ORDER BY LocationName";
        return await _context.QueryAsync<LocationEntity>(sql, new { ParentLocationId = parentLocationId });
    }

    public async Task<bool> IsLocationCodeExistsAsync(string locationCode, int? excludeLocationId = null)
    {
        var sql = "SELECT COUNT(1) FROM Location WHERE LocationCode = @LocationCode";
        var parameters = new DynamicParameters();
        parameters.Add("@LocationCode", locationCode);

        if (excludeLocationId.HasValue)
        {
            sql += " AND LocationId != @ExcludeLocationId";
            parameters.Add("@ExcludeLocationId", excludeLocationId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<bool> IsLocationNameExistsAsync(string locationName, int? excludeLocationId = null)
    {
        var sql = "SELECT COUNT(1) FROM Location WHERE LocationName = @LocationName";
        var parameters = new DynamicParameters();
        parameters.Add("@LocationName", locationName);

        if (excludeLocationId.HasValue)
        {
            sql += " AND LocationId != @ExcludeLocationId";
            parameters.Add("@ExcludeLocationId", excludeLocationId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<int> GetChildCountAsync(int locationId)
    {
        const string sql = "SELECT COUNT(*) FROM Location WHERE ParentLocationId = @LocationId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { LocationId = locationId });
    }
}