using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Asset;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Asset;

public class AssetReps : IAssetReps
{
    private readonly DapperContext _context;

    public AssetReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<AssetEntity?> GetByIdRawAsync(int assetId)
    {
        const string sql = "SELECT * FROM Asset WHERE AssetId = @AssetId";
        return await _context.QueryFirstOrDefaultAsync<AssetEntity>(sql, new { AssetId = assetId });
    }

    public async Task<AssetEntity?> GetByIdWithRelationsAsync(int assetId)
    {
        const string sql = @"
            SELECT a.*, 
                   c.CategoryName, 
                   s.SupplierName,
                   o.OfficeName,
                   md1.MasterDataName as AssetConditionName,
                   md2.MasterDataName as AssetConditionPurchaseName,
                   CASE 
                       WHEN at2.AssetId IS NULL THEN 'Available'
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            WHERE a.AssetId = @AssetId";
        return await _context.QueryFirstOrDefaultAsync<AssetEntity>(sql, new { AssetId = assetId });
    }

    public async Task<IEnumerable<AssetEntity>> GetAllWithRelationsAsync()
    {
        const string sql = @"
            SELECT a.*, 
                   c.CategoryName, 
                   s.SupplierName,
                   o.OfficeName,
                   md1.MasterDataName as AssetConditionName,
                   md2.MasterDataName as AssetConditionPurchaseName,
                   CASE 
                       WHEN at2.AssetId IS NULL THEN 'Available'
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            ORDER BY a.AssetCode";
        return await _context.QueryAsync<AssetEntity>(sql);
    }

    public async Task<IEnumerable<AssetEntity>> GetByCategoryWithRelationsAsync(int categoryId)
    {
        const string sql = @"
            SELECT a.*, c.CategoryName, s.SupplierName, o.OfficeName,
                   md1.MasterDataName as AssetConditionName,
                   md2.MasterDataName as AssetConditionPurchaseName,
                   CASE 
                       WHEN at2.AssetId IS NULL THEN 'Available'
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            WHERE a.CategoryId = @CategoryId
            ORDER BY a.AssetCode";
        return await _context.QueryAsync<AssetEntity>(sql, new { CategoryId = categoryId });
    }

    public async Task<IEnumerable<AssetEntity>> GetByOfficeWithRelationsAsync(int officeId)
    {
        const string sql = @"
            SELECT a.*, c.CategoryName, s.SupplierName, o.OfficeName,
                   md1.MasterDataName as AssetConditionName,
                   md2.MasterDataName as AssetConditionPurchaseName,
                   CASE 
                       WHEN at2.AssetId IS NULL THEN 'Available'
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            WHERE a.OfficeId = @OfficeId
            ORDER BY a.AssetCode";
        return await _context.QueryAsync<AssetEntity>(sql, new { OfficeId = officeId });
    }

    public async Task<IEnumerable<AssetEntity>> GetByHolderWithRelationsAsync(int employeeId)
    {
        const string sql = @"
            SELECT DISTINCT a.*, c.CategoryName, s.SupplierName, o.OfficeName,
                   md1.MasterDataName as AssetConditionName,
                   md2.MasterDataName as AssetConditionPurchaseName,
                   CASE 
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            INNER JOIN AssetTransaction at ON a.AssetId = at.AssetId
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            WHERE at.ToEmployeeId = @EmployeeId
              AND at.Approved = 1
              AND at.FromAssetTransactionId IS NULL
              AND at.TransactionType IN (1, 2, 3)
            ORDER BY a.AssetCode";
        return await _context.QueryAsync<AssetEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<IEnumerable<AssetEntity>> GetExpiredWarrantyWithRelationsAsync()
    {
        const string sql = @"
            SELECT a.*, c.CategoryName, s.SupplierName, o.OfficeName,
                   md1.MasterDataName as AssetConditionName,
                   md2.MasterDataName as AssetConditionPurchaseName,
                   CASE 
                       WHEN at2.AssetId IS NULL THEN 'Available'
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            WHERE a.WarrantyExpiryDate < GETDATE() AND a.WarrantyExpiryDate IS NOT NULL
            ORDER BY a.WarrantyExpiryDate";
        return await _context.QueryAsync<AssetEntity>(sql);
    }

    public async Task<IEnumerable<AssetEntity>> GetUpcomingMaintenanceWithRelationsAsync(int daysAhead = 30)
    {
        const string sql = @"
            SELECT a.*, c.CategoryName, s.SupplierName, o.OfficeName,
                   md1.MasterDataName as AssetConditionName,
                   md2.MasterDataName as AssetConditionPurchaseName,
                   CASE 
                       WHEN at2.AssetId IS NULL THEN 'Available'
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            WHERE a.NextMaintenanceDate BETWEEN GETDATE() AND DATEADD(DAY, @DaysAhead, GETDATE())
            ORDER BY a.NextMaintenanceDate";
        return await _context.QueryAsync<AssetEntity>(sql, new { DaysAhead = daysAhead });
    }

    public async Task<IEnumerable<AssetEntity>> GetByStatusWithRelationsAsync(string status)
    {
        string sql = @"
            SELECT a.*, 
                   c.CategoryName, 
                   s.SupplierName,
                   o.OfficeName,
                   md1.MasterDataName as AssetConditionName,
                   md2.MasterDataName as AssetConditionPurchaseName,
                   CASE 
                       WHEN at2.AssetId IS NULL THEN 'Available'
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1";

        if (status == "Available")
        {
            sql += " WHERE a.IsActive = 1 AND at2.AssetId IS NULL";
        }
        else if (status == "Assigned")
        {
            sql += " WHERE a.IsActive = 1 AND at2.TransactionType IN (1,2)";
        }
        else if (status == "On Loan")
        {
            sql += " WHERE a.IsActive = 1 AND at2.TransactionType = 3";
        }
        else if (status == "In Maintenance")
        {
            sql += " WHERE a.IsActive = 1 AND at2.TransactionType = 6";
        }
        else if (status == "Active")
        {
            sql += " WHERE a.IsActive = 1";
        }
        else if (status == "Inactive")
        {
            sql += " WHERE a.IsActive = 0";
        }
        else
        {
            sql += " WHERE a.IsActive = 1";
        }

        sql += " ORDER BY a.AssetCode";

        return await _context.QueryAsync<AssetEntity>(sql);
    }

    public async Task<bool> IsAssetCodeExistsAsync(string assetCode, int? excludeAssetId = null)
    {
        var sql = "SELECT COUNT(1) FROM Asset WHERE AssetCode = @AssetCode";
        var parameters = new DynamicParameters();
        parameters.Add("@AssetCode", assetCode);

        if (excludeAssetId.HasValue)
        {
            sql += " AND AssetId != @ExcludeAssetId";
            parameters.Add("@ExcludeAssetId", excludeAssetId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<PaginatedResult<AssetEntity>> GetPagedWithRelationsAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        // Extract status filter from filters dictionary
        string? statusFilter = null;
        if (filters != null && filters.ContainsKey("status"))
        {
            statusFilter = filters["status"]?.ToString();
            filters.Remove("status");
        }

        // Apply status filter based on business rules
        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "Available")
            {
                conditions.Add("a.IsActive = 1");
                conditions.Add("NOT EXISTS (SELECT 1 FROM AssetTransaction at WHERE at.AssetId = a.AssetId AND at.Approved = 1 AND at.FromAssetTransactionId IS NULL AND at.IsActive = 1 AND at.TransactionType IN (1,2,3,6))");
            }
            else if (statusFilter == "Assigned")
            {
                conditions.Add("a.IsActive = 1");
                conditions.Add("EXISTS (SELECT 1 FROM AssetTransaction at WHERE at.AssetId = a.AssetId AND at.Approved = 1 AND at.FromAssetTransactionId IS NULL AND at.IsActive = 1 AND at.TransactionType IN (1,2))");
            }
            else if (statusFilter == "On Loan")
            {
                conditions.Add("a.IsActive = 1");
                conditions.Add("EXISTS (SELECT 1 FROM AssetTransaction at WHERE at.AssetId = a.AssetId AND at.Approved = 1 AND at.FromAssetTransactionId IS NULL AND at.IsActive = 1 AND at.TransactionType = 3)");
            }
            else if (statusFilter == "In Maintenance")
            {
                conditions.Add("a.IsActive = 1");
                conditions.Add("EXISTS (SELECT 1 FROM AssetTransaction at WHERE at.AssetId = a.AssetId AND at.Approved = 1 AND at.FromAssetTransactionId IS NULL AND at.IsActive = 1 AND at.TransactionType = 6)");
            }
            else if (statusFilter == "Active")
            {
                conditions.Add("a.IsActive = 1");
            }
            else if (statusFilter == "Inactive")
            {
                conditions.Add("a.IsActive = 0");
            }
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR a.SerialNumber LIKE @Search OR a.Brand LIKE @Search OR a.Model LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        // Apply other filters (category, office, etc.)
        if (filters != null)
        {
            foreach (var filter in filters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"a.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        sortBy = string.IsNullOrEmpty(sortBy) ? "a.AssetCode" : $"a.{sortBy}";
        var orderBy = $"{sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Asset a
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT a.*, c.CategoryName, s.SupplierName, o.OfficeName,
                   md1.MasterDataName as AssetConditionName,
                   md2.MasterDataName as AssetConditionPurchaseName,
                   CASE 
                       WHEN at2.AssetId IS NULL THEN 'Available'
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN Office o ON a.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON a.AssetCondition = md1.ReferenceCode AND md1.ReferenceName = 'AssetCondition' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON a.AssetConditionPurchase = md2.ReferenceCode AND md2.ReferenceName = 'AssetConditionPurchase' AND md2.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            {whereClause}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<AssetEntity>(dataSql, parameters);

        return new PaginatedResult<AssetEntity>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<int> GetTotalAssetsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Asset";
        return await _context.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetAvailableAssetsCountAsync()
    {
        const string sql = @"
            SELECT COUNT(*) FROM Asset a 
            WHERE a.IsActive = 1 
            AND NOT EXISTS (
                SELECT 1 FROM AssetTransaction at 
                WHERE at.AssetId = a.AssetId 
                  AND at.Approved = 1 
                  AND at.FromAssetTransactionId IS NULL 
                  AND at.IsActive = 1
                  AND at.TransactionType IN (1, 2, 3, 6)
            )";
        return await _context.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetAssignedAssetsCountAsync()
    {
        const string sql = @"
            SELECT COUNT(DISTINCT at.AssetId) 
            FROM AssetTransaction at
            WHERE at.TransactionType IN (1, 2)
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetAssetsOnLoanCountAsync()
    {
        const string sql = @"
            SELECT COUNT(DISTINCT at.AssetId) 
            FROM AssetTransaction at
            WHERE at.TransactionType = 3
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1
              AND at.ActualReturnDate IS NULL";
        return await _context.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetAssetsInMaintenanceCountAsync()
    {
        const string sql = @"
            SELECT COUNT(DISTINCT at.AssetId) 
            FROM AssetTransaction at
            WHERE at.TransactionType = 6
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1
              AND at.ActualReturnDate IS NULL";
        return await _context.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetExpiredWarrantyCountAsync()
    {
        const string sql = @"
            SELECT COUNT(*) FROM Asset 
            WHERE WarrantyExpiryDate < GETDATE() AND WarrantyExpiryDate IS NOT NULL";
        return await _context.ExecuteScalarAsync<int>(sql);
    }

    public async Task<int> GetUpcomingMaintenanceCountAsync(int daysAhead = 30)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Asset 
            WHERE NextMaintenanceDate BETWEEN GETDATE() AND DATEADD(DAY, @DaysAhead, GETDATE())";
        return await _context.ExecuteScalarAsync<int>(sql, new { DaysAhead = daysAhead });
    }

    public async Task<decimal> GetTotalAssetValueAsync()
    {
        const string sql = "SELECT ISNULL(SUM(PurchasePrice), 0) FROM Asset";
        return await _context.ExecuteScalarAsync<decimal>(sql);
    }

    public async Task<IEnumerable<AssetEntity>> SearchAssetsAsync(string keyword, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            return new List<AssetEntity>();

        var sql = @"
            SELECT TOP (@Limit) a.*, c.CategoryName, 
                   CASE 
                       WHEN at2.AssetId IS NULL THEN 'Available'
                       WHEN at2.TransactionType = 3 THEN 'On Loan'
                       WHEN at2.TransactionType IN (1,2) THEN 'Assigned'
                       WHEN at2.TransactionType = 6 THEN 'In Maintenance'
                       ELSE 'Available'
                   END as CurrentStatus
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN (
                SELECT AssetId, TransactionType,
                       ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
                FROM AssetTransaction
                WHERE Approved = 1 AND FromAssetTransactionId IS NULL AND IsActive = 1
            ) at2 ON a.AssetId = at2.AssetId AND at2.rn = 1
            WHERE a.IsActive = 1
              AND (a.AssetCode LIKE @Keyword 
                   OR a.AssetName LIKE @Keyword 
                   OR a.SerialNumber LIKE @Keyword
                   OR a.Brand LIKE @Keyword
                   OR a.Model LIKE @Keyword)
            ORDER BY a.AssetCode";

        var parameters = new { Keyword = $"%{keyword}%", Limit = limit };
        return await _context.QueryAsync<AssetEntity>(sql, parameters);
    }
}