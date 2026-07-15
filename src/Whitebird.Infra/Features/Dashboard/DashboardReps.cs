using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Dashboard;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Dashboard;

/// <summary>
/// Repository implementation for Dashboard operations using Dapper
/// </summary>
public class DashboardReps : IDashboardReps
{
    private readonly DapperContext _context;

    public DashboardReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // MAIN STATS
    // ============================================================

    public async Task<DashboardStatsViewModel> GetStatsAsync()
    {
        const string sql = @"
        WITH ActiveTransactions AS (
            SELECT 
                AssetId,
                TransactionType,
                ToEmployeeId,
                ExpectedLoanReturnDate,
                ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
            FROM AssetTransaction
            WHERE Approved = 1
              AND FromAssetTransactionId IS NULL
              AND IsActive = 1
        )
        SELECT 
            (SELECT COUNT(*) FROM Asset WHERE IsActive = 1) AS TotalAssets,
            (SELECT COUNT(*) FROM Asset a WHERE a.IsActive = 1 AND NOT EXISTS (
                SELECT 1 FROM ActiveTransactions at WHERE at.AssetId = a.AssetId AND at.rn = 1
            )) AS AvailableAssets,
            (SELECT COUNT(*) FROM ActiveTransactions at WHERE at.TransactionType IN (@HandoverType, @TransferType) AND at.rn = 1) AS AssignedAssets,
            (SELECT COUNT(*) FROM ActiveTransactions at WHERE at.TransactionType = @LoanType AND at.rn = 1) AS AssetsOnLoan,
            (SELECT COUNT(*) FROM ActiveTransactions at WHERE at.TransactionType = @MaintenanceType AND at.rn = 1) AS AssetsInMaintenance,
            (SELECT COUNT(DISTINCT AssetId) FROM AssetTransaction WHERE TransactionType = @DisposalType AND Approved = 1 AND IsActive = 1) AS DisposedAssets,
            (SELECT COUNT(*) FROM (
                SELECT AssetId FROM Asset WHERE IsActive = 0
                UNION
                SELECT AssetId FROM AssetTransaction WHERE TransactionType = @DisposalType AND Approved = 1 AND IsActive = 1
            ) AS DamagedUnion) AS DamagedAssets,
            (SELECT COUNT(*) FROM Asset WHERE WarrantyExpiryDate < GETDATE() AND WarrantyExpiryDate IS NOT NULL AND IsActive = 1) AS ExpiredWarrantyCount,
            (SELECT COUNT(*) FROM ActiveTransactions at WHERE at.TransactionType = @LoanType AND at.ExpectedLoanReturnDate < GETDATE() AND at.rn = 1) AS OverdueLoanCount,
            ISNULL((SELECT SUM(PurchasePrice) FROM Asset WHERE IsActive = 1), 0) AS TotalAssetValue,
            (SELECT COUNT(*) FROM Employee WHERE IsActive = 1) AS TotalEmployees,
            (SELECT COUNT(*) FROM Employee WHERE IsActive = 1 AND EmploymentStatus != @ResignedStatus) AS ActiveEmployees,
            (SELECT COUNT(*) FROM AssetTransaction WHERE Approved IS NULL AND IsActive = 1) AS PendingApprovals,
            (SELECT COUNT(*) FROM AssetTransaction WHERE Approved = 1 AND TransactionDate >= DATEADD(DAY, -30, GETDATE()) AND IsActive = 1) AS ApprovedTransactions,
            (SELECT COUNT(*) FROM AssetTransaction WHERE Approved = 0 AND TransactionDate >= DATEADD(DAY, -30, GETDATE()) AND IsActive = 1) AS RejectedTransactions,
            (SELECT COUNT(*) FROM AssetTransaction WHERE TransactionDate >= DATEADD(DAY, -30, GETDATE()) AND IsActive = 1) AS Last30DaysTransactions,
            (SELECT COUNT(*) FROM Office WHERE IsActive = 1) AS TotalOffices,
            (SELECT COUNT(*) FROM Department WHERE IsActive = 1) AS TotalDepartments";

        var parameters = new
        {
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            LoanType = TransactionTypeConstants.LOAN,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE,
            DisposalType = TransactionTypeConstants.DISPOSAL,
            ResignedStatus = EmployeeStatusConstants.RESIGNED
        };

        var result = await _context.QueryFirstOrDefaultAsync<DashboardStatsViewModel>(sql, parameters);
        return result ?? new DashboardStatsViewModel();
    }

