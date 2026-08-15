using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Schedule;

public class ScheduleHandoverReps : IScheduleHandoverReps
{
    private readonly DapperContext _context;

    public ScheduleHandoverReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<ScheduleHandoverEntity?> GetByIdRawAsync(int id)
    {
        const string sql = "SELECT * FROM ScheduleHandover WHERE ScheduleHandoverId = @Id";
        return await _context.QueryFirstOrDefaultAsync<ScheduleHandoverEntity>(sql, new { Id = id });
    }

    public async Task<ScheduleHandoverView?> GetDetailByIdAsync(int id)
    {
        const string sql = @"
            SELECT 
                sh.ScheduleHandoverId, sh.ScheduleDate, sh.AssetId, sh.EmployeeId,
                sh.HandoverAssetTransactionId, sh.IsActive, sh.CreatedDate, sh.CreatedBy,
                sh.ModifiedDate, sh.ModifiedBy,
                a.AssetCode, a.AssetName,
                e.EmployeeCode, e.FullName as EmployeeName,
                at.AssetTransactionCode as TransactionCode
            FROM ScheduleHandover sh
            LEFT JOIN Asset a ON sh.AssetId = a.AssetId
            LEFT JOIN Employee e ON sh.EmployeeId = e.EmployeeId
            LEFT JOIN AssetTransaction at ON sh.HandoverAssetTransactionId = at.AssetTransactionId
            WHERE sh.ScheduleHandoverId = @Id";

        return await _context.QueryFirstOrDefaultAsync<ScheduleHandoverView>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ScheduleHandoverListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                sh.ScheduleHandoverId, sh.ScheduleDate, sh.AssetId, sh.EmployeeId,
                sh.HandoverAssetTransactionId, sh.IsActive, sh.CreatedDate, sh.CreatedBy,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName
            FROM ScheduleHandover sh
            LEFT JOIN Asset a ON sh.AssetId = a.AssetId
            LEFT JOIN Employee e ON sh.EmployeeId = e.EmployeeId
            WHERE sh.IsActive = 1
            ORDER BY sh.ScheduleDate DESC";

        return await _context.QueryAsync<ScheduleHandoverListView>(sql);
    }

    public async Task<IEnumerable<ScheduleHandoverListView>> GetByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT 
                sh.ScheduleHandoverId, sh.ScheduleDate, sh.AssetId, sh.EmployeeId,
                sh.HandoverAssetTransactionId, sh.IsActive, sh.CreatedDate, sh.CreatedBy,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName
            FROM ScheduleHandover sh
            LEFT JOIN Asset a ON sh.AssetId = a.AssetId
            LEFT JOIN Employee e ON sh.EmployeeId = e.EmployeeId
            WHERE sh.AssetId = @AssetId AND sh.IsActive = 1
            ORDER BY sh.ScheduleDate DESC";

        return await _context.QueryAsync<ScheduleHandoverListView>(sql, new { AssetId = assetId });
    }

    public async Task<IEnumerable<ScheduleHandoverListView>> GetByEmployeeIdAsync(int employeeId)
    {
        const string sql = @"
            SELECT 
                sh.ScheduleHandoverId, sh.ScheduleDate, sh.AssetId, sh.EmployeeId,
                sh.HandoverAssetTransactionId, sh.IsActive, sh.CreatedDate, sh.CreatedBy,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName
            FROM ScheduleHandover sh
            LEFT JOIN Asset a ON sh.AssetId = a.AssetId
            LEFT JOIN Employee e ON sh.EmployeeId = e.EmployeeId
            WHERE sh.EmployeeId = @EmployeeId AND sh.IsActive = 1
            ORDER BY sh.ScheduleDate DESC";

        return await _context.QueryAsync<ScheduleHandoverListView>(sql, new { EmployeeId = employeeId });
    }

    public async Task<bool> IsScheduleExistsAsync(DateTime scheduleDate, int assetId, int? excludeId = null)
    {
        var sql = "SELECT COUNT(1) FROM ScheduleHandover WHERE ScheduleDate = @ScheduleDate AND AssetId = @AssetId AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@ScheduleDate", scheduleDate.Date);
        parameters.Add("@AssetId", assetId);

        if (excludeId.HasValue)
        {
            sql += " AND ScheduleHandoverId != @ExcludeId";
            parameters.Add("@ExcludeId", excludeId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<IEnumerable<DateTime>> GetExistingDatesAsync(DateTime startDate, DateTime endDate, int assetId)
    {
        const string sql = @"
            SELECT DISTINCT CAST(ScheduleDate AS DATE) 
            FROM ScheduleHandover 
            WHERE ScheduleDate BETWEEN @StartDate AND @EndDate 
              AND AssetId = @AssetId 
              AND IsActive = 1";

        var result = await _context.QueryAsync<DateTime>(sql, new { StartDate = startDate.Date, EndDate = endDate.Date, AssetId = assetId });
        return result;
    }

    public async Task<int> InsertAsync(ScheduleHandoverEntity entity)
    {
        const string sql = @"
            INSERT INTO ScheduleHandover (
                ScheduleDate, AssetId, EmployeeId, HandoverAssetTransactionId,
                IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @ScheduleDate, @AssetId, @EmployeeId, @HandoverAssetTransactionId,
                @IsActive, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<int> UpdateAsync(ScheduleHandoverEntity entity)
    {
        const string sql = @"
            UPDATE ScheduleHandover SET
                ScheduleDate = @ScheduleDate,
                AssetId = @AssetId,
                EmployeeId = @EmployeeId,
                HandoverAssetTransactionId = @HandoverAssetTransactionId,
                IsActive = @IsActive,
                ModifiedDate = GETDATE(),
                ModifiedBy = @ModifiedBy
            WHERE ScheduleHandoverId = @ScheduleHandoverId";

        return await _context.ExecuteAsync(sql, entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM ScheduleHandover WHERE ScheduleHandoverId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<int> SoftDeleteAsync(int id)
    {
        const string sql = "UPDATE ScheduleHandover SET IsActive = 0, ModifiedDate = GETDATE() WHERE ScheduleHandoverId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = id });
    }

    // ============================================================
    // PAGINATION
    // ============================================================

    public async Task<PaginatedResult<ScheduleHandoverListView>> GetPagedListAsync(ScheduleGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("sh.IsActive = 1");

        if (request.IsActive.HasValue)
        {
            conditions.RemoveAt(conditions.Count - 1);
            conditions.Add($"sh.IsActive = {(request.IsActive.Value ? 1 : 0)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "a.AssetCode", "a.AssetName", "e.FullName", "e.EmployeeCode" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "sh.AssetId", request.AssetId);
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "sh.EmployeeId", request.EmployeeId);
        GridQueryBuilder.AddDateRangeFilter(conditions, parameters, "sh.ScheduleDate", request.DateFrom, request.DateTo);

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "sh.ScheduleDate", true);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*) 
            FROM ScheduleHandover sh
            LEFT JOIN Asset a ON sh.AssetId = a.AssetId
            LEFT JOIN Employee e ON sh.EmployeeId = e.EmployeeId
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                sh.ScheduleHandoverId, sh.ScheduleDate, sh.AssetId, sh.EmployeeId,
                sh.HandoverAssetTransactionId, sh.IsActive, sh.CreatedDate, sh.CreatedBy,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName,
                1 as CanEdit,
                1 as CanDelete";

        var fromClause = @"
            FROM ScheduleHandover sh
            LEFT JOIN Asset a ON sh.AssetId = a.AssetId
            LEFT JOIN Employee e ON sh.EmployeeId = e.EmployeeId";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<ScheduleHandoverListView>(dataSql, parameters);

        return new PaginatedResult<ScheduleHandoverListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}