using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Schedule;

public class ScheduleMaintenanceReps : IScheduleMaintenanceReps
{
    private readonly DapperContext _context;

    public ScheduleMaintenanceReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<ScheduleMaintenanceEntity?> GetByIdRawAsync(int id)
    {
        const string sql = "SELECT * FROM ScheduleMaintenance WHERE ScheduleMaintenanceId = @Id";
        return await _context.QueryFirstOrDefaultAsync<ScheduleMaintenanceEntity>(sql, new { Id = id });
    }

    public async Task<ScheduleMaintenanceView?> GetDetailByIdAsync(int id)
    {
        const string sql = @"
            SELECT 
                sm.ScheduleMaintenanceId, sm.ScheduleDate, sm.AssetId, 
                sm.MaintenanceType, sm.MaintenanceAssetTransactionId, sm.PostAssetTransactionId,
                sm.IsActive, sm.CreatedDate, sm.CreatedBy, sm.ModifiedDate, sm.ModifiedBy,
                a.AssetCode, a.AssetName,
                md.MasterDataName as MaintenanceTypeName
            FROM ScheduleMaintenance sm
            LEFT JOIN Asset a ON sm.AssetId = a.AssetId
            LEFT JOIN MasterData md ON sm.MaintenanceType = md.ReferenceCode AND md.ReferenceName = 'MaintenanceType' AND md.IsActive = 1
            WHERE sm.ScheduleMaintenanceId = @Id";
        
        return await _context.QueryFirstOrDefaultAsync<ScheduleMaintenanceView>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ScheduleMaintenanceListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                sm.ScheduleMaintenanceId, sm.ScheduleDate, sm.AssetId, 
                sm.MaintenanceType, sm.MaintenanceAssetTransactionId, sm.PostAssetTransactionId,
                sm.IsActive, sm.CreatedDate, sm.CreatedBy,
                a.AssetCode, a.AssetName,
                md.MasterDataName as MaintenanceTypeName
            FROM ScheduleMaintenance sm
            LEFT JOIN Asset a ON sm.AssetId = a.AssetId
            LEFT JOIN MasterData md ON sm.MaintenanceType = md.ReferenceCode AND md.ReferenceName = 'MaintenanceType' AND md.IsActive = 1
            WHERE sm.IsActive = 1
            ORDER BY sm.ScheduleDate DESC";
        
        return await _context.QueryAsync<ScheduleMaintenanceListView>(sql);
    }

    public async Task<IEnumerable<ScheduleMaintenanceListView>> GetByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT 
                sm.ScheduleMaintenanceId, sm.ScheduleDate, sm.AssetId, 
                sm.MaintenanceType, sm.MaintenanceAssetTransactionId, sm.PostAssetTransactionId,
                sm.IsActive, sm.CreatedDate, sm.CreatedBy,
                a.AssetCode, a.AssetName,
                md.MasterDataName as MaintenanceTypeName
            FROM ScheduleMaintenance sm
            LEFT JOIN Asset a ON sm.AssetId = a.AssetId
            LEFT JOIN MasterData md ON sm.MaintenanceType = md.ReferenceCode AND md.ReferenceName = 'MaintenanceType' AND md.IsActive = 1
            WHERE sm.AssetId = @AssetId AND sm.IsActive = 1
            ORDER BY sm.ScheduleDate DESC";
        
