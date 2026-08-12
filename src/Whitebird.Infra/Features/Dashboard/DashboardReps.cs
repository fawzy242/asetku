using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Dashboard;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.Common;
using Whitebird.Domain.Features.Common;

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
    // MAIN STATS - FIXED (ToEmployeeId → EmployeeId)
    // ============================================================

    public async Task<DashboardStatsViewModel> GetStatsAsync()
    {
        const string sql = @"
        WITH ActiveTransactions AS (
            SELECT 
                AssetId,
                TransactionType,
                EmployeeId,
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
            LEFT JOIN Asset a ON c.CategoryId = c.CategoryId AND a.IsActive = 1
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
        e.FullName as EmployeeName,
        t.TransactionDate,
        t.Approved,
        t.ExpectedLoanReturnDate,
        t.Notes
    FROM AssetTransaction t
    LEFT JOIN Asset a ON t.AssetId = a.AssetId
    LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
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
    // DETAIL STATS (Drilldown) - Using GridRequest
    // ============================================================

    public async Task<PaginatedResult<AssetDetailDto>> GetDetailStatsAsync(DashboardDetailStatsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return new PaginatedResult<AssetDetailDto>();
        }

        var conditions = new List<string>();
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

        switch (request.Type.ToLower())
        {
            case "available":
                conditions.Add(@"a.IsActive = 1 AND NOT EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.FromAssetTransactionId IS NULL 
                      AND at.IsActive = 1
                      AND at.TransactionType IN (@HandoverType, @TransferType, @LoanType, @MaintenanceType))");
                break;
            case "assigned":
                conditions.Add(@"a.IsActive = 1 AND EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.FromAssetTransactionId IS NULL 
                      AND at.IsActive = 1
                      AND at.TransactionType IN (@HandoverType, @TransferType))");
                break;
            case "onloan":
                conditions.Add(@"a.IsActive = 1 AND EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.FromAssetTransactionId IS NULL 
                      AND at.IsActive = 1
                      AND at.TransactionType = @LoanType)");
                break;
            case "inmaintenance":
                conditions.Add(@"a.IsActive = 1 AND EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.FromAssetTransactionId IS NULL 
                      AND at.IsActive = 1
                      AND at.TransactionType = @MaintenanceType)");
                break;
            case "disposed":
                conditions.Add(@"EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.IsActive = 1
                      AND at.TransactionType = @DisposalType)");
                break;
            case "damaged":
                conditions.Add(@"(a.IsActive = 0 OR EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.IsActive = 1
                      AND at.TransactionType = @DisposalType))");
                break;
            case "expiredwarranty":
                conditions.Add("a.WarrantyExpiryDate < GETDATE() AND a.WarrantyExpiryDate IS NOT NULL");
                break;
            case "overdueloans":
                return await GetOverdueLoansDetailAsync(request);
            default:
                return new PaginatedResult<AssetDetailDto>();
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "a.AssetCode", "a.AssetName", "a.SerialNumber" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        if (request.IsActive.HasValue && request.Type.ToLower() != "available" && request.Type.ToLower() != "assigned")
        {
            GridQueryBuilder.AddEqualsFilter(conditions, parameters, "a.IsActive", request.IsActive);
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "a.AssetCode", false);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

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
            LEFT JOIN Employee e ON lt.EmployeeId = e.EmployeeId
            LEFT JOIN (
                SELECT AssetId, TransactionType, EmployeeId,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND IsActive = 1 AND FromAssetTransactionId IS NULL
            ) lt ON a.AssetId = lt.AssetId AND lt.rn = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<AssetDetailDto>(dataSql, parameters);

        return new PaginatedResult<AssetDetailDto>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    private async Task<PaginatedResult<AssetDetailDto>> GetOverdueLoansDetailAsync(DashboardDetailStatsRequest request)
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

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR e.FullName LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "at.ExpectedLoanReturnDate", false);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*) 
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId
            LEFT JOIN Employee e ON at.EmployeeId = e.EmployeeId
            {whereClause}";

        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                at.AssetTransactionId, at.AssetId, at.TransactionDate, at.ExpectedLoanReturnDate, at.Notes,
                a.AssetCode, a.AssetName,
                e.FullName as EmployeeName,
                DATEDIFF(DAY, at.ExpectedLoanReturnDate, GETDATE()) as DaysOverdue";

        var fromClause = @"
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId
            LEFT JOIN Employee e ON at.EmployeeId = e.EmployeeId";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<AssetDetailDto>(dataSql, parameters);

        return new PaginatedResult<AssetDetailDto>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}