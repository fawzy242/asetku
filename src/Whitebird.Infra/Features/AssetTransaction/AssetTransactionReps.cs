using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.AssetTransaction.Entities;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTransaction;

public class AssetTransactionReps : IAssetTransactionReps
{
    private readonly DapperContext _context;

    public AssetTransactionReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<AssetTransactionEntity?> GetByIdRawAsync(int transactionId)
    {
        const string sql = "SELECT * FROM AssetTransaction WHERE AssetTransactionId = @TransactionId";
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { TransactionId = transactionId });
    }

    public async Task<AssetTransactionEntity?> GetByIdWithRelationsAsync(int transactionId)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   fl.LocationName as FromLocationName, tl.LocationName as ToLocationName, 
                   ap.FullName as ApprovedByName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Location fl ON t.FromLocationId = fl.LocationId
            LEFT JOIN Location tl ON t.ToLocationId = tl.LocationId
            LEFT JOIN Employee ap ON t.ApprovedBy = ap.EmployeeId
            WHERE t.AssetTransactionId = @TransactionId";

        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { TransactionId = transactionId });
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetAllWithRelationsAsync()
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<AssetTransactionEntity>(sql);
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetByAssetIdWithRelationsAsync(int assetId)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            WHERE t.AssetId = @AssetId
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<AssetTransactionEntity>(sql, new { AssetId = assetId });
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetByEmployeeIdWithRelationsAsync(int employeeId)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            WHERE t.FromEmployeeId = @EmployeeId OR t.ToEmployeeId = @EmployeeId
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<AssetTransactionEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetByStatusWithRelationsAsync(string status)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            WHERE t.TransactionStatus = @Status
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<AssetTransactionEntity>(sql, new { Status = status });
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetPendingApprovalsWithRelationsAsync()
        => await GetByStatusWithRelationsAsync("Pending");

    public async Task<IEnumerable<AssetTransactionEntity>> GetByDateRangeWithRelationsAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            WHERE t.TransactionDate BETWEEN @StartDate AND @EndDate
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<AssetTransactionEntity>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<AssetTransactionEntity?> GetActiveTransactionByAssetIdWithRelationsAsync(int assetId)
    {
        const string sql = @"
            SELECT TOP 1 t.*, a.AssetCode, a.AssetName, fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            WHERE t.AssetId = @AssetId AND t.TransactionStatus = 'Approved' AND t.ActualReturnDate IS NULL
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { AssetId = assetId });
    }

    public async Task<int> GetTransactionCountByAssetAsync(int assetId)
    {
        const string sql = "SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = @AssetId";
        return await _context.ExecuteScalarAsync<int>(sql, new { AssetId = assetId });
    }

    public async Task<PaginatedResult<AssetTransactionEntity>> GetPagedWithRelationsAsync(int page, int pageSize, string? search = null, string? status = null, int? assetId = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR fe.FullName LIKE @Search OR te.FullName LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            conditions.Add("t.TransactionStatus = @Status");
            parameters.Add("@Status", status);
        }

        if (assetId.HasValue)
        {
            conditions.Add("t.AssetId = @AssetId");
            parameters.Add("@AssetId", assetId.Value);
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";

        var countSql = $@"
            SELECT COUNT(*) FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            {whereClause}";

        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT t.*, a.AssetCode, a.AssetName, fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            {whereClause}
            ORDER BY t.TransactionDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<AssetTransactionEntity>(dataSql, parameters);

        return new PaginatedResult<AssetTransactionEntity>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}