        return await _context.QueryAsync<ScheduleMaintenanceListView>(sql, new { AssetId = assetId });
    }

    public async Task<IEnumerable<ScheduleMaintenanceListView>> GetByMaintenanceTypeAsync(int maintenanceType)
    {
        const string sql = @"
            SELECT 
                sm.ScheduleMaintenanceId, sm.ScheduleDate, sm.AssetId, 
                sm.MaintenanceType, sm.MaintenanceAssetTransactionId, sm.PostAssetTransactionId,
                sm.IsActive, sm.CreatedDate, sm.CreatedBy,
                a.AssetCode, a.AssetName,
                md.MasterDataName as MaintenanceTypeName
            FROM ScheduleMaintenance sm
            LEFT JOIN Asset a ON sm.AssetId = a.AssetId
            LEFT JOIN MasterData md ON sm.MaintenanceType = md.ReferenceCode AND md.ReferenceName = 'MaintenanceType' AND md.IsActive = 1
            WHERE sm.MaintenanceType = @MaintenanceType AND sm.IsActive = 1
            ORDER BY sm.ScheduleDate DESC";
        
        return await _context.QueryAsync<ScheduleMaintenanceListView>(sql, new { MaintenanceType = maintenanceType });
    }

    public async Task<IEnumerable<ScheduleMaintenanceListView>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT 
                sm.ScheduleMaintenanceId, sm.ScheduleDate, sm.AssetId, 
                sm.MaintenanceType, sm.MaintenanceAssetTransactionId, sm.PostAssetTransactionId,
                sm.IsActive, sm.CreatedDate, sm.CreatedBy,
                a.AssetCode, a.AssetName,
                md.MasterDataName as MaintenanceTypeName
            FROM ScheduleMaintenance sm
            LEFT JOIN Asset a ON sm.AssetId = a.AssetId
            LEFT JOIN MasterData md ON sm.MaintenanceType = md.ReferenceCode AND md.ReferenceName = 'MaintenanceType' AND md.IsActive = 1
            WHERE sm.ScheduleDate BETWEEN @StartDate AND @EndDate AND sm.IsActive = 1
            ORDER BY sm.ScheduleDate ASC";
        
        return await _context.QueryAsync<ScheduleMaintenanceListView>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<bool> IsScheduleExistsAsync(DateTime scheduleDate, int assetId, int? excludeId = null)
    {
        var sql = "SELECT COUNT(1) FROM ScheduleMaintenance WHERE ScheduleDate = @ScheduleDate AND AssetId = @AssetId AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@ScheduleDate", scheduleDate.Date);
        parameters.Add("@AssetId", assetId);

        if (excludeId.HasValue)
        {
            sql += " AND ScheduleMaintenanceId != @ExcludeId";
            parameters.Add("@ExcludeId", excludeId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<IEnumerable<DateTime>> GetExistingDatesAsync(DateTime startDate, DateTime endDate, int assetId)
    {
        const string sql = @"
            SELECT DISTINCT CAST(ScheduleDate AS DATE) 
            FROM ScheduleMaintenance 
            WHERE ScheduleDate BETWEEN @StartDate AND @EndDate 
              AND AssetId = @AssetId 
              AND IsActive = 1";
        
        var result = await _context.QueryAsync<DateTime>(sql, new { StartDate = startDate.Date, EndDate = endDate.Date, AssetId = assetId });
        return result;
    }

    public async Task<int> InsertAsync(ScheduleMaintenanceEntity entity)
    {
        const string sql = @"
            INSERT INTO ScheduleMaintenance (
                ScheduleDate, AssetId, MaintenanceType, 
                MaintenanceAssetTransactionId, PostAssetTransactionId,
                IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @ScheduleDate, @AssetId, @MaintenanceType,
                @MaintenanceAssetTransactionId, @PostAssetTransactionId,
                @IsActive, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        
        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<int> UpdateAsync(ScheduleMaintenanceEntity entity)
    {
        const string sql = @"
            UPDATE ScheduleMaintenance SET
                ScheduleDate = @ScheduleDate,
                AssetId = @AssetId,
                MaintenanceType = @MaintenanceType,
                MaintenanceAssetTransactionId = @MaintenanceAssetTransactionId,
                PostAssetTransactionId = @PostAssetTransactionId,
                IsActive = @IsActive,
                ModifiedDate = GETDATE(),
                ModifiedBy = @ModifiedBy
            WHERE ScheduleMaintenanceId = @ScheduleMaintenanceId";
        
        return await _context.ExecuteAsync(sql, entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM ScheduleMaintenance WHERE ScheduleMaintenanceId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<int> SoftDeleteAsync(int id)
    {
        const string sql = "UPDATE ScheduleMaintenance SET IsActive = 0, ModifiedDate = GETDATE() WHERE ScheduleMaintenanceId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<PaginatedResult<ScheduleMaintenanceListView>> GetPagedListAsync(ScheduleGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("sm.IsActive = 1");

        if (request.IsActive.HasValue)
        {
            conditions.RemoveAt(conditions.Count - 1);
            conditions.Add($"sm.IsActive = {(request.IsActive.Value ? 1 : 0)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "a.AssetCode", "a.AssetName", "md.MasterDataName" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "sm.AssetId", request.AssetId);
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "sm.MaintenanceType", request.MaintenanceType);
        GridQueryBuilder.AddDateRangeFilter(conditions, parameters, "sm.ScheduleDate", request.DateFrom, request.DateTo);

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "sm.ScheduleDate", true);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*) 
            FROM ScheduleMaintenance sm
            LEFT JOIN Asset a ON sm.AssetId = a.AssetId
            LEFT JOIN MasterData md ON sm.MaintenanceType = md.ReferenceCode AND md.ReferenceName = 'MaintenanceType' AND md.IsActive = 1
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                sm.ScheduleMaintenanceId, sm.ScheduleDate, sm.AssetId, 
                sm.MaintenanceType, sm.MaintenanceAssetTransactionId, sm.PostAssetTransactionId,
                sm.IsActive, sm.CreatedDate, sm.CreatedBy,
                a.AssetCode, a.AssetName,
                md.MasterDataName as MaintenanceTypeName";

        var fromClause = @"
            FROM ScheduleMaintenance sm
            LEFT JOIN Asset a ON sm.AssetId = a.AssetId
            LEFT JOIN MasterData md ON sm.MaintenanceType = md.ReferenceCode AND md.ReferenceName = 'MaintenanceType' AND md.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<ScheduleMaintenanceListView>(dataSql, parameters);

        return new PaginatedResult<ScheduleMaintenanceListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}