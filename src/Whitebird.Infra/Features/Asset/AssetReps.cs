using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Asset;

/// <summary>
/// Repository implementation for Asset operations using Dapper
/// (PURE INFRASTRUCTURE — NO APPLICATION DEPENDENCIES)
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
            a.AssetConditionPurchase, md.MasterDataName as AssetConditionPurchaseName,
            a.Notes, a.IsActive,
            a.Hostname, a.IpAddress,
            a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy
        FROM Asset a
        LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
        LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
        LEFT JOIN MasterData md ON a.AssetConditionPurchase = md.ReferenceCode AND md.ReferenceName = 'AssetConditionPurchase' AND md.IsActive = 1
        WHERE a.AssetId = @AssetId";

        var result = await _context.QueryFirstOrDefaultAsync<AssetDetailView>(sql, new { AssetId = assetId });

        if (result != null)
        {
            // Get Photo Profile
            var photoSql = @"
            SELECT TOP 1
                fa.FileAttachmentId,
                '/api/FileAttachment/download/' + CAST(fa.FileAttachmentId AS VARCHAR) as FileUrl
            FROM AssetPhotoProfileAttachment appa
            INNER JOIN FileAttachment fa ON appa.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            WHERE appa.AssetId = @AssetId AND appa.IsActive = 1
            ORDER BY fa.IsPrimary DESC, fa.CreatedDate DESC";

            var photo = await _context.QueryFirstOrDefaultAsync<dynamic>(photoSql, new { AssetId = assetId });
            if (photo != null)
            {
                result.PhotoProfileUrl = photo.FileUrl;
            }

            // Get Attachments
            var attSql = @"
            SELECT 
                aa.AssetAttachmentId,
                aa.FileAttachmentId,
                fa.FileName,
                fa.OriginalFileName,
                fa.FileCategory,
                fa.IsPrimary,
                aa.[Order],
                aa.CreatedDate,
                '/api/FileAttachment/download/' + CAST(fa.FileAttachmentId AS VARCHAR) as FileUrl
            FROM AssetAttachment aa
            INNER JOIN FileAttachment fa ON aa.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            WHERE aa.AssetId = @AssetId AND aa.IsActive = 1
            ORDER BY aa.[Order] ASC, fa.CreatedDate DESC";

            var attachments = await _context.QueryAsync<AssetAttachmentViewDto>(attSql, new { AssetId = assetId });
            result.Attachments = attachments.ToList();
        }

        return result;
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
                a.AssetConditionPurchase, md.MasterDataName as AssetConditionPurchaseName,
                a.Notes,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN MasterData md ON a.AssetConditionPurchase = md.ReferenceCode AND md.ReferenceName = 'AssetConditionPurchase' AND md.IsActive = 1
            ORDER BY a.AssetCode";

        return await _context.QueryAsync<AssetListView>(sql);
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
                a.AssetConditionPurchase, md.MasterDataName as AssetConditionPurchaseName,
                a.Notes,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN MasterData md ON a.AssetConditionPurchase = md.ReferenceCode AND md.ReferenceName = 'AssetConditionPurchase' AND md.IsActive = 1
            WHERE a.CategoryId = @CategoryId
            ORDER BY a.AssetCode";

        return await _context.QueryAsync<AssetListView>(sql, new { CategoryId = categoryId });
    }

    // ============================================================
    // PAGINATION
    // ============================================================

    public async Task<PaginatedResult<AssetListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
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

        if (filters != null && filters.ContainsKey("isActive") && filters["isActive"] is bool isActive)
        {
            conditions.Add($"a.IsActive = {(isActive ? 1 : 0)}");
            filters.Remove("isActive");
        }

        if (filters != null && filters.ContainsKey("status") && filters["status"] is string status)
        {
            switch (status)
            {
                case "Available":
                    conditions.Add($@"a.IsActive = 1 AND NOT EXISTS (
                        SELECT 1 FROM AssetTransaction at 
                        WHERE at.AssetId = a.AssetId 
                          AND at.Approved = 1 
                          AND at.FromAssetTransactionId IS NULL 
                          AND at.IsActive = 1 
                          AND at.TransactionType IN (@HandoverType,@TransferType,@LoanType,@MaintenanceType))");
                    break;
                case "Assigned":
                    conditions.Add($@"a.IsActive = 1 AND EXISTS (
                        SELECT 1 FROM AssetTransaction at 
                        WHERE at.AssetId = a.AssetId 
                          AND at.Approved = 1 
                          AND at.FromAssetTransactionId IS NULL 
                          AND at.IsActive = 1 
                          AND at.TransactionType IN (@HandoverType,@TransferType))");
                    break;
                case "On Loan":
                    conditions.Add($@"a.IsActive = 1 AND EXISTS (
                        SELECT 1 FROM AssetTransaction at 
                        WHERE at.AssetId = a.AssetId 
                          AND at.Approved = 1 
                          AND at.FromAssetTransactionId IS NULL 
                          AND at.IsActive = 1 
                          AND at.TransactionType = @LoanType)");
                    break;
                case "In Maintenance":
                    conditions.Add($@"a.IsActive = 1 AND EXISTS (
                        SELECT 1 FROM AssetTransaction at 
                        WHERE at.AssetId = a.AssetId 
                          AND at.Approved = 1 
                          AND at.FromAssetTransactionId IS NULL 
                          AND at.IsActive = 1 
                          AND at.TransactionType = @MaintenanceType)");
                    break;
                case "Damaged":
                    conditions.Add($@"EXISTS (
                        SELECT 1 FROM AssetTransaction at 
                        WHERE at.AssetId = a.AssetId 
                          AND at.Approved = 1 
                          AND at.IsActive = 1 
                          AND at.TransactionType = @DisposalType)");
                    break;
                case "Active":
                    conditions.Add("a.IsActive = 1");
                    break;
                case "Inactive":
                    conditions.Add("a.IsActive = 0");
                    break;
            }
            filters.Remove("status");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR a.SerialNumber LIKE @Search OR a.Brand LIKE @Search OR a.Model LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (filters != null)
        {
            foreach (var filter in filters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"a.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";

        var defaultSort = "a.AssetCode";
        var orderBy = string.IsNullOrEmpty(sortBy)
            ? $"ORDER BY {defaultSort} ASC"
            : $"ORDER BY a.{sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $"SELECT COUNT(*) FROM Asset a {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var selectClause = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, a.CategoryId, c.CategoryName,
                a.Brand, a.Model, a.SerialNumber, a.Imei, a.MacAddress,
                a.Hostname, a.IpAddress,
                a.PurchaseDate, a.PurchasePrice, a.InvoiceNumber,
                a.SupplierId, s.SupplierName,
                a.WarrantyPeriod, a.WarrantyExpiryDate,
                a.AssetConditionPurchase, md.MasterDataName as AssetConditionPurchaseName,
                a.Notes,
                a.IsActive, a.CreatedDate, a.CreatedBy, a.ModifiedDate, a.ModifiedBy,
                1 as CanEdit,
                1 as CanDelete";

        var fromClause = @"
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN Supplier s ON a.SupplierId = s.SupplierId AND s.IsActive = 1
            LEFT JOIN MasterData md ON a.AssetConditionPurchase = md.ReferenceCode AND md.ReferenceName = 'AssetConditionPurchase' AND md.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await _context.QueryAsync<AssetListView>(dataSql, parameters);

        return new PaginatedResult<AssetListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    public async Task<IEnumerable<AssetDropdownView>> GetDropdownListByTransactionTypeAsync(int transactionType)
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
                    AND at.TransactionType = @TransactionType
              )
            ORDER BY a.AssetCode";

        return await _context.QueryAsync<AssetDropdownView>(sql, new { TransactionType = transactionType });
    }

    // ============================================================
    // SOFTWARE INSTALLATION
    // ============================================================

    public async Task<SoftwareInstallationEntity?> GetSoftwareInstallationByAssetIdAsync(int assetId)
    {
        const string sql = "SELECT * FROM SoftwareInstallation WHERE AssetId = @AssetId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<SoftwareInstallationEntity>(sql, new { AssetId = assetId });
    }

    public async Task<int> InsertSoftwareInstallationAsync(SoftwareInstallationEntity entity)
    {
        const string sql = @"
            INSERT INTO SoftwareInstallation (
                AssetId, UserPassword, AdminOperationalPassword, 
                OperatingSystem, MicrosoftOffice, VisualStudio, DBMS, ConnectToNAS,
                IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @AssetId, @UserPassword, @AdminOperationalPassword,
                @OperatingSystem, @MicrosoftOffice, @VisualStudio, @DBMS, @ConnectToNAS,
                1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<int> UpdateSoftwareInstallationAsync(SoftwareInstallationEntity entity)
    {
        const string sql = @"
            UPDATE SoftwareInstallation SET
                UserPassword = @UserPassword,
                AdminOperationalPassword = @AdminOperationalPassword,
                OperatingSystem = @OperatingSystem,
                MicrosoftOffice = @MicrosoftOffice,
                VisualStudio = @VisualStudio,
                DBMS = @DBMS,
                ConnectToNAS = @ConnectToNAS,
                ModifiedDate = GETDATE(),
                ModifiedBy = @ModifiedBy
            WHERE SoftwareInstallationId = @SoftwareInstallationId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, entity);
    }

    public async Task<int> DeleteSoftwareInstallationByAssetIdAsync(int assetId)
    {
        const string sql = @"
            UPDATE SoftwareInstallation 
            SET IsActive = 0, ModifiedDate = GETDATE() 
            WHERE AssetId = @AssetId";

        return await _context.ExecuteAsync(sql, new { AssetId = assetId });
    }

    // ============================================================
    // ASSET ATTACHMENT
    // ============================================================

    public async Task<int> InsertAttachmentAsync(AssetAttachmentEntity entity)
    {
        const string sql = @"
            INSERT INTO AssetAttachment (
                AssetId, FileAttachmentId, [Order], IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @AssetId, @FileAttachmentId, @Order, 1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<IEnumerable<AssetAttachmentEntity>> GetAttachmentsByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT * FROM AssetAttachment 
            WHERE AssetId = @AssetId AND IsActive = 1
            ORDER BY [Order] ASC, CreatedDate DESC";

        return await _context.QueryAsync<AssetAttachmentEntity>(sql, new { AssetId = assetId });
    }

    public async Task<AssetAttachmentEntity?> GetAttachmentByIdAsync(int attachmentId)
    {
        const string sql = "SELECT * FROM AssetAttachment WHERE AssetAttachmentId = @AttachmentId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<AssetAttachmentEntity>(sql, new { AttachmentId = attachmentId });
    }

    public async Task<int> DeleteAttachmentAsync(int attachmentId)
    {
        const string sql = "UPDATE AssetAttachment SET IsActive = 0, ModifiedDate = GETDATE() WHERE AssetAttachmentId = @AttachmentId";
        return await _context.ExecuteAsync(sql, new { AttachmentId = attachmentId });
    }

    public async Task<int> DeleteAttachmentsByAssetIdAsync(int assetId)
    {
        const string sql = "UPDATE AssetAttachment SET IsActive = 0, ModifiedDate = GETDATE() WHERE AssetId = @AssetId";
        return await _context.ExecuteAsync(sql, new { AssetId = assetId });
    }

    // ============================================================
    // ASSET PHOTO PROFILE
    // ============================================================

    public async Task<int> InsertPhotoProfileAsync(AssetPhotoProfileAttachmentEntity entity)
    {
        const string sql = @"
            INSERT INTO AssetPhotoProfileAttachment (
                AssetId, FileAttachmentId, IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @AssetId, @FileAttachmentId, 1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<AssetPhotoProfileAttachmentEntity?> GetPhotoProfileByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT * FROM AssetPhotoProfileAttachment 
            WHERE AssetId = @AssetId AND IsActive = 1";

        return await _context.QueryFirstOrDefaultAsync<AssetPhotoProfileAttachmentEntity>(sql, new { AssetId = assetId });
    }

    public async Task<int> DeletePhotoProfileByAssetIdAsync(int assetId)
    {
        const string sql = @"
            UPDATE AssetPhotoProfileAttachment 
            SET IsActive = 0, ModifiedDate = GETDATE() 
            WHERE AssetId = @AssetId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, new { AssetId = assetId });
    }
}