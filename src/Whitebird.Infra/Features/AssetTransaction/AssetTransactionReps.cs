using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.AssetTransaction;
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
        const string sql = "SELECT * FROM AssetTransaction WHERE AssetTransactionId = @TransactionId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { TransactionId = transactionId });
    }

    public async Task<AssetTransactionEntity?> GetByIdWithRelationsAsync(int transactionId)
    {
        const string sql = @"
            SELECT t.*, 
                   a.AssetCode, a.AssetName,
                   fe.FullName as FromEmployeeName, 
                   te.FullName as ToEmployeeName,
                   tl.OfficeName as ToLocationName,
                   md1.MasterDataName as TransactionTypeName,
                   md2.MasterDataName as ConditionBeforeName,
                   md3.MasterDataName as ConditionAfterName,
                   md4.MasterDataName as MaintenanceTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId AND tl.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition' AND md3.IsActive = 1
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType' AND md4.IsActive = 1
            WHERE t.AssetTransactionId = @TransactionId AND t.IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { TransactionId = transactionId });
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetAllWithRelationsAsync()
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        return await _context.QueryAsync<AssetTransactionEntity>(sql);
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetByAssetIdWithRelationsAsync(int assetId)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.AssetId = @AssetId AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        return await _context.QueryAsync<AssetTransactionEntity>(sql, new { AssetId = assetId });
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetByEmployeeIdWithRelationsAsync(int employeeId)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE (t.FromEmployeeId = @EmployeeId OR t.ToEmployeeId = @EmployeeId) AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        return await _context.QueryAsync<AssetTransactionEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetByApprovalStatusAsync(bool? approved)
    {
        var sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.IsActive = 1";

        if (approved.HasValue)
            sql += " AND t.Approved = @Approved";

        sql += " ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<AssetTransactionEntity>(sql, new { Approved = approved });
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetPendingApprovalsWithRelationsAsync()
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.Approved IS NULL AND t.IsActive = 1
            ORDER BY t.TransactionDate ASC";
        return await _context.QueryAsync<AssetTransactionEntity>(sql);
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetByDateRangeWithRelationsAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.TransactionDate BETWEEN @StartDate AND @EndDate AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        return await _context.QueryAsync<AssetTransactionEntity>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<AssetTransactionEntity?> GetActiveTransactionByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT TOP 1 t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.AssetId = @AssetId 
              AND t.Approved = 1 
              AND t.FromAssetTransactionId IS NULL
              AND t.TransactionType IN (1, 2, 3, 6)
              AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { AssetId = assetId });
    }

    public async Task<int> GetTransactionCountByAssetAsync(int assetId)
    {
        const string sql = "SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = @AssetId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { AssetId = assetId });
    }

    public async Task<PaginatedResult<AssetTransactionEntity>> GetPagedWithRelationsAsync(
        int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null)
    {
        var conditions = new List<string> { "t.IsActive = 1" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR fe.FullName LIKE @Search OR te.FullName LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (approved.HasValue)
        {
            conditions.Add("t.Approved = @Approved");
            parameters.Add("@Approved", approved.Value);
        }

        if (assetId.HasValue)
        {
            conditions.Add("t.AssetId = @AssetId");
            parameters.Add("@AssetId", assetId.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var countSql = $@"
            SELECT COUNT(*) FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
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

    public async Task<AssetTransactionEntity?> GetPairedTransactionAsync(int transactionId)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.FromAssetTransactionId = @TransactionId AND t.IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { TransactionId = transactionId });
    }

    public async Task<bool> HasOpenPairedTransactionAsync(int assetId, int transactionType)
    {
        var pairingTypes = new[] { 3, 6 };
        if (!pairingTypes.Contains(transactionType))
            return false;

        const string sql = @"
            SELECT COUNT(1) FROM AssetTransaction
            WHERE AssetId = @AssetId
              AND TransactionType = @TransactionType
              AND Approved = 1
              AND FromAssetTransactionId IS NULL
              AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { AssetId = assetId, TransactionType = transactionType }) > 0;
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetActiveLoansWithRelationsAsync()
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.TransactionType = 3
              AND t.Approved = 1
              AND t.FromAssetTransactionId IS NULL
              AND t.IsActive = 1
            ORDER BY t.ExpectedReturnDate";
        return await _context.QueryAsync<AssetTransactionEntity>(sql);
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetOverdueLoansWithRelationsAsync()
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId AND fe.IsActive = 1
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId AND te.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.TransactionType = 3
              AND t.Approved = 1
              AND t.FromAssetTransactionId IS NULL
              AND t.ExpectedReturnDate < GETDATE()
              AND t.IsActive = 1
            ORDER BY t.ExpectedReturnDate";
        return await _context.QueryAsync<AssetTransactionEntity>(sql);
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetAssetTransactionHistoryAsync(int assetId)
    {
        return await GetByAssetIdWithRelationsAsync(assetId);
    }

    public async Task<IEnumerable<AssetTransactionEntity>> GetEmployeeTransactionHistoryAsync(int employeeId)
    {
        return await GetByEmployeeIdWithRelationsAsync(employeeId);
    }
}