    // ============================================================
    // CHARTS
    // ============================================================

    public async Task<IEnumerable<MonthlyStatDto>> GetMonthlyStatsAsync(int year)
    {
        const string sql = @"
            SELECT 
                MONTH(TransactionDate) as Month,
                COUNT(*) as TransactionCount,
                COUNT(DISTINCT AssetId) as UniqueAssetsCount
            FROM AssetTransaction
            WHERE YEAR(TransactionDate) = @Year
              AND IsActive = 1
            GROUP BY MONTH(TransactionDate)
            ORDER BY Month";

        return await _context.QueryAsync<MonthlyStatDto>(sql, new { Year = year });
    }

    public async Task<IEnumerable<CategoryBreakdownDto>> GetCategoryBreakdownAsync()
    {
        const string sql = @"
            SELECT 
                ISNULL(c.CategoryName, 'Uncategorized') as Category,
                COUNT(a.AssetId) as AssetCount,
                ISNULL(SUM(a.PurchasePrice), 0) as TotalValue
            FROM Category c
            LEFT JOIN Asset a ON c.CategoryId = a.CategoryId AND a.IsActive = 1
            WHERE c.IsActive = 1
            GROUP BY c.CategoryName
            ORDER BY AssetCount DESC";

        return await _context.QueryAsync<CategoryBreakdownDto>(sql);
    }

    // ============================================================
    // RECENT TRANSACTIONS
    // ============================================================

    public async Task<IEnumerable<RecentTransactionDto>> GetRecentTransactionsAsync(int limit)
    {
        const string sql = @"
        SELECT TOP (@Limit)
            t.AssetTransactionId,
            a.AssetCode,
            a.AssetName,
            t.TransactionType,
            md1.MasterDataName as TransactionTypeName,
            fe.FullName as FromEmployeeName,
            te.FullName as ToEmployeeName,
            t.TransactionDate,
            t.Approved,
            t.ExpectedLoanReturnDate,
            t.Notes
        FROM AssetTransaction t
        LEFT JOIN Asset a ON t.AssetId = a.AssetId
        LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
        LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
        LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
        WHERE t.IsActive = 1
        ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<RecentTransactionDto>(sql, new { Limit = limit });
    }

    // ============================================================
    // EXPIRED WARRANTY
    // ============================================================

    public async Task<IEnumerable<ExpiredWarrantyDto>> GetExpiredWarrantyAsync()
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, 
                c.CategoryName, a.PurchaseDate, a.WarrantyExpiryDate,
                s.SupplierName,
                CASE 
                    WHEN a.IsActive = 0 THEN 'Inactive'
                    WHEN lt.AssetId IS NULL THEN 'Available'
                    WHEN lt.TransactionType = @LoanType THEN 'On Loan'
                    WHEN lt.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN lt.TransactionType = @MaintenanceType THEN 'In Maintenance'
                    ELSE 'Available'
                END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND IsActive = 1 AND FromAssetTransactionId IS NULL
            ) lt ON a.AssetId = lt.AssetId AND lt.rn = 1
            WHERE a.WarrantyExpiryDate < GETDATE() AND a.WarrantyExpiryDate IS NOT NULL
            ORDER BY a.WarrantyExpiryDate ASC";

        var parameters = new
        {
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };

        return await _context.QueryAsync<ExpiredWarrantyDto>(sql, parameters);
    }

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    public async Task<ScheduleStatsDto> GetScheduleStatsAsync()
    {
        const string sql = @"
            SELECT 
                (SELECT COUNT(*) FROM ScheduleHandover WHERE IsActive = 1) AS TotalHandover,
                (SELECT COUNT(*) FROM ScheduleReturn WHERE IsActive = 1) AS TotalReturn,
                (SELECT COUNT(*) FROM ScheduleMaintenance WHERE IsActive = 1) AS TotalMaintenance,
                (SELECT COUNT(*) FROM ScheduleHandover WHERE ScheduleDate >= GETDATE() AND IsActive = 1) AS UpcomingHandover,
                (SELECT COUNT(*) FROM ScheduleReturn WHERE ScheduleDate >= GETDATE() AND IsActive = 1) AS UpcomingReturn,
                (SELECT COUNT(*) FROM ScheduleMaintenance WHERE ScheduleDate >= GETDATE() AND IsActive = 1) AS UpcomingMaintenance";

        return await _context.QueryFirstOrDefaultAsync<ScheduleStatsDto>(sql) ?? new ScheduleStatsDto();
    }

    // ============================================================
    // DETAIL STATS - PRIVATE HELPERS
    // ============================================================

    private async Task<PaginatedResult<AssetDetailDto>> GetAssetDetailListAsync(
        string baseWhere,
        Dictionary<string, object>? additionalFilters,
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        bool sortDescending)
    {
        var conditions = new List<string> { baseWhere };
        var parameters = new DynamicParameters();

        var loanType = TransactionTypeConstants.LOAN;
        var handoverType = TransactionTypeConstants.HANDOVER;
        var transferType = TransactionTypeConstants.TRANSFER;
        var maintenanceType = TransactionTypeConstants.MAINTENANCE;
        var disposalType = TransactionTypeConstants.DISPOSAL;

        parameters.Add("@LoanType", loanType);
        parameters.Add("@HandoverType", handoverType);
        parameters.Add("@TransferType", transferType);
        parameters.Add("@MaintenanceType", maintenanceType);
        parameters.Add("@DisposalType", disposalType);

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR a.SerialNumber LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (additionalFilters != null)
        {
            foreach (var filter in additionalFilters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        if (string.IsNullOrEmpty(sortBy)) sortBy = "a.AssetCode";
        if (!sortBy.StartsWith("a.") && !sortBy.StartsWith("c.") && !sortBy.StartsWith("e."))
            sortBy = $"a.{sortBy}";

        var orderBy = $"ORDER BY {sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            {whereClause}";

        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.PurchaseDate, a.PurchasePrice, a.WarrantyExpiryDate, a.IsActive,
                CASE 
                    WHEN a.IsActive = 0 THEN 'Inactive'
                    WHEN lt.AssetId IS NULL THEN 'Available'
                    WHEN lt.TransactionType = @LoanType THEN 'On Loan'
                    WHEN lt.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN lt.TransactionType = @MaintenanceType THEN 'In Maintenance'
                    ELSE 'Available'
                END as CurrentStatus,
                e.FullName as CurrentHolderName";

        var fromClause = @"
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Employee e ON lt.ToEmployeeId = e.EmployeeId
            LEFT JOIN (
                SELECT AssetId, TransactionType, ToEmployeeId,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND IsActive = 1 AND FromAssetTransactionId IS NULL
            ) lt ON a.AssetId = lt.AssetId AND lt.rn = 1";

        var offset = (page - 1) * pageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await _context.QueryAsync<AssetDetailDto>(dataSql, parameters);

        return new PaginatedResult<AssetDetailDto>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    // ============================================================
    // DETAIL STATS - ASSET TYPES
    // ============================================================

    public async Task<PaginatedResult<AssetDetailDto>> GetAvailableAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending)
    {
        var baseWhere = @"a.IsActive = 1 AND NOT EXISTS (
            SELECT 1 FROM AssetTransaction at 
            WHERE at.AssetId = a.AssetId 
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1
              AND at.TransactionType IN (@HandoverType, @TransferType, @LoanType, @MaintenanceType))";

        return await GetAssetDetailListAsync(baseWhere, null, page, pageSize, search, sortBy, sortDescending);
    }

    public async Task<PaginatedResult<AssetDetailDto>> GetAssignedAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending)
    {
        var baseWhere = @"a.IsActive = 1 AND EXISTS (
            SELECT 1 FROM AssetTransaction at 
            WHERE at.AssetId = a.AssetId 
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1
              AND at.TransactionType IN (@HandoverType, @TransferType))";

        return await GetAssetDetailListAsync(baseWhere, null, page, pageSize, search, sortBy, sortDescending);
    }

    public async Task<PaginatedResult<AssetDetailDto>> GetOnLoanAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending)
    {
        var baseWhere = @"a.IsActive = 1 AND EXISTS (
            SELECT 1 FROM AssetTransaction at 
            WHERE at.AssetId = a.AssetId 
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1
              AND at.TransactionType = @LoanType)";

        return await GetAssetDetailListAsync(baseWhere, null, page, pageSize, search, sortBy, sortDescending);
    }

    public async Task<PaginatedResult<AssetDetailDto>> GetInMaintenanceAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending)
    {
        var baseWhere = @"a.IsActive = 1 AND EXISTS (
            SELECT 1 FROM AssetTransaction at 
            WHERE at.AssetId = a.AssetId 
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1
              AND at.TransactionType = @MaintenanceType)";

        return await GetAssetDetailListAsync(baseWhere, null, page, pageSize, search, sortBy, sortDescending);
    }

    public async Task<PaginatedResult<AssetDetailDto>> GetDisposedAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending)
    {
        var baseWhere = @"EXISTS (
            SELECT 1 FROM AssetTransaction at 
            WHERE at.AssetId = a.AssetId 
              AND at.Approved = 1 
              AND at.IsActive = 1
              AND at.TransactionType = @DisposalType)";

        return await GetAssetDetailListAsync(baseWhere, null, page, pageSize, search, sortBy, sortDescending);
    }

    public async Task<PaginatedResult<AssetDetailDto>> GetDamagedAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending)
    {
        var baseWhere = @"(a.IsActive = 0 OR EXISTS (
            SELECT 1 FROM AssetTransaction at 
            WHERE at.AssetId = a.AssetId 
              AND at.Approved = 1 
              AND at.IsActive = 1
              AND at.TransactionType = @DisposalType))";

        return await GetAssetDetailListAsync(baseWhere, null, page, pageSize, search, sortBy, sortDescending);
    }

    public async Task<PaginatedResult<AssetDetailDto>> GetExpiredWarrantyDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending)
    {
        var baseWhere = @"a.WarrantyExpiryDate < GETDATE() AND a.WarrantyExpiryDate IS NOT NULL";

        return await GetAssetDetailListAsync(baseWhere, null, page, pageSize, search, sortBy, sortDescending);
    }

    // ============================================================
    // DETAIL STATS - OVERDUE LOANS
    // ============================================================

    public async Task<PaginatedResult<OverdueLoanDetailDto>> GetOverdueLoansDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending)
    {
        var conditions = new List<string> {
            "at.TransactionType = @LoanType",
            "at.Approved = 1",
            "at.FromAssetTransactionId IS NULL",
            "at.IsActive = 1",
            "at.ExpectedLoanReturnDate < GETDATE()"
        };
        var parameters = new DynamicParameters();
        parameters.Add("@LoanType", TransactionTypeConstants.LOAN);

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR e.FullName LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        if (string.IsNullOrEmpty(sortBy)) sortBy = "at.ExpectedLoanReturnDate";
        if (!sortBy.StartsWith("at.") && !sortBy.StartsWith("a.") && !sortBy.StartsWith("e."))
            sortBy = $"at.{sortBy}";

        var orderBy = $"ORDER BY {sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"
            SELECT COUNT(*) 
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId
            LEFT JOIN Employee e ON at.ToEmployeeId = e.EmployeeId
            {whereClause}";

        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var dataSql = $@"
            SELECT 
                at.AssetTransactionId, at.AssetId, at.TransactionDate, at.ExpectedLoanReturnDate, at.Notes,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName,
                DATEDIFF(DAY, at.ExpectedLoanReturnDate, GETDATE()) as DaysOverdue
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId
            LEFT JOIN Employee e ON at.ToEmployeeId = e.EmployeeId
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await _context.QueryAsync<OverdueLoanDetailDto>(dataSql, parameters);

        return new PaginatedResult<OverdueLoanDetailDto>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}