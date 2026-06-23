using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Reports;
using Whitebird.Domain.Features.MasterData;

namespace Whitebird.Infra.Features.Reports;

/// <summary>
/// Repository implementation for Reports operations using Dapper
/// </summary>
public class ReportsReps : IReportsReps
{
    private readonly DapperContext _context;

    public ReportsReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReportsAssetTransactionViewModel>> GetAssetTransactionReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (startDate.HasValue)
        {
            conditions.Add("t.TransactionDate >= @StartDate");
            parameters.Add("@StartDate", startDate.Value);
        }
        if (endDate.HasValue)
        {
            conditions.Add("t.TransactionDate <= @EndDate");
            parameters.Add("@EndDate", endDate.Value);
        }
        if (!string.IsNullOrEmpty(transactionType))
        {
            conditions.Add("md1.MasterDataName = @TransactionType");
            parameters.Add("@TransactionType", transactionType);
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";

        var sql = $@"
            SELECT 
                e.EmployeeCode, e.FullName, e.Email, d.DepartmentName,
                c.CategoryName, c.CategoryId,
                a.AssetName, a.AssetCode, a.SerialNumber, a.Brand, a.Model,
                md2.MasterDataName as AssetConditionName, a.PurchaseDate,
                t.TransactionDate, md1.MasterDataName as TransactionTypeName,
                CASE 
                    WHEN t.Approved = 1 THEN 'Approved'
                    WHEN t.Approved = 0 THEN 'Rejected'
                    ELSE 'Pending'
                END as ApprovalStatus,
                a.PurchasePrice, t.Notes, t.ExpectedReturnDate, t.ActualReturnDate,
                o.OfficeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee e ON t.ToEmployeeId = e.EmployeeId AND e.IsActive = 1
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetCondition = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            WHERE t.IsActive = 1
            {whereClause}
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<ReportsAssetTransactionViewModel>(sql, parameters);
    }

    public async Task<IEnumerable<ReportsAssetInventoryViewModel>> GetAssetInventoryReportsAsync(
        string? status = null, int? categoryId = null, int? supplierId = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Available")
            {
                conditions.Add(@"
                    NOT EXISTS (
                        SELECT 1 FROM AssetTransaction at 
                        WHERE at.AssetId = a.AssetId 
                          AND at.Approved = 1 
                          AND at.FromAssetTransactionId IS NULL 
                          AND at.IsActive = 1
                    )");
            }
            else if (status == "Assigned")
            {
                conditions.Add(@"
                    EXISTS (
                        SELECT 1 FROM AssetTransaction at 
                        WHERE at.AssetId = a.AssetId 
                          AND at.TransactionType IN (@HandoverType, @TransferType)
                          AND at.Approved = 1 
                          AND at.FromAssetTransactionId IS NULL 
                          AND at.IsActive = 1
                    )");
                parameters.Add("@HandoverType", TransactionTypeConstants.HANDOVER);
                parameters.Add("@TransferType", TransactionTypeConstants.TRANSFER);
            }
            else if (status == "Damaged")
            {
                conditions.Add("a.AssetCondition = @DamagedCondition OR a.IsActive = 0");
                parameters.Add("@DamagedCondition", AssetConditionConstants.DAMAGED);
            }
            else if (status == "On Loan")
            {
                conditions.Add(@"
                    EXISTS (
                        SELECT 1 FROM AssetTransaction at 
                        WHERE at.AssetId = a.AssetId 
                          AND at.TransactionType = @LoanType
                          AND at.Approved = 1 
                          AND at.FromAssetTransactionId IS NULL 
                          AND at.IsActive = 1
                    )");
                parameters.Add("@LoanType", TransactionTypeConstants.LOAN);
            }
            else if (status == "In Maintenance")
            {
                conditions.Add(@"
                    EXISTS (
                        SELECT 1 FROM AssetTransaction at 
                        WHERE at.AssetId = a.AssetId 
                          AND at.TransactionType = @MaintenanceType
                          AND at.Approved = 1 
                          AND at.FromAssetTransactionId IS NULL 
                          AND at.IsActive = 1
                    )");
                parameters.Add("@MaintenanceType", TransactionTypeConstants.MAINTENANCE);
            }
            else if (status == "Active")
            {
                conditions.Add("a.IsActive = 1");
            }
            else if (status == "Inactive")
            {
                conditions.Add("a.IsActive = 0");
            }
        }

        if (categoryId.HasValue)
        {
            conditions.Add("a.CategoryId = @CategoryId");
            parameters.Add("@CategoryId", categoryId.Value);
        }
        if (supplierId.HasValue)
        {
            conditions.Add("a.SupplierId = @SupplierId");
            parameters.Add("@SupplierId", supplierId.Value);
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";

        var sql = $@"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.SerialNumber, a.Brand, a.Model,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
                    ELSE 'Available'
                END as CurrentStatus,
                md1.MasterDataName as AssetConditionName,
                md2.MasterDataName as AssetConditionPurchaseName,
                o.OfficeName, c.CategoryName, s.SupplierName,
                e.FullName as CurrentHolderName,
                a.PurchaseDate, a.PurchasePrice, a.WarrantyExpiryDate,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.Hostname, a.IpAddress,
                CASE WHEN a.OperasionalOffice = 1 THEN 'Yes' ELSE 'No' END as OperasionalOffice,
                a.ResidualValue, a.UsefulLife, a.Notes
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN Employee e ON at2.ToEmployeeId = e.EmployeeId AND e.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType, ToEmployeeId,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            {whereClause}
            ORDER BY a.AssetCode";

        var sqlParams = new DynamicParameters(parameters);
        sqlParams.Add("@LoanType", TransactionTypeConstants.LOAN);
        sqlParams.Add("@HandoverType", TransactionTypeConstants.HANDOVER);
        sqlParams.Add("@TransferType", TransactionTypeConstants.TRANSFER);
        sqlParams.Add("@MaintenanceType", TransactionTypeConstants.MAINTENANCE);

        return await _context.QueryAsync<ReportsAssetInventoryViewModel>(sql, sqlParams);
    }

    public async Task<IEnumerable<ReportsEmployeeAssetViewModel>> GetEmployeeAssetReportsAsync(
        int? employeeId = null)
    {
        var conditions = new List<string> {
            "e.IsActive = 1",
            "at.Approved = 1",
            "at.FromAssetTransactionId IS NULL"
        };
        var parameters = new DynamicParameters();

        if (employeeId.HasValue)
        {
            conditions.Add("e.EmployeeId = @EmployeeId");
            parameters.Add("@EmployeeId", employeeId.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sql = $@"
            SELECT 
                e.EmployeeId, e.EmployeeCode, e.FullName, d.DepartmentName,
                md1.MasterDataName as PositionName, e.Email, e.PhoneNumber,
                md2.MasterDataName as EmploymentStatusName, o.OfficeName,
                a.AssetId, a.AssetCode, a.AssetName, a.SerialNumber, a.Brand, a.Model,
                md3.MasterDataName as AssetConditionName, c.CategoryName,
                at.TransactionDate as AssignmentDate, at.ExpectedReturnDate,
                CASE 
                    WHEN at.TransactionType = @LoanType THEN 'On Loan'
                    ELSE 'Assigned'
                END as AssociationType,
                CASE WHEN at.ExpectedReturnDate < GETDATE() THEN 'Yes' ELSE 'No' END as IsOverdue,
                a.LastMaintenanceDate, a.NextMaintenanceDate, a.Notes
            FROM Employee e
            INNER JOIN AssetTransaction at ON e.EmployeeId = at.ToEmployeeId
            INNER JOIN Asset a ON at.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            LEFT JOIN MasterData md3 ON a.AssetCondition = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition' AND md3.IsActive = 1
            {whereClause}
            ORDER BY e.FullName, a.AssetCode";

        parameters.Add("@LoanType", TransactionTypeConstants.LOAN);

        return await _context.QueryAsync<ReportsEmployeeAssetViewModel>(sql, parameters);
    }

    public async Task<IEnumerable<ReportsMaintenanceViewModel>> GetMaintenanceReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (isUpcoming == true)
        {
            conditions.Add("a.NextMaintenanceDate >= GETDATE() AND a.NextMaintenanceDate IS NOT NULL");
            if (startDate.HasValue)
            {
                conditions.Add("a.NextMaintenanceDate >= @StartDate");
                parameters.Add("@StartDate", startDate.Value);
            }
            if (endDate.HasValue)
            {
                conditions.Add("a.NextMaintenanceDate <= @EndDate");
                parameters.Add("@EndDate", endDate.Value);
            }
        }
        else
        {
            conditions.Add("a.IsActive = 1");
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";

        var sql = $@"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.SerialNumber, a.Brand, a.Model,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                md1.MasterDataName as AssetConditionName, c.CategoryName,
                e.FullName as CurrentHolderName, o.OfficeName,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
                    ELSE 'Available'
                END as CurrentStatus,
                (SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceType IS NOT NULL AND IsActive = 1) as MaintenanceCount,
                md2.MasterDataName as LastMaintenanceTypeName,
                (SELECT TOP 1 MaintenanceCost FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL ORDER BY TransactionDate DESC) as LastMaintenanceCost,
                (SELECT TOP 1 Notes FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceType IS NOT NULL ORDER BY TransactionDate DESC) as LastMaintenanceNotes,
                (SELECT SUM(MaintenanceCost) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL AND IsActive = 1) as TotalMaintenanceCost
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN Employee e ON at2.ToEmployeeId = e.EmployeeId AND e.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON lastMaint.MaintenanceType = md2.ReferenceCode AND md2.ReferenceName = 'MaintenanceType' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType, ToEmployeeId,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            LEFT JOIN (
                SELECT TOP 1 AssetId, MaintenanceType
                FROM AssetTransaction
                WHERE MaintenanceType IS NOT NULL AND IsActive = 1
                ORDER BY TransactionDate DESC
            ) lastMaint ON a.AssetId = lastMaint.AssetId
            {whereClause}
            ORDER BY a.NextMaintenanceDate, a.AssetCode";

        var sqlParams = new DynamicParameters(parameters);
        sqlParams.Add("@LoanType", TransactionTypeConstants.LOAN);
        sqlParams.Add("@HandoverType", TransactionTypeConstants.HANDOVER);
        sqlParams.Add("@TransferType", TransactionTypeConstants.TRANSFER);
        sqlParams.Add("@MaintenanceType", TransactionTypeConstants.MAINTENANCE);

        return await _context.QueryAsync<ReportsMaintenanceViewModel>(sql, sqlParams);
    }

    public async Task<IEnumerable<ReportsFinancialViewModel>> GetFinancialReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("a.IsActive = 1");

        if (startDate.HasValue)
        {
            conditions.Add("a.PurchaseDate >= @StartDate");
            parameters.Add("@StartDate", startDate.Value);
        }
        if (endDate.HasValue)
        {
            conditions.Add("a.PurchaseDate <= @EndDate");
            parameters.Add("@EndDate", endDate.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sql = $@"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.WarrantyPeriod, a.WarrantyExpiryDate, a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                c.CategoryName, s.SupplierName, o.OfficeName,
                md1.MasterDataName as AssetConditionPurchaseName,
                CASE WHEN a.WarrantyExpiryDate < GETDATE() THEN 'Yes' ELSE 'No' END as IsWarrantyExpired,
                CASE 
                    WHEN a.UsefulLife > 0 AND a.PurchasePrice IS NOT NULL AND a.PurchasePrice > 0 AND a.DepreciationStartDate IS NOT NULL
                    THEN a.PurchasePrice / a.UsefulLife
                    ELSE NULL
                END as AnnualDepreciation,
                CASE 
                    WHEN a.UsefulLife > 0 AND a.PurchasePrice IS NOT NULL AND a.PurchasePrice > 0 AND a.DepreciationStartDate IS NOT NULL
                    THEN a.PurchasePrice - ((DATEDIFF(YEAR, a.DepreciationStartDate, GETDATE()) * (a.PurchasePrice / a.UsefulLife)))
                    ELSE a.PurchasePrice
                END as CurrentBookValue,
                (SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL AND IsActive = 1) as MaintenanceCount,
                (SELECT SUM(MaintenanceCost) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL AND IsActive = 1) as TotalMaintenanceCost,
                ISNULL(a.PurchasePrice, 0) + ISNULL((SELECT SUM(MaintenanceCost) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL AND IsActive = 1), 0) as TotalCostOfOwnership
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetConditionPurchase = md1.ReferenceCode AND md1.ReferenceName = 'AssetConditionPurchase' AND md1.IsActive = 1
            {whereClause}
            ORDER BY a.PurchaseDate DESC, a.AssetCode";

        return await _context.QueryAsync<ReportsFinancialViewModel>(sql, parameters);
    }

    // ============================================================
    // DASHBOARD STATS - COMPLETE
    // ============================================================

    public async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
    {
        const string sql = @"
        WITH ActiveTransactions AS (
            SELECT 
                AssetId,
                TransactionType,
                ToEmployeeId,
                ExpectedReturnDate,
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
            (SELECT COUNT(*) FROM Asset WHERE IsActive = 0 OR AssetCondition = @DamagedCondition) AS DamagedAssets,
            (SELECT COUNT(*) FROM Asset WHERE WarrantyExpiryDate < GETDATE() AND WarrantyExpiryDate IS NOT NULL AND IsActive = 1) AS ExpiredWarrantyCount,
            (SELECT COUNT(*) FROM Asset WHERE NextMaintenanceDate BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE()) AND IsActive = 1) AS UpcomingMaintenanceCount,
            (SELECT COUNT(*) FROM ActiveTransactions at WHERE at.TransactionType = @LoanType AND at.ExpectedReturnDate < GETDATE() AND at.rn = 1) AS OverdueLoanCount,
            ISNULL((SELECT SUM(PurchasePrice) FROM Asset WHERE IsActive = 1), 0) AS TotalAssetValue,
            (SELECT COUNT(*) FROM Employee WHERE IsActive = 1) AS TotalEmployees,
            (SELECT COUNT(*) FROM Employee WHERE IsActive = 1 AND EmploymentStatus = @PermanentStatus) AS ActiveEmployees,
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
            PermanentStatus = EmployeeStatusConstants.PERMANENT,
            DamagedCondition = AssetConditionConstants.DAMAGED
        };

        var result = await _context.QueryFirstOrDefaultAsync<DashboardStatsViewModel>(sql, parameters);
        return result ?? new DashboardStatsViewModel();
    }

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

    public async Task<IEnumerable<RecentTransactionDto>> GetRecentTransactionsAsync(int limit = 10)
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
            t.ExpectedReturnDate,
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

    public async Task<int> GetPendingApprovalsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM AssetTransaction WHERE Approved IS NULL AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetDamagedAssetsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Asset WHERE IsActive = 0 OR AssetCondition = @DamagedCondition";
        return await _context.ExecuteScalarAsync<int>(sql, new { DamagedCondition = AssetConditionConstants.DAMAGED });
    }

    // ============================================================
    // EXPORT METHODS
    // ============================================================

    public async Task<IEnumerable<ReportsAssetTransactionViewModel>> ExportAssetTransactionReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null)
    {
        return await GetAssetTransactionReportsAsync(startDate, endDate, transactionType);
    }

    public async Task<IEnumerable<ReportsAssetInventoryViewModel>> ExportAssetInventoryReportsAsync(
        string? status = null, int? categoryId = null, int? supplierId = null)
    {
        return await GetAssetInventoryReportsAsync(status, categoryId, supplierId);
    }

    public async Task<IEnumerable<ReportsEmployeeAssetViewModel>> ExportEmployeeAssetReportsAsync(
        int? employeeId = null)
    {
        return await GetEmployeeAssetReportsAsync(employeeId);
    }

    public async Task<IEnumerable<ReportsMaintenanceViewModel>> ExportMaintenanceReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null)
    {
        return await GetMaintenanceReportsAsync(startDate, endDate, isUpcoming);
    }

    public async Task<IEnumerable<ReportsFinancialViewModel>> ExportFinancialReportsAsync(
        DateTime? startDate = null, DateTime? endDate = null)
    {
        return await GetFinancialReportsAsync(startDate, endDate);
    }
}