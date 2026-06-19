using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Asset;

/// <summary>
/// Repository implementation for Asset operations using Dapper
/// </summary>
public class AssetReps : IAssetReps
{
    private readonly DapperContext _context;

    public AssetReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // CRUD - return Entity
    // ============================================================

    public async Task<AssetEntity?> GetByIdRawAsync(int assetId)
    {
        const string sql = "SELECT * FROM Asset WHERE AssetId = @AssetId";
        return await _context.QueryFirstOrDefaultAsync<AssetEntity>(sql, new { AssetId = assetId });
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

    // ============================================================
    // GET METHODS - return View
    // ============================================================

    public async Task<AssetDetailView?> GetDetailByIdAsync(int assetId)
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId,
                c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.IsActive,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.OfficeId, o.OfficeName,
                a.Hostname, a.IpAddress, a.OperasionalOffice,
                a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
        
        var parameters = new
        {
            AssetId = assetId,
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryFirstOrDefaultAsync<AssetDetailView>(sql, parameters);
    }

    public async Task<IEnumerable<AssetListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
        
        var parameters = new
        {
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    public async Task<IEnumerable<AssetListView>> GetByCategoryListViewAsync(int categoryId)
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
        
        var parameters = new
        {
            CategoryId = categoryId,
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    public async Task<IEnumerable<AssetListView>> GetByOfficeListViewAsync(int officeId)
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
        
        var parameters = new
        {
            OfficeId = officeId,
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    public async Task<IEnumerable<AssetListView>> GetByHolderListViewAsync(int employeeId)
    {
        const string sql = @"
            SELECT DISTINCT
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
              AND at.TransactionType IN (@HandoverType, @TransferType, @LoanType)
            ORDER BY a.AssetCode";
        
        var parameters = new
        {
            EmployeeId = employeeId,
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER
        };
        
        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    public async Task<IEnumerable<AssetListView>> GetExpiredWarrantyListViewAsync()
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
        
        var parameters = new
        {
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    public async Task<IEnumerable<AssetListView>> GetUpcomingMaintenanceListViewAsync(int daysAhead = 30)
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
        
        var parameters = new
        {
            DaysAhead = daysAhead,
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    public async Task<IEnumerable<AssetListView>> GetByStatusListViewAsync(string status)
    {
        string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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

        var parameters = new
        {
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };

        if (status == "Available")
        {
            sql += " WHERE a.IsActive = 1 AND at2.AssetId IS NULL";
        }
        else if (status == "Assigned")
        {
            sql += " WHERE a.IsActive = 1 AND at2.TransactionType IN (@HandoverType, @TransferType)";
        }
        else if (status == "On Loan")
        {
            sql += " WHERE a.IsActive = 1 AND at2.TransactionType = @LoanType";
        }
        else if (status == "In Maintenance")
        {
            sql += " WHERE a.IsActive = 1 AND at2.TransactionType = @MaintenanceType";
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

        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    public async Task<IEnumerable<AssetListView>> SearchAssetsAsync(string keyword, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            return new List<AssetListView>();

        var sql = @"
            SELECT TOP (@Limit)
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
            WHERE a.IsActive = 1
              AND (a.AssetCode LIKE @Keyword 
                   OR a.AssetName LIKE @Keyword 
                   OR a.SerialNumber LIKE @Keyword
                   OR a.Brand LIKE @Keyword
                   OR a.Model LIKE @Keyword)
            ORDER BY a.AssetCode";

        var parameters = new
        {
            Keyword = $"%{keyword}%",
            Limit = limit,
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    // ============================================================
    // COUNT METHODS
    // ============================================================

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
                  AND at.TransactionType IN (@HandoverType, @TransferType, @LoanType, @MaintenanceType)
            )";
        
        var parameters = new
        {
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            LoanType = TransactionTypeConstants.LOAN,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task<int> GetAssignedAssetsCountAsync()
    {
        const string sql = @"
            SELECT COUNT(DISTINCT at.AssetId) 
            FROM AssetTransaction at
            WHERE at.TransactionType IN (@HandoverType, @TransferType)
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1";
        
        var parameters = new
        {
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER
        };
        
        return await _context.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task<int> GetAssetsOnLoanCountAsync()
    {
        const string sql = @"
            SELECT COUNT(DISTINCT at.AssetId) 
            FROM AssetTransaction at
            WHERE at.TransactionType = @LoanType
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1
              AND at.ActualReturnDate IS NULL";
        
        return await _context.ExecuteScalarAsync<int>(sql, new { LoanType = TransactionTypeConstants.LOAN });
    }

    public async Task<int> GetAssetsInMaintenanceCountAsync()
    {
        const string sql = @"
            SELECT COUNT(DISTINCT at.AssetId) 
            FROM AssetTransaction at
            WHERE at.TransactionType = @MaintenanceType
              AND at.Approved = 1 
              AND at.FromAssetTransactionId IS NULL 
              AND at.IsActive = 1
              AND at.ActualReturnDate IS NULL";
        
        return await _context.ExecuteScalarAsync<int>(sql, new { MaintenanceType = TransactionTypeConstants.MAINTENANCE });
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

    // ============================================================
    // GRID/LIST METHODS
    // ============================================================

    private string BuildBaseAssetQueryWithPagination(string selectClause, string whereClause, string orderBy, int offset, int pageSize)
    {
        return $@"
            {selectClause}
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
            {orderBy}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";
    }

    private string BuildAssetListViewSelectClauseWithParams()
    {
        return @"
            SELECT 
                a.AssetId,
                a.AssetCode,
                a.AssetName,
                a.CategoryId,
                c.CategoryName,
                a.Brand,
                a.Model,
                a.SerialNumber,
                a.Imei,
                a.MacAddress,
                a.Hostname,
                a.IpAddress,
                a.PurchaseDate,
                a.PurchasePrice,
                a.InvoiceNumber,
                a.SupplierId,
                s.SupplierName,
                a.WarrantyPeriod,
                a.WarrantyExpiryDate,
                a.AssetCondition,
                md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase,
                md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue,
                a.UsefulLife,
                a.DepreciationStartDate,
                a.OfficeId,
                o.OfficeName,
                a.OperasionalOffice,
                a.Notes,
                a.LastMaintenanceDate,
                a.NextMaintenanceDate,
                a.IsActive,
                a.CreatedDate,
                a.CreatedBy,
                a.ModifiedDate,
                a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
                    ELSE 'Available'
                END as CurrentStatus";
    }

    private (string WhereClause, DynamicParameters Parameters) BuildAssetWhereClauseForGrid(
        string? search = null, 
        string? statusFilter = null,
        int? categoryId = null,
        int? officeId = null,
        int? supplierId = null,
        bool? isActive = null,
        Dictionary<string, object>? additionalFilters = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        var loanType = TransactionTypeConstants.LOAN;
        var handoverType = TransactionTypeConstants.HANDOVER;
        var transferType = TransactionTypeConstants.TRANSFER;
        var maintenanceType = TransactionTypeConstants.MAINTENANCE;

        parameters.Add("@LoanType", loanType);
        parameters.Add("@HandoverType", handoverType);
        parameters.Add("@TransferType", transferType);
        parameters.Add("@MaintenanceType", maintenanceType);

        // Handle isActive filter
        if (isActive.HasValue)
        {
            conditions.Add($"a.IsActive = {(isActive.Value ? 1 : 0)}");
        }
        else
        {
            // Default: show all assets (both active and inactive)
            // No condition added
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "Available")
            {
                conditions.Add($@"NOT EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.FromAssetTransactionId IS NULL 
                      AND at.IsActive = 1 
                      AND at.TransactionType IN (@HandoverType,@TransferType,@LoanType,@MaintenanceType))");
            }
            else if (statusFilter == "Assigned")
            {
                conditions.Add($@"EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.FromAssetTransactionId IS NULL 
                      AND at.IsActive = 1 
                      AND at.TransactionType IN (@HandoverType,@TransferType))");
            }
            else if (statusFilter == "On Loan")
            {
                conditions.Add($@"EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.FromAssetTransactionId IS NULL 
                      AND at.IsActive = 1 
                      AND at.TransactionType = @LoanType)");
            }
            else if (statusFilter == "In Maintenance")
            {
                conditions.Add($@"EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = a.AssetId 
                      AND at.Approved = 1 
                      AND at.FromAssetTransactionId IS NULL 
                      AND at.IsActive = 1 
                      AND at.TransactionType = @MaintenanceType)");
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

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR a.SerialNumber LIKE @Search OR a.Brand LIKE @Search OR a.Model LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            conditions.Add("a.CategoryId = @CategoryId");
            parameters.Add("@CategoryId", categoryId.Value);
        }

        if (officeId.HasValue && officeId.Value > 0)
        {
            conditions.Add("a.OfficeId = @OfficeId");
            parameters.Add("@OfficeId", officeId.Value);
        }

        if (supplierId.HasValue && supplierId.Value > 0)
        {
            conditions.Add("a.SupplierId = @SupplierId");
            parameters.Add("@SupplierId", supplierId.Value);
        }

        if (additionalFilters != null)
        {
            foreach (var filter in additionalFilters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"a.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        return (whereClause, parameters);
    }

    public async Task<PaginatedResult<AssetListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        string? statusFilter = null;
        int? categoryId = null;
        int? officeId = null;
        int? supplierId = null;
        bool? isActive = null;

        if (filters != null)
        {
            if (filters.ContainsKey("status")) statusFilter = filters["status"]?.ToString();
            if (filters.ContainsKey("categoryId") && int.TryParse(filters["categoryId"]?.ToString(), out int catId)) categoryId = catId;
            if (filters.ContainsKey("officeId") && int.TryParse(filters["officeId"]?.ToString(), out int offId)) officeId = offId;
            if (filters.ContainsKey("supplierId") && int.TryParse(filters["supplierId"]?.ToString(), out int supId)) supplierId = supId;
            if (filters.ContainsKey("isActive") && bool.TryParse(filters["isActive"]?.ToString(), out bool act)) isActive = act;
            
            filters.Remove("status");
            filters.Remove("categoryId");
            filters.Remove("officeId");
            filters.Remove("supplierId");
            filters.Remove("isActive");
        }

        var (whereClause, parameters) = BuildAssetWhereClauseForGrid(search, statusFilter, categoryId, officeId, supplierId, isActive, filters);

        if (string.IsNullOrEmpty(sortBy))
        {
            sortBy = "a.AssetCode";
            sortDescending = false;
        }
        else
        {
            if (!sortBy.StartsWith("a.") && !sortBy.StartsWith("c.") && !sortBy.StartsWith("s.") && !sortBy.StartsWith("o."))
            {
                sortBy = $"a.{sortBy}";
            }
        }
        
        var orderBy = $"ORDER BY {sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $"SELECT COUNT(*) FROM Asset a {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = BuildAssetListViewSelectClauseWithParams();
        var offset = (page - 1) * pageSize;
        var dataSql = BuildBaseAssetQueryWithPagination(selectClause, whereClause, orderBy, offset, pageSize);

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<AssetListView>(dataSql, parameters);

        return new PaginatedResult<AssetListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Filters = filters,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
    }

    public async Task<IEnumerable<AssetDropdownView>> GetDropdownListAsync()
    {
        const string sql = @"
            SELECT AssetId, AssetCode, AssetName
            FROM Asset
            WHERE IsActive = 1
            ORDER BY AssetCode";

        return await _context.QueryAsync<AssetDropdownView>(sql);
    }

        // ============================================================
    // NEW: AVAILABLE ASSETS FOR TRANSACTION
    // ============================================================

    public async Task<IEnumerable<AssetDropdownView>> GetAvailableAssetsForTransactionAsync()
    {
        const string sql = @"
            SELECT a.AssetId, a.AssetCode, a.AssetName
            FROM Asset a
            WHERE a.IsActive = 1
              AND NOT EXISTS (
                  SELECT 1 FROM AssetTransaction at 
                  WHERE at.AssetId = a.AssetId 
                    AND at.Approved = 1 
                    AND at.FromAssetTransactionId IS NULL 
                    AND at.IsActive = 1
                    AND at.TransactionType IN (@HandoverType, @TransferType, @LoanType, @MaintenanceType)
                    AND at.ActualReturnDate IS NULL
              )
            ORDER BY a.AssetCode";
        
        var parameters = new
        {
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            LoanType = TransactionTypeConstants.LOAN,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryAsync<AssetDropdownView>(sql, parameters);
    }

    public async Task<bool> IsAssetAvailableForTransactionAsync(int assetId)
    {
        const string sql = @"
            SELECT CASE 
                WHEN EXISTS (
                    SELECT 1 FROM AssetTransaction at 
                    WHERE at.AssetId = @AssetId 
                      AND at.Approved = 1 
                      AND at.FromAssetTransactionId IS NULL 
                      AND at.IsActive = 1
                      AND at.TransactionType IN (@HandoverType, @TransferType, @LoanType, @MaintenanceType)
                      AND at.ActualReturnDate IS NULL
                ) THEN 0 ELSE 1 END";
        
        var parameters = new
        {
            AssetId = assetId,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            LoanType = TransactionTypeConstants.LOAN,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.ExecuteScalarAsync<int>(sql, parameters) == 1;
    }

    // ============================================================
    // NEW: ASSET STATUS BY DAMAGED/INACTIVE
    // ============================================================

    public async Task<IEnumerable<AssetListView>> GetDamagedAssetsListViewAsync()
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
            WHERE a.AssetCondition = @DamagedCondition
            ORDER BY a.AssetCode";
        
        var parameters = new
        {
            DamagedCondition = AssetConditionConstants.DAMAGED,
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    public async Task<IEnumerable<AssetListView>> GetInactiveAssetsListViewAsync()
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetCondition, md1.MasterDataName as AssetConditionName,
                a.AssetConditionPurchase, md2.MasterDataName as AssetConditionPurchaseName,
                a.ResidualValue, a.UsefulLife, a.DepreciationStartDate,
                a.Notes, a.OfficeId, o.OfficeName, a.OperasionalOffice,
                a.LastMaintenanceDate, a.NextMaintenanceDate,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                CASE 
                    WHEN at2.AssetId IS NULL THEN 'Available'
                    WHEN at2.TransactionType = @LoanType THEN 'On Loan'
                    WHEN at2.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                    WHEN at2.TransactionType = @MaintenanceType THEN 'In Maintenance'
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
            WHERE a.IsActive = 0
            ORDER BY a.AssetCode";
        
        var parameters = new
        {
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryAsync<AssetListView>(sql, parameters);
    }

    public async Task<int> GetDamagedAssetsCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM Asset WHERE AssetCondition = @DamagedCondition AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { DamagedCondition = AssetConditionConstants.DAMAGED });
    }
}