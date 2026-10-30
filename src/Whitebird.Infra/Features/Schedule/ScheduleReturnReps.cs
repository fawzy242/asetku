using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Schedule;

public class ScheduleReturnReps : IScheduleReturnReps
{
    private readonly DapperContext _context;

    public ScheduleReturnReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<ScheduleReturnEntity?> GetByIdRawAsync(int id)
    {
        const string sql = "SELECT * FROM ScheduleReturn WHERE ScheduleReturnId = @Id";
        return await _context.QueryFirstOrDefaultAsync<ScheduleReturnEntity>(sql, new { Id = id });
    }

    public async Task<ScheduleReturnView?> GetDetailByIdAsync(int id)
    {
        const string sql = @"
            SELECT 
                sr.ScheduleReturnId, sr.ScheduleDate, sr.AssetId, sr.EmployeeId,
                sr.ReturnAssetTransactionId, sr.IsActive, sr.CreatedDate, sr.CreatedBy,
                sr.ModifiedDate, sr.ModifiedBy,
                a.AssetCode, a.AssetName,
                e.EmployeeCode, e.FullName as EmployeeName,
                at.AssetTransactionCode as TransactionCode
            FROM ScheduleReturn sr
            LEFT JOIN Asset a ON sr.AssetId = a.AssetId
            LEFT JOIN Employee e ON sr.EmployeeId = e.EmployeeId
            LEFT JOIN AssetTransaction at ON sr.ReturnAssetTransactionId = at.AssetTransactionId
            WHERE sr.ScheduleReturnId = @Id";

        return await _context.QueryFirstOrDefaultAsync<ScheduleReturnView>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ScheduleReturnListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                sr.ScheduleReturnId, sr.ScheduleDate, sr.AssetId, sr.EmployeeId,
                sr.ReturnAssetTransactionId, sr.IsActive, sr.CreatedDate, sr.CreatedBy,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName
            FROM ScheduleReturn sr
            LEFT JOIN Asset a ON sr.AssetId = a.AssetId
            LEFT JOIN Employee e ON sr.EmployeeId = e.EmployeeId
            WHERE sr.IsActive = 1
            ORDER BY sr.ScheduleDate DESC";

        return await _context.QueryAsync<ScheduleReturnListView>(sql);
    }

    public async Task<IEnumerable<ScheduleReturnListView>> GetByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT 
                sr.ScheduleReturnId, sr.ScheduleDate, sr.AssetId, sr.EmployeeId,
                sr.ReturnAssetTransactionId, sr.IsActive, sr.CreatedDate, sr.CreatedBy,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName
            FROM ScheduleReturn sr
            LEFT JOIN Asset a ON sr.AssetId = a.AssetId
            LEFT JOIN Employee e ON sr.EmployeeId = e.EmployeeId
            WHERE sr.AssetId = @AssetId AND sr.IsActive = 1
            ORDER BY sr.ScheduleDate DESC";

        return await _context.QueryAsync<ScheduleReturnListView>(sql, new { AssetId = assetId });
    }

    public async Task<IEnumerable<ScheduleReturnListView>> GetByEmployeeIdAsync(int employeeId)
    {
        const string sql = @"
            SELECT 
                sr.ScheduleReturnId, sr.ScheduleDate, sr.AssetId, sr.EmployeeId,
                sr.ReturnAssetTransactionId, sr.IsActive, sr.CreatedDate, sr.CreatedBy,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName
            FROM ScheduleReturn sr
            LEFT JOIN Asset a ON sr.AssetId = a.AssetId
            LEFT JOIN Employee e ON sr.EmployeeId = e.EmployeeId
            WHERE sr.EmployeeId = @EmployeeId AND sr.IsActive = 1
            ORDER BY sr.ScheduleDate DESC";

        return await _context.QueryAsync<ScheduleReturnListView>(sql, new { EmployeeId = employeeId });
    }

    public async Task<bool> IsScheduleExistsAsync(DateTime scheduleDate, int assetId, int? excludeId = null)
    {
        var sql = "SELECT COUNT(1) FROM ScheduleReturn WHERE ScheduleDate = @ScheduleDate AND AssetId = @AssetId AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@ScheduleDate", scheduleDate.Date);
        parameters.Add("@AssetId", assetId);

        if (excludeId.HasValue)
        {
            sql += " AND ScheduleReturnId != @ExcludeId";
            parameters.Add("@ExcludeId", excludeId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<IEnumerable<DateTime>> GetExistingDatesAsync(DateTime startDate, DateTime endDate, int assetId)
    {
        const string sql = @"
            SELECT DISTINCT CAST(ScheduleDate AS DATE) 
            FROM ScheduleReturn 
            WHERE ScheduleDate BETWEEN @StartDate AND @EndDate 
              AND AssetId = @AssetId 
              AND IsActive = 1";

        var result = await _context.QueryAsync<DateTime>(sql, new { StartDate = startDate.Date, EndDate = endDate.Date, AssetId = assetId });
        return result;
    }

    public async Task<int> InsertAsync(ScheduleReturnEntity entity)
    {
        const string sql = @"
            INSERT INTO ScheduleReturn (
                ScheduleDate, AssetId, EmployeeId, ReturnAssetTransactionId,
                IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @ScheduleDate, @AssetId, @EmployeeId, @ReturnAssetTransactionId,
                @IsActive, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<int> UpdateAsync(ScheduleReturnEntity entity)
    {
        const string sql = @"
            UPDATE ScheduleReturn SET
                ScheduleDate = @ScheduleDate,
                AssetId = @AssetId,
                EmployeeId = @EmployeeId,
                ReturnAssetTransactionId = @ReturnAssetTransactionId,
                IsActive = @IsActive,
                ModifiedDate = GETDATE(),
                ModifiedBy = @ModifiedBy
            WHERE ScheduleReturnId = @ScheduleReturnId";

        return await _context.ExecuteAsync(sql, entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM ScheduleReturn WHERE ScheduleReturnId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<int> SoftDeleteAsync(int id)
    {
        const string sql = "UPDATE ScheduleReturn SET IsActive = 0, ModifiedDate = GETDATE() WHERE ScheduleReturnId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = id });
    }

    // ============================================================
    // PAGINATION
    // ============================================================

    public async Task<PaginatedResult<ScheduleReturnListView>> GetPagedListAsync(ScheduleReturnGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("sr.IsActive = 1");

        if (request.IsActive.HasValue)
        {
            conditions.RemoveAt(conditions.Count - 1);
            conditions.Add($"sr.IsActive = {(request.IsActive.Value ? 1 : 0)}");
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "a.AssetCode", "a.AssetName", "e.FullName", "e.EmployeeCode" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "sr.AssetId", request.AssetId);
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "sr.EmployeeId", request.EmployeeId);
        GridQueryBuilder.AddDateRangeFilter(conditions, parameters, "sr.ScheduleDate", request.DateFrom, request.DateTo);

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "sr.ScheduleDate", true);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*) 
            FROM ScheduleReturn sr
            LEFT JOIN Asset a ON sr.AssetId = a.AssetId
            LEFT JOIN Employee e ON sr.EmployeeId = e.EmployeeId
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                sr.ScheduleReturnId, sr.ScheduleDate, sr.AssetId, sr.EmployeeId,
                sr.ReturnAssetTransactionId, sr.IsActive, sr.CreatedDate, sr.CreatedBy,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName,
                1 as CanEdit,
                1 as CanDelete";

        var fromClause = @"
            FROM ScheduleReturn sr
            LEFT JOIN Asset a ON sr.AssetId = a.AssetId
            LEFT JOIN Employee e ON sr.EmployeeId = e.EmployeeId";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<ScheduleReturnListView>(dataSql, parameters);

        return new PaginatedResult<ScheduleReturnListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}