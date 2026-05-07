using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Reports.View;

namespace Whitebird.Infra.Features.Reports;

public class ReportsReps : IReportsReps
{
    private readonly DapperContext _context;

    public ReportsReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReportsAssetTransactionViewModel>> GetAssetTransactionReportsAsync(DateTime? startDate = null, DateTime? endDate = null, string? transactionType = null)
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
            conditions.Add("t.TransactionType = @TransactionType");
            parameters.Add("@TransactionType", transactionType);
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";

        var sql = $@"
            SELECT e.EmployeeCode, e.FullName, e.Email, e.Department, c.CategoryName, c.CategoryId,
                   a.AssetName, a.AssetCode, a.SerialNumber, a.Brand, a.Model, a.Condition, a.PurchaseDate,
                   t.TransactionDate, t.TransactionType, t.TransactionStatus, a.PurchasePrice,
                   t.Notes, t.ExpectedReturnDate, t.ActualReturnDate
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.ToEmployeeId = e.EmployeeId
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            {whereClause}
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<ReportsAssetTransactionViewModel>(sql, parameters);
    }

    public async Task<IEnumerable<ReportsAssetInventoryViewModel>> GetAssetInventoryReportsAsync(string? status = null, int? categoryId = null, int? supplierId = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(status))
        {
            conditions.Add("a.Status = @Status");
            parameters.Add("@Status", status);
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

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "WHERE a.IsActive = 1";

        var sql = $@"
            SELECT a.AssetId, a.AssetCode, a.AssetName, a.SerialNumber, a.Brand, a.Model,
                   a.Status, a.Condition, a.Location, a.PurchaseDate, a.PurchasePrice,
                   a.WarrantyExpiryDate, a.LastMaintenanceDate, a.NextMaintenanceDate,
                   c.CategoryName, s.SupplierName, e.FullName AS CurrentHolderName, a.Notes
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            {whereClause}
            ORDER BY a.AssetCode";

        return await _context.QueryAsync<ReportsAssetInventoryViewModel>(sql, parameters);
    }

    public async Task<IEnumerable<ReportsEmployeeAssetViewModel>> GetEmployeeAssetReportsAsync(int? employeeId = null, string? department = null)
    {
        var conditions = new List<string> { "e.IsActive = 1", "a.Status IN ('Assigned', 'On Loan')" };
        var parameters = new DynamicParameters();

        if (employeeId.HasValue)
        {
            conditions.Add("e.EmployeeId = @EmployeeId");
            parameters.Add("@EmployeeId", employeeId.Value);
        }
        if (!string.IsNullOrEmpty(department))
        {
            conditions.Add("e.Department = @Department");
            parameters.Add("@Department", department);
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sql = $@"
            SELECT e.EmployeeId, e.EmployeeCode, e.FullName, e.Department, e.Position, e.Email, e.PhoneNumber,
                   a.AssetId, a.AssetCode, a.AssetName, a.SerialNumber, a.Brand, a.Model,
                   a.Condition, a.LastMaintenanceDate, a.NextMaintenanceDate, a.Notes,
                   c.CategoryName, tr.TransactionDate AS AssignmentDate, tr.ExpectedReturnDate
            FROM Employee e
            INNER JOIN Asset a ON e.EmployeeId = a.CurrentHolderId AND a.IsActive = 1
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN (
                SELECT AssetId, TransactionDate, ExpectedReturnDate,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) AS rn
                FROM AssetTransaction
                WHERE TransactionType IN ('HANDOVER', 'LOAN', 'TRANSFER') AND TransactionStatus = 'Approved'
            ) tr ON a.AssetId = tr.AssetId AND tr.rn = 1
            {whereClause}
            ORDER BY e.Department, e.FullName, a.AssetCode";

        return await _context.QueryAsync<ReportsEmployeeAssetViewModel>(sql, parameters);
    }

    public async Task<IEnumerable<ReportsMaintenanceViewModel>> GetMaintenanceReportsAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isUpcoming = null)
    {
        var conditions = new List<string> { "a.IsActive = 1" };
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

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sql = $@"
            SELECT a.AssetId, a.AssetCode, a.AssetName, a.SerialNumber, a.Brand, a.Model,
                   a.LastMaintenanceDate, a.NextMaintenanceDate, a.Condition, a.Status,
                   c.CategoryName, e.FullName AS CurrentHolderName,
                   (SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceType IS NOT NULL) AS MaintenanceCount,
                   (SELECT TOP 1 Notes FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceType IS NOT NULL ORDER BY TransactionDate DESC) AS LastMaintenanceNotes
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Employee e ON a.CurrentHolderId = e.EmployeeId
            {whereClause}
            ORDER BY a.NextMaintenanceDate, a.AssetCode";

        return await _context.QueryAsync<ReportsMaintenanceViewModel>(sql, parameters);
    }

    public async Task<IEnumerable<ReportsFinancialViewModel>> GetFinancialReportsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var conditions = new List<string> { "a.IsActive = 1" };
        var parameters = new DynamicParameters();

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
            SELECT a.AssetId, a.AssetCode, a.AssetName, a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                   a.WarrantyPeriod, a.WarrantyExpiryDate, a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                   c.CategoryName, s.SupplierName,
                   (SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL) AS MaintenanceCount,
                   (SELECT SUM(MaintenanceCost) FROM AssetTransaction WHERE AssetId = a.AssetId AND MaintenanceCost IS NOT NULL) AS TotalMaintenanceCost
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId
            {whereClause}
            ORDER BY a.PurchaseDate DESC, a.AssetCode";

        return await _context.QueryAsync<ReportsFinancialViewModel>(sql, parameters);
    }

    public async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
    {
        // Simplified: single query with conditional aggregation
        const string sql = @"
            SELECT 
                COUNT(*) AS TotalAssets,
                SUM(CASE WHEN Status = 'Available' THEN 1 ELSE 0 END) AS AvailableAssets,
                SUM(CASE WHEN Status = 'Assigned' THEN 1 ELSE 0 END) AS AssignedAssets,
                SUM(CASE WHEN Status = 'On Loan' THEN 1 ELSE 0 END) AS AssetsOnLoan,
                SUM(CASE WHEN Status = 'In Maintenance' THEN 1 ELSE 0 END) AS AssetsInMaintenance,
                SUM(CASE WHEN Status = 'Under Repair' THEN 1 ELSE 0 END) AS UnderRepairAssets,
                SUM(CASE WHEN Status = 'Damaged' THEN 1 ELSE 0 END) AS DamagedAssets,
                SUM(CASE WHEN Status = 'Retired' THEN 1 ELSE 0 END) AS RetiredAssets,
                SUM(CASE WHEN WarrantyExpiryDate < GETDATE() AND WarrantyExpiryDate IS NOT NULL THEN 1 ELSE 0 END) AS ExpiredWarrantyCount,
                SUM(CASE WHEN NextMaintenanceDate BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE()) THEN 1 ELSE 0 END) AS UpcomingMaintenanceCount,
                ISNULL(SUM(PurchasePrice), 0) AS TotalAssetValue,
                (SELECT COUNT(*) FROM Employee WHERE IsActive = 1) AS TotalEmployees,
                (SELECT COUNT(*) FROM AssetTransaction WHERE TransactionStatus = 'Pending') AS PendingTransactions,
                (SELECT COUNT(*) FROM AssetTransaction WHERE TransactionDate >= DATEADD(DAY, -30, GETDATE())) AS Last30DaysTransactions,
                (SELECT COUNT(*) FROM AssetTransaction WHERE TransactionType = 'LOAN' AND TransactionStatus = 'Approved' AND PairedTransactionId IS NULL AND ExpectedReturnDate < GETDATE()) AS OverdueLoanCount
            FROM Asset
            WHERE IsActive = 1";

        return await _context.QueryFirstOrDefaultAsync<DashboardStatsViewModel>(sql) ?? new DashboardStatsViewModel();
    }
}