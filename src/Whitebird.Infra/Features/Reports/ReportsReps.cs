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

    // ============================================================
    // ASSET TRANSACTION DATA
    // ============================================================

    public async Task<IEnumerable<ReportsAssetTransactionViewModel>> GetAssetTransactionDataAsync(
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
                md2.MasterDataName as ConditionName, a.PurchaseDate,
                t.TransactionDate, md1.MasterDataName as TransactionTypeName,
                CASE 
                    WHEN t.Approved = 1 THEN 'Approved'
                    WHEN t.Approved = 0 THEN 'Rejected'
                    ELSE 'Pending'
                END as ApprovalStatus,
                a.PurchasePrice, t.Notes, t.ExpectedLoanReturnDate,
                o.OfficeName, t.EmployeeId
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId AND e.IsActive = 1
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Office o ON t.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.Condition = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            WHERE t.IsActive = 1
            {whereClause}
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<ReportsAssetTransactionViewModel>(sql, parameters);
    }

    // ============================================================
    // ASSET INVENTORY DATA
    // ============================================================

    public async Task<IEnumerable<ReportsAssetInventoryViewModel>> GetAssetInventoryDataAsync(
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
                      AND at.TransactionType IN (@HandoverType, @TransferType, @LoanType, @MaintenanceType)
                )");
                parameters.Add("@HandoverType", TransactionTypeConstants.HANDOVER);
                parameters.Add("@TransferType", TransactionTypeConstants.TRANSFER);
                parameters.Add("@LoanType", TransactionTypeConstants.LOAN);
                parameters.Add("@MaintenanceType", TransactionTypeConstants.MAINTENANCE);
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
            else if (status == "Damaged")
            {
                conditions.Add(@"
                EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.TransactionType = @DisposalType
                      AND at.Approved = 1 
                      AND at.IsActive = 1
                )");
                parameters.Add("@DisposalType", TransactionTypeConstants.DISPOSAL);
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

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            conditions.Add("a.CategoryId = @CategoryId");
            parameters.Add("@CategoryId", categoryId.Value);
        }

        if (supplierId.HasValue && supplierId.Value > 0)
        {
            conditions.Add("a.SupplierId = @SupplierId");
            parameters.Add("@SupplierId", supplierId.Value);
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";

        var sql = $@"
        SELECT 
            a.AssetId, a.AssetCode, a.AssetName, a.SerialNumber, a.Brand, a.Model,
            a.Hostname, a.IpAddress, a.Notes,
            a.PurchaseDate, a.PurchasePrice, a.WarrantyExpiryDate,
            CASE 
                WHEN at2.AssetId IS NULL THEN 'Available'
                WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
                WHEN at2.TransactionType = @DisposalType THEN 'Disposed'
                ELSE 'Available'
            END as CurrentStatus,
            mdCondition.MasterDataName as AssetConditionName,
            mdPurchase.MasterDataName as AssetConditionPurchaseName,
            o.OfficeName, c.CategoryName, s.SupplierName,
            e.FullName as CurrentHolderName
        FROM Asset a
        LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
        LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
        LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
        LEFT JOIN MasterData mdPurchase ON a.AssetConditionPurchase = mdPurchase.ReferenceCode AND mdPurchase.ReferenceName = 'AssetConditionPurchase' AND mdPurchase.IsActive = 1
        LEFT JOIN MasterData mdCondition ON at2.Condition = mdCondition.ReferenceCode AND mdCondition.ReferenceName = 'AssetCondition' AND mdCondition.IsActive = 1
        LEFT JOIN Employee e ON at2.EmployeeId = e.EmployeeId AND e.IsActive = 1
        LEFT JOIN (
            SELECT AssetId, TransactionType, EmployeeId, Condition,
                   ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
            FROM AssetTransaction
            WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
        ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
        {whereClause}
        ORDER BY a.AssetCode";

        return await _context.QueryAsync<ReportsAssetInventoryViewModel>(sql, parameters);
    }

    // ============================================================
    // EMPLOYEE ASSET DATA
    // ============================================================

    public async Task<IEnumerable<ReportsEmployeeAssetViewModel>> GetEmployeeAssetDataAsync(
        int? employeeId = null)
    {
        var conditions = new List<string>
        {
            "e.IsActive = 1",
            "at.Approved = 1",
            "at.FromAssetTransactionId IS NULL",
            "at.IsActive = 1",
            "at.TransactionType IN (@HandoverType, @TransferType, @LoanType)"
        };
        var parameters = new DynamicParameters();
        parameters.Add("@HandoverType", TransactionTypeConstants.HANDOVER);
        parameters.Add("@TransferType", TransactionTypeConstants.TRANSFER);
        parameters.Add("@LoanType", TransactionTypeConstants.LOAN);

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
                at.TransactionDate as AssignmentDate, at.ExpectedLoanReturnDate as ExpectedReturnDate,
                CASE 
                    WHEN at.TransactionType = @LoanType THEN 'On Loan'
                    ELSE 'Assigned'
                END as AssociationType,
                CASE WHEN at.ExpectedLoanReturnDate < GETDATE() THEN 'Yes' ELSE 'No' END as IsOverdue,
                a.Notes
            FROM Employee e
            INNER JOIN AssetTransaction at ON e.EmployeeId = at.EmployeeId
            INNER JOIN Asset a ON at.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            LEFT JOIN MasterData md3 ON at.Condition = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition' AND md3.IsActive = 1
            {whereClause}
            ORDER BY e.FullName, a.AssetCode";

        return await _context.QueryAsync<ReportsEmployeeAssetViewModel>(sql, parameters);
    }

    // ============================================================
    // MAINTENANCE DATA
    // ============================================================

    public async Task<IEnumerable<ReportsMaintenanceViewModel>> GetMaintenanceDataAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("t.TransactionType = @MaintenanceType");
        parameters.Add("@MaintenanceType", TransactionTypeConstants.MAINTENANCE);

        if (isUpcoming == true)
        {
            conditions.Add("t.TransactionDate >= GETDATE()");
        }
        else if (isUpcoming == false)
        {
            conditions.Add("t.TransactionDate < GETDATE()");
        }

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

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sql = $@"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.SerialNumber, a.Brand, a.Model,
                c.CategoryName,
                e.FullName as CurrentHolderName,
                o.OfficeName,
                mdCondition.MasterDataName as AssetConditionName,
                t.TransactionDate as MaintenanceDate,
                mdMaintenance.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost,
                t.Notes,
                (SELECT SUM(MaintenanceCost) FROM AssetTransaction WHERE AssetId = a.AssetId AND TransactionType = @MaintenanceType AND IsActive = 1) as TotalMaintenanceCost,
                (SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = a.AssetId AND TransactionType = @MaintenanceType AND IsActive = 1) as MaintenanceCount,
                CASE 
                    WHEN a.IsActive = 0 THEN 'Inactive'
                    WHEN lt.AssetId IS NULL THEN 'Available'
                    WHEN lt.TransactionType = @LoanType THEN 'On Loan'
                    WHEN lt.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN lt.TransactionType = @MaintenanceType THEN 'In Maintenance'
                    ELSE 'Available'
                END as CurrentStatus
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Office o ON t.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId AND e.IsActive = 1
            LEFT JOIN MasterData mdMaintenance ON t.MaintenanceType = mdMaintenance.ReferenceCode AND mdMaintenance.ReferenceName = 'MaintenanceType' AND mdMaintenance.IsActive = 1
            LEFT JOIN MasterData mdCondition ON t.Condition = mdCondition.ReferenceCode AND mdCondition.ReferenceName = 'AssetCondition' AND mdCondition.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND IsActive = 1 AND FromAssetTransactionId IS NULL
            ) lt ON a.AssetId = lt.AssetId AND lt.rn = 1
            {whereClause}
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<ReportsMaintenanceViewModel>(sql, parameters);
    }

    // ============================================================
    // FINANCIAL DATA
    // ============================================================

    public async Task<IEnumerable<ReportsFinancialViewModel>> GetFinancialDataAsync(
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
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                c.CategoryName, s.SupplierName, o.OfficeName,
                md.MasterDataName as AssetConditionPurchaseName,
                CASE WHEN a.WarrantyExpiryDate < GETDATE() THEN 'Yes' ELSE 'No' END as IsWarrantyExpired,
                (SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL AND IsActive = 1) as MaintenanceCount,
                (SELECT SUM(MaintenanceCost) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL AND IsActive = 1) as TotalMaintenanceCost,
                ISNULL(a.PurchasePrice, 0) + ISNULL((SELECT SUM(MaintenanceCost) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL AND IsActive = 1), 0) as TotalCostOfOwnership
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md ON a.AssetConditionPurchase = md.ReferenceCode AND md.ReferenceName = 'AssetConditionPurchase' AND md.IsActive = 1
            {whereClause}
            ORDER BY a.PurchaseDate DESC, a.AssetCode";

        return await _context.QueryAsync<ReportsFinancialViewModel>(sql, parameters);
    }
}