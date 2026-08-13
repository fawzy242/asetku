using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.AssetTracking;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTracking;

/// <summary>
/// Repository implementation for Asset Tracking operations using Dapper
/// </summary>
public class AssetTrackingReps : IAssetTrackingReps
{
    private readonly DapperContext _context;

    public AssetTrackingReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // LOOKUP / DROPDOWN
    // ============================================================

    public async Task<AssetTrackingGetDataDto?> GetAssetDataAsync(int assetId)
    {
        const string sql = @"
            SELECT 
                a.AssetId, a.AssetCode, a.AssetName, 
                c.CategoryName, a.IsActive
            FROM Asset a
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            WHERE a.AssetId = @AssetId";

        return await _context.QueryFirstOrDefaultAsync<AssetTrackingGetDataDto>(sql, new { AssetId = assetId });
    }

    // ============================================================
    // DETAIL TRACKING
    // ============================================================

    public async Task<AssetTrackingView?> GetTrackingDetailAsync(int assetId)
    {
        var asset = await GetAssetStatsAsync(assetId);
        if (asset == null) return null;

        var timeline = await GetTimelineAsync(assetId);
        var software = await GetSoftwareInstallationByAssetIdAsync(assetId);
        var attachments = await GetAttachmentsByAssetIdAsync(assetId);
        var photoProfile = await GetPhotoProfileByAssetIdAsync(assetId);
        var totalTransactions = await GetTransactionCountAsync(assetId);

        return new AssetTrackingView
        {
            AssetId = asset.AssetId,
            AssetCode = asset.AssetCode,
            AssetName = asset.AssetName,
            CategoryName = asset.CategoryName,
            Brand = asset.Brand,
            Model = asset.Model,
            SerialNumber = asset.SerialNumber,
            CurrentStatus = asset.CurrentStatus,
            CurrentHolderName = asset.CurrentHolderName,
            CurrentLocation = asset.CurrentLocation,
            ConditionName = asset.ConditionName,
            IsOnLoan = asset.IsOnLoan,
            IsInMaintenance = asset.IsInMaintenance,
            IsOverdue = asset.IsOverdue,
            LoanDueDate = asset.LoanDueDate,
            PurchaseDate = asset.PurchaseDate,
            PurchasePrice = asset.PurchasePrice,
            WarrantyExpiryDate = asset.WarrantyExpiryDate,
            TotalTransactions = totalTransactions,
            PhotoProfileUrl = photoProfile?.FileUrl,
            Attachments = attachments.ToList(),
            SoftwareInstallation = software,
            Timeline = timeline.ToList()
        };
    }

    private async Task<int> GetTransactionCountAsync(int assetId)
    {
        const string sql = "SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = @AssetId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { AssetId = assetId });
    }

    // ============================================================
    // STATS HEADER
    // ============================================================

    public async Task<AssetStatsView?> GetAssetStatsAsync(int assetId)
    {
        const string sql = @"
        SELECT 
            a.AssetId, a.AssetCode, a.AssetName, a.Brand, a.Model, a.SerialNumber,
            a.PurchaseDate, a.PurchasePrice, a.WarrantyExpiryDate,
            c.CategoryName,
            e.FullName as CurrentHolderName,
            o.OfficeName as CurrentLocation,
            mdCondition.MasterDataName as ConditionName,
            CASE 
                WHEN a.IsActive = 0 THEN 'Inactive'
                WHEN lt.AssetId IS NULL THEN 'Available'
                WHEN lt.TransactionType = @LoanType THEN 'On Loan'
                WHEN lt.TransactionType IN (@HandoverType, @TransferType) THEN 'Assigned'
                WHEN lt.TransactionType = @MaintenanceType THEN 'In Maintenance'
                ELSE 'Available'
            END as CurrentStatus,
            CASE WHEN lt.TransactionType = @LoanType THEN 1 ELSE 0 END as IsOnLoan,
            CASE WHEN lt.TransactionType = @MaintenanceType THEN 1 ELSE 0 END as IsInMaintenance,
            CASE 
                WHEN lt.TransactionType = @LoanType AND lt.ExpectedLoanReturnDate < GETDATE() THEN 1 
                ELSE 0 
            END as IsOverdue,
            lt.ExpectedLoanReturnDate as LoanDueDate
        FROM Asset a
        LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
        LEFT JOIN Employee e ON lt.EmployeeId = e.EmployeeId
        LEFT JOIN Office o ON lt.OfficeId = o.OfficeId AND o.IsActive = 1
        LEFT JOIN MasterData mdCondition ON lt.Condition = mdCondition.ReferenceCode AND mdCondition.ReferenceName = 'AssetCondition' AND mdCondition.IsActive = 1
        LEFT JOIN (
            SELECT AssetId, TransactionType, EmployeeId, OfficeId, Condition, ExpectedLoanReturnDate,
                   ROW_NUMBER() OVER (PARTITION BY AssetId ORDER BY TransactionDate DESC) as rn
            FROM AssetTransaction
            WHERE Approved = 1 AND IsActive = 1 AND FromAssetTransactionId IS NULL
        ) lt ON a.AssetId = lt.AssetId AND lt.rn = 1
        WHERE a.AssetId = @AssetId";

        var parameters = new
        {
            AssetId = assetId,
            LoanType = TransactionTypeConstants.LOAN,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };

        return await _context.QueryFirstOrDefaultAsync<AssetStatsView>(sql, parameters);
    }

    // ============================================================
    // TIMELINE - FIXED (hapus FromEmployeeId dan ToEmployeeId)
    // ============================================================

    public async Task<IEnumerable<AssetTimelineEntry>> GetTimelineAsync(int assetId)
    {
        const string sql = @"
        SELECT 
            t.AssetTransactionId as Id,
            t.TransactionDate as Date,
            mdType.MasterDataName as ActivityType,
            COALESCE(t.Notes, '') as Description,
            e.FullName as EmployeeName,
            mdCondition.MasterDataName as Status,
            t.Notes
        FROM AssetTransaction t
        LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
        LEFT JOIN MasterData mdType ON t.TransactionType = mdType.ReferenceCode AND mdType.ReferenceName = 'TransactionType' AND mdType.IsActive = 1
        LEFT JOIN MasterData mdCondition ON t.Condition = mdCondition.ReferenceCode AND mdCondition.ReferenceName = 'AssetCondition' AND mdCondition.IsActive = 1
        WHERE t.AssetId = @AssetId AND t.IsActive = 1
        ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<AssetTimelineEntry>(sql, new { AssetId = assetId });
    }

    // ============================================================
    // SOFTWARE INSTALLATION
    // ============================================================

    public async Task<AssetTrackingSoftwareInstallationDto?> GetSoftwareInstallationByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT 
                si.OperatingSystem, si.MicrosoftOffice, si.VisualStudio, si.DBMS,
                si.UserPassword, si.AdminOperationalPassword, si.ConnectToNAS,
                mdOS.MasterDataName as OperatingSystemName,
                mdOffice.MasterDataName as MicrosoftOfficeName,
                mdVS.MasterDataName as VisualStudioName,
                mdDBMS.MasterDataName as DBMSName
            FROM SoftwareInstallation si
            LEFT JOIN MasterData mdOS ON si.OperatingSystem = mdOS.ReferenceCode AND mdOS.ReferenceName = 'OperatingSystem' AND mdOS.IsActive = 1
            LEFT JOIN MasterData mdOffice ON si.MicrosoftOffice = mdOffice.ReferenceCode AND mdOffice.ReferenceName = 'MicrosoftOffice' AND mdOffice.IsActive = 1
            LEFT JOIN MasterData mdVS ON si.VisualStudio = mdVS.ReferenceCode AND mdVS.ReferenceName = 'VisualStudio' AND mdVS.IsActive = 1
            LEFT JOIN MasterData mdDBMS ON si.DBMS = mdDBMS.ReferenceCode AND mdDBMS.ReferenceName = 'DBMS' AND mdDBMS.IsActive = 1
            WHERE si.AssetId = @AssetId AND si.IsActive = 1";

        return await _context.QueryFirstOrDefaultAsync<AssetTrackingSoftwareInstallationDto>(sql, new { AssetId = assetId });
    }

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    public async Task<IEnumerable<AssetTrackingAttachmentDto>> GetAttachmentsByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT 
                fa.FileAttachmentId, fa.FileName, fa.OriginalFileName, fa.FileCategory, fa.IsPrimary,
                '/api/FileAttachment/download/' + CAST(fa.FileAttachmentId AS VARCHAR) as FileUrl
            FROM AssetAttachment aa
            INNER JOIN FileAttachment fa ON aa.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            WHERE aa.AssetId = @AssetId AND aa.IsActive = 1
            ORDER BY aa.Order ASC, fa.CreatedDate DESC";

        return await _context.QueryAsync<AssetTrackingAttachmentDto>(sql, new { AssetId = assetId });
    }

    public async Task<AssetTrackingAttachmentDto?> GetPhotoProfileByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT TOP 1
                fa.FileAttachmentId, fa.FileName, fa.OriginalFileName, fa.FileCategory, fa.IsPrimary,
                '/api/FileAttachment/download/' + CAST(fa.FileAttachmentId AS VARCHAR) as FileUrl
            FROM AssetPhotoProfileAttachment appa
            INNER JOIN FileAttachment fa ON appa.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            WHERE appa.AssetId = @AssetId AND appa.IsActive = 1
            ORDER BY fa.IsPrimary DESC, fa.CreatedDate DESC";

        return await _context.QueryFirstOrDefaultAsync<AssetTrackingAttachmentDto>(sql, new { AssetId = assetId });
    }

    // ============================================================
    // SCHEDULE STATS - Using GridRequest
    // ============================================================

    public async Task<PaginatedResult<AssetTrackingScheduleStatsDto>> GetScheduleStatsAsync(AssetTrackingGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        parameters.Add("@AssetId", request.AssetId);

        if (string.IsNullOrWhiteSpace(request.SortBy))
        {
            request.SortBy = "s.ScheduleDate";
            request.SortDescending = true;
        }

        string tableName, idColumn, employeeColumn, typeColumn, statusColumn;

        switch (request.Search?.ToLower())
        {
            case "handover":
                tableName = "ScheduleHandover";
                idColumn = "ScheduleHandoverId";
                employeeColumn = "EmployeeId";
                typeColumn = "'Handover'";
                statusColumn = "CASE WHEN HandoverAssetTransactionId IS NOT NULL THEN 'Completed' ELSE 'Pending' END";
                break;
            case "return":
                tableName = "ScheduleReturn";
                idColumn = "ScheduleReturnId";
                employeeColumn = "EmployeeId";
                typeColumn = "'Return'";
                statusColumn = "CASE WHEN ReturnAssetTransactionId IS NOT NULL THEN 'Completed' ELSE 'Pending' END";
                break;
            case "maintenance":
                tableName = "ScheduleMaintenance";
                idColumn = "ScheduleMaintenanceId";
                employeeColumn = "EmployeeId";
                typeColumn = "'Maintenance'";
                statusColumn = "CASE WHEN MaintenanceAssetTransactionId IS NOT NULL THEN 'Completed' ELSE 'Pending' END";
                break;
            default:
                return new PaginatedResult<AssetTrackingScheduleStatsDto>();
        }

        conditions.Add($"s.AssetId = @AssetId");
        conditions.Add("s.IsActive = 1");

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "s.ScheduleDate", true);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*)
            FROM {tableName} s
            LEFT JOIN Asset a ON s.AssetId = a.AssetId
            LEFT JOIN Employee e ON s.{employeeColumn} = e.EmployeeId
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @$"
            SELECT 
                s.{idColumn} as ScheduleId,
                s.ScheduleDate,
                a.AssetCode,
                a.AssetName,
                e.FullName as EmployeeName,
                {statusColumn} as Status,
                {typeColumn} as Type,
                s.{idColumn} as RelatedTransactionId,
                s.IsActive";

        var fromClause = $@"
            FROM {tableName} s
            LEFT JOIN Asset a ON s.AssetId = a.AssetId
            LEFT JOIN Employee e ON s.{employeeColumn} = e.EmployeeId";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<AssetTrackingScheduleStatsDto>(dataSql, parameters);

        return new PaginatedResult<AssetTrackingScheduleStatsDto>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // TRANSACTION TYPE STATS
    // ============================================================

    public async Task<AssetTrackingTransactionTypeStatsResponse> GetTransactionTypeStatsAsync(int assetId)
    {
        const string sql = @"
            SELECT 
                md.MasterDataName as TransactionType,
                COUNT(*) as Count
            FROM AssetTransaction t
            LEFT JOIN MasterData md ON t.TransactionType = md.ReferenceCode AND md.ReferenceName = 'TransactionType' AND md.IsActive = 1
            WHERE t.AssetId = @AssetId AND t.IsActive = 1
            GROUP BY md.MasterDataName
            ORDER BY Count DESC";

        var data = await _context.QueryAsync<AssetTrackingTransactionTypeStatsDto>(sql, new { AssetId = assetId });

        var totalCountSql = "SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = @AssetId AND IsActive = 1";
        var totalCount = await _context.ExecuteScalarAsync<int>(totalCountSql, new { AssetId = assetId });

        var result = data.ToList();
        foreach (var item in result)
        {
            item.Percentage = totalCount > 0 ? Math.Round((decimal)item.Count / totalCount * 100, 1) : 0;
        }

        return new AssetTrackingTransactionTypeStatsResponse
        {
            TotalTransactions = totalCount,
            Data = result
        };
    }

    // ============================================================
    // EMPLOYEE HISTORY - Using GridRequest
    // ============================================================

    public async Task<PaginatedResult<AssetTrackingEmployeeHistoryDto>> GetEmployeeHistoryAsync(AssetTrackingGridRequest request)
    {
        var conditions = new List<string>
        {
            "t.AssetId = @AssetId",
            "t.IsActive = 1"
        };
        var parameters = new DynamicParameters();
        parameters.Add("@AssetId", request.AssetId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(e.FullName LIKE @Search OR e.EmployeeCode LIKE @Search OR d.DepartmentName LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "MAX(t.TransactionDate)", true);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(DISTINCT e.EmployeeId)
            FROM AssetTransaction t
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                e.EmployeeId,
                e.EmployeeCode,
                e.FullName,
                d.DepartmentName,
                mdPos.MasterDataName as PositionName,
                MAX(t.TransactionDate) as LastTransactionDate,
                COUNT(t.AssetTransactionId) as TotalTransactions,
                COUNT(DISTINCT t.AssetId) as AssetsHeld";

        var fromClause = @"
            FROM AssetTransaction t
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId
            LEFT JOIN MasterData mdPos ON e.Position = mdPos.ReferenceCode AND mdPos.ReferenceName = 'Position' AND mdPos.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            GROUP BY e.EmployeeId, e.EmployeeCode, e.FullName, d.DepartmentName, mdPos.MasterDataName
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<AssetTrackingEmployeeHistoryDto>(dataSql, parameters);

        return new PaginatedResult<AssetTrackingEmployeeHistoryDto>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}