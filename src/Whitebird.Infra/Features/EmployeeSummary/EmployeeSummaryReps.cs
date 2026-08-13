using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.EmployeeSummary;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.EmployeeSummary;

/// <summary>
/// Repository implementation for Employee Summary operations using Dapper
/// </summary>
public class EmployeeSummaryReps : IEmployeeSummaryReps
{
    private readonly DapperContext _context;

    public EmployeeSummaryReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // LOOKUP
    // ============================================================

    public async Task<EmployeeSummaryGetDataDto?> GetEmployeeDataAsync(int employeeId)
    {
        const string sql = @"
            SELECT 
                e.EmployeeId, e.EmployeeCode, e.FullName, 
                d.DepartmentName, e.IsActive
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            WHERE e.EmployeeId = @EmployeeId";

        return await _context.QueryFirstOrDefaultAsync<EmployeeSummaryGetDataDto>(sql, new { EmployeeId = employeeId });
    }

    // ============================================================
    // COMPLETE SUMMARY
    // ============================================================

    public async Task<EmployeeSummaryView?> GetSummaryAsync(int employeeId)
    {
        var employee = await GetEmployeeDataAsync(employeeId);
        if (employee == null) return null;

        var stats = await GetStatsAsync(employeeId);
        var currentHolding = await GetCurrentHoldingAsync(new EmployeeSummaryGridRequest { EmployeeId = employeeId, Page = 1, PageSize = 100 });
        var assetsHistory = await GetAssetsHistoryAsync(new EmployeeSummaryGridRequest { EmployeeId = employeeId, Page = 1, PageSize = 100 });
        var transactionHistory = await GetTransactionHistoryAsync(new EmployeeSummaryGridRequest { EmployeeId = employeeId, Page = 1, PageSize = 100 });
        var attachments = await GetAttachmentsByEmployeeIdAsync(employeeId);
        var photoProfile = await GetPhotoProfileByEmployeeIdAsync(employeeId);

        return new EmployeeSummaryView
        {
            EmployeeId = employee.EmployeeId,
            EmployeeCode = employee.EmployeeCode,
            FullName = employee.FullName,
            DepartmentName = employee.DepartmentName,
            PositionName = await GetPositionNameAsync(employeeId),
            EmploymentStatusName = await GetEmploymentStatusNameAsync(employeeId),
            JoinDate = await GetJoinDateAsync(employeeId),
            ResignDate = await GetResignDateAsync(employeeId),
            IsActive = employee.IsActive,
            PhotoProfileUrl = photoProfile?.FileUrl,
            Attachments = attachments.ToList(),
            Stats = stats,
            CurrentHolding = currentHolding.Data.ToList(),
            AssetsHistory = assetsHistory.Data.ToList(),
            TransactionHistory = transactionHistory.Data.ToList()
        };
    }

    private async Task<string?> GetPositionNameAsync(int employeeId)
    {
        const string sql = @"
            SELECT md.MasterDataName 
            FROM Employee e
            LEFT JOIN MasterData md ON e.Position = md.ReferenceCode AND md.ReferenceName = 'Position' AND md.IsActive = 1
            WHERE e.EmployeeId = @EmployeeId";
        return await _context.ExecuteScalarAsync<string?>(sql, new { EmployeeId = employeeId });
    }

    private async Task<string?> GetEmploymentStatusNameAsync(int employeeId)
    {
        const string sql = @"
            SELECT md.MasterDataName 
            FROM Employee e
            LEFT JOIN MasterData md ON e.EmploymentStatus = md.ReferenceCode AND md.ReferenceName = 'EmployeeStatus' AND md.IsActive = 1
            WHERE e.EmployeeId = @EmployeeId";
        return await _context.ExecuteScalarAsync<string?>(sql, new { EmployeeId = employeeId });
    }

    private async Task<DateTime?> GetJoinDateAsync(int employeeId)
    {
        const string sql = "SELECT JoinDate FROM Employee WHERE EmployeeId = @EmployeeId";
        return await _context.ExecuteScalarAsync<DateTime?>(sql, new { EmployeeId = employeeId });
    }

    private async Task<DateTime?> GetResignDateAsync(int employeeId)
    {
        const string sql = "SELECT ResignDate FROM Employee WHERE EmployeeId = @EmployeeId";
        return await _context.ExecuteScalarAsync<DateTime?>(sql, new { EmployeeId = employeeId });
    }

    // ============================================================
    // STATISTICS
    // ============================================================

    public async Task<EmployeeSummaryStatsDto> GetStatsAsync(int employeeId)
    {
        const string sql = @"
            SELECT 
                (SELECT COUNT(DISTINCT AssetId) 
                 FROM AssetTransaction 
                 WHERE EmployeeId = @EmployeeId 
                   AND Approved = 1 
                   AND FromAssetTransactionId IS NULL 
                   AND IsActive = 1
                   AND TransactionType IN (@HandoverType, @TransferType, @LoanType)) AS CurrentlyHeldAssets,

                (SELECT COUNT(DISTINCT AssetId) 
                 FROM AssetTransaction 
                 WHERE EmployeeId = @EmployeeId 
                   AND Approved = 1 
                   AND FromAssetTransactionId IS NULL 
                   AND IsActive = 1
                   AND TransactionType = @LoanType) AS AssetsOnLoan,

                (SELECT COUNT(*) 
                 FROM AssetTransaction 
                 WHERE EmployeeId = @EmployeeId 
                   AND Approved = 1 
                   AND FromAssetTransactionId IS NULL 
                   AND IsActive = 1
                   AND TransactionType = @LoanType 
                   AND ExpectedLoanReturnDate < GETDATE()) AS OverdueLoans,

                (SELECT COUNT(DISTINCT AssetId) 
                 FROM AssetTransaction 
                 WHERE (EmployeeId = @EmployeeId)
                   AND IsActive = 1) AS TotalHistoricalAssets,

                (SELECT COUNT(*) 
                 FROM AssetTransaction 
                 WHERE EmployeeId = @EmployeeId 
                   AND Approved = 1 
                   AND IsActive = 1
                   AND TransactionType IN (@ReturnType, @LoanReturnType)) AS ReturnedAssets,

                (SELECT COUNT(*) 
                 FROM AssetTransaction 
                 WHERE EmployeeId = @EmployeeId 
                   AND Approved = 1 
                   AND IsActive = 1
                   AND TransactionType IN (@ReturnType, @LoanReturnType)
                   AND Condition = @DamagedCondition) AS DamagedReturns";

        var parameters = new
        {
            EmployeeId = employeeId,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            LoanType = TransactionTypeConstants.LOAN,
            ReturnType = TransactionTypeConstants.RETURN,
            LoanReturnType = TransactionTypeConstants.LOAN_RETURN,
            DamagedCondition = AssetConditionConstants.DAMAGED
        };

        return await _context.QueryFirstOrDefaultAsync<EmployeeSummaryStatsDto>(sql, parameters)
               ?? new EmployeeSummaryStatsDto();
    }

    // ============================================================
    // CURRENT HOLDING - Using GridRequest
    // ============================================================

    public async Task<PaginatedResult<EmployeeCurrentHoldingDto>> GetCurrentHoldingAsync(EmployeeSummaryGridRequest request)
    {
        var conditions = new List<string>
        {
            "at.EmployeeId = @EmployeeId",
            "at.Approved = 1",
            "at.FromAssetTransactionId IS NULL",
            "at.IsActive = 1",
            "at.TransactionType IN (@HandoverType, @TransferType, @LoanType)"
        };
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", request.EmployeeId);
        parameters.Add("@HandoverType", TransactionTypeConstants.HANDOVER);
        parameters.Add("@TransferType", TransactionTypeConstants.TRANSFER);
        parameters.Add("@LoanType", TransactionTypeConstants.LOAN);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR a.SerialNumber LIKE @Search OR c.CategoryName LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "at.TransactionDate", true);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(DISTINCT at.AssetId)
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT DISTINCT
                a.AssetId,
                a.AssetCode,
                a.AssetName,
                c.CategoryName,
                CASE 
                    WHEN at.TransactionType = @LoanType THEN 'On Loan'
                    ELSE 'Assigned'
                END AS Status,
                CASE 
                    WHEN at.TransactionType = @LoanType THEN 'On Loan'
                    ELSE 'Assigned'
                END AS AssociationType,
                at.TransactionDate AS SinceDate,
                at.ExpectedLoanReturnDate AS ExpectedReturnDate,
                CASE 
                    WHEN at.TransactionType = @LoanType AND at.ExpectedLoanReturnDate < GETDATE() THEN 1 
                    ELSE 0 
                END AS IsOverdue,
                md.MasterDataName AS ConditionName";

        var fromClause = @"
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN MasterData md ON at.Condition = md.ReferenceCode AND md.ReferenceName = 'AssetCondition' AND md.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<EmployeeCurrentHoldingDto>(dataSql, parameters);

        return new PaginatedResult<EmployeeCurrentHoldingDto>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // ASSETS HISTORY - Using GridRequest
    // ============================================================

    public async Task<PaginatedResult<EmployeeAssetsHistoryDto>> GetAssetsHistoryAsync(EmployeeSummaryGridRequest request)
    {
        var conditions = new List<string>
        {
            "at.EmployeeId = @EmployeeId",
            "at.Approved = 1",
            "at.FromAssetTransactionId IS NULL",
            "at.IsActive = 1",
            "at.TransactionType IN (@HandoverType, @TransferType, @LoanType)",
            "NOT EXISTS (SELECT 1 FROM AssetTransaction at2 WHERE at2.AssetId = at.AssetId AND at2.EmployeeId = @EmployeeId AND at2.Approved = 1 AND at2.FromAssetTransactionId IS NULL AND at2.IsActive = 1 AND at2.TransactionType IN (@HandoverType, @TransferType, @LoanType) AND at2.AssetTransactionId > at.AssetTransactionId)"
        };
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", request.EmployeeId);
        parameters.Add("@HandoverType", TransactionTypeConstants.HANDOVER);
        parameters.Add("@TransferType", TransactionTypeConstants.TRANSFER);
        parameters.Add("@LoanType", TransactionTypeConstants.LOAN);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR a.SerialNumber LIKE @Search OR c.CategoryName LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "at.TransactionDate", true);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(DISTINCT at.AssetId)
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                a.AssetId,
                a.AssetCode,
                a.AssetName,
                c.CategoryName,
                MAX(at.TransactionDate) AS LastHeldDate,
                e2.FullName AS CurrentHolderName,
                e2.EmployeeId AS CurrentHolderId,
                md.MasterDataName AS ConditionName";

        var fromClause = @"
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN AssetTransaction at_current ON at_current.AssetId = a.AssetId 
                AND at_current.Approved = 1 
                AND at_current.FromAssetTransactionId IS NULL 
                AND at_current.IsActive = 1
                AND at_current.TransactionType IN (@HandoverType, @TransferType, @LoanType)
            LEFT JOIN Employee e2 ON at_current.EmployeeId = e2.EmployeeId
            LEFT JOIN MasterData md ON at.Condition = md.ReferenceCode AND md.ReferenceName = 'AssetCondition' AND md.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            GROUP BY a.AssetId, a.AssetCode, a.AssetName, c.CategoryName, e2.FullName, e2.EmployeeId, md.MasterDataName
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<EmployeeAssetsHistoryDto>(dataSql, parameters);

        return new PaginatedResult<EmployeeAssetsHistoryDto>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // TRANSACTION HISTORY - Using GridRequest
    // ============================================================

    public async Task<PaginatedResult<EmployeeTransactionHistoryDto>> GetTransactionHistoryAsync(EmployeeSummaryGridRequest request)
    {
        var conditions = new List<string>
        {
            "at.EmployeeId = @EmployeeId",
            "at.IsActive = 1"
        };
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeId", request.EmployeeId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR mdType.MasterDataName LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "at.TransactionDate", true);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*)
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId
            LEFT JOIN MasterData mdType ON at.TransactionType = mdType.ReferenceCode AND mdType.ReferenceName = 'TransactionType' AND mdType.IsActive = 1
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                at.AssetTransactionId,
                at.AssetId,
                a.AssetCode,
                a.AssetName,
                mdType.MasterDataName AS TransactionTypeName,
                at.TransactionDate,
                e.FullName AS EmployeeName,
                mdCondition.MasterDataName AS ConditionAfterName,
                at.Notes";

        var fromClause = @"
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId
            LEFT JOIN Employee e ON at.EmployeeId = e.EmployeeId
            LEFT JOIN MasterData mdType ON at.TransactionType = mdType.ReferenceCode AND mdType.ReferenceName = 'TransactionType' AND mdType.IsActive = 1
            LEFT JOIN MasterData mdCondition ON at.Condition = mdCondition.ReferenceCode AND mdCondition.ReferenceName = 'AssetCondition' AND mdCondition.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<EmployeeTransactionHistoryDto>(dataSql, parameters);

        return new PaginatedResult<EmployeeTransactionHistoryDto>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // CURRENT HOLDING STATS (Category breakdown)
    // ============================================================

    public async Task<EmployeeCategoryStatsResponse> GetCurrentHoldingStatsAsync(int employeeId)
    {
        var sql = @"
            SELECT 
                c.CategoryName AS Category,
                COUNT(DISTINCT a.AssetId) AS Count
            FROM AssetTransaction at
            LEFT JOIN Asset a ON at.AssetId = a.AssetId AND a.IsActive = 1
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            WHERE at.EmployeeId = @EmployeeId
              AND at.Approved = 1
              AND at.FromAssetTransactionId IS NULL
              AND at.IsActive = 1
              AND at.TransactionType IN (@HandoverType, @TransferType, @LoanType)
            GROUP BY c.CategoryName";

        var parameters = new
        {
            EmployeeId = employeeId,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            LoanType = TransactionTypeConstants.LOAN
        };

        var data = await _context.QueryAsync<EmployeeCategoryStatsDto>(sql, parameters);

        var totalCountSql = @"
            SELECT COUNT(DISTINCT AssetId)
            FROM AssetTransaction 
            WHERE EmployeeId = @EmployeeId
              AND Approved = 1
              AND FromAssetTransactionId IS NULL
              AND IsActive = 1
              AND TransactionType IN (@HandoverType, @TransferType, @LoanType)";

        var totalAssets = await _context.ExecuteScalarAsync<int>(totalCountSql, parameters);

        var result = data.ToList();
        foreach (var item in result)
        {
            item.Percentage = totalAssets > 0 ? Math.Round((decimal)item.Count / totalAssets * 100, 1) : 0;
        }

        return new EmployeeCategoryStatsResponse
        {
            TotalAssets = totalAssets,
            Data = result
        };
    }

    // ============================================================
    // TRANSACTION STATS
    // ============================================================

    public async Task<EmployeeTransactionStatsResponse> GetTransactionStatsAsync(int employeeId)
    {
        var sql = @"
            SELECT 
                md.MasterDataName AS TransactionType,
                COUNT(*) AS Count
            FROM AssetTransaction at
            LEFT JOIN MasterData md ON at.TransactionType = md.ReferenceCode AND md.ReferenceName = 'TransactionType' AND md.IsActive = 1
            WHERE at.EmployeeId = @EmployeeId
              AND at.IsActive = 1
            GROUP BY md.MasterDataName
            ORDER BY Count DESC";

        var data = await _context.QueryAsync<EmployeeTransactionStatsDto>(sql, new { EmployeeId = employeeId });

        var totalCountSql = "SELECT COUNT(*) FROM AssetTransaction WHERE EmployeeId = @EmployeeId AND IsActive = 1";
        var totalTransactions = await _context.ExecuteScalarAsync<int>(totalCountSql, new { EmployeeId = employeeId });

        var result = data.ToList();
        foreach (var item in result)
        {
            item.Percentage = totalTransactions > 0 ? Math.Round((decimal)item.Count / totalTransactions * 100, 1) : 0;
        }

        return new EmployeeTransactionStatsResponse
        {
            TotalTransactions = totalTransactions,
            Data = result
        };
    }

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    public async Task<EmployeeScheduleStatsDto> GetScheduleStatsAsync(int employeeId)
    {
        var result = new EmployeeScheduleStatsDto();

        var handoverSql = @"
            SELECT 
                sh.ScheduleHandoverId AS ScheduleId,
                sh.ScheduleDate,
                a.AssetCode,
                a.AssetName,
                CASE WHEN sh.HandoverAssetTransactionId IS NOT NULL THEN 'Completed' ELSE 'Pending' END AS Status
            FROM ScheduleHandover sh
            LEFT JOIN Asset a ON sh.AssetId = a.AssetId
            WHERE sh.EmployeeId = @EmployeeId AND sh.IsActive = 1
            ORDER BY sh.ScheduleDate DESC";

        result.HandoverList = (await _context.QueryAsync<EmployeeScheduleItemDto>(handoverSql, new { EmployeeId = employeeId })).ToList();

        var returnSql = @"
            SELECT 
                sr.ScheduleReturnId AS ScheduleId,
                sr.ScheduleDate,
                a.AssetCode,
                a.AssetName,
                CASE WHEN sr.ReturnAssetTransactionId IS NOT NULL THEN 'Completed' ELSE 'Pending' END AS Status
            FROM ScheduleReturn sr
            LEFT JOIN Asset a ON sr.AssetId = a.AssetId
            WHERE sr.EmployeeId = @EmployeeId AND sr.IsActive = 1
            ORDER BY sr.ScheduleDate DESC";

        result.ReturnList = (await _context.QueryAsync<EmployeeScheduleItemDto>(returnSql, new { EmployeeId = employeeId })).ToList();

        result.TotalHandover = result.HandoverList.Count;
        result.TotalReturn = result.ReturnList.Count;
        result.UpcomingHandover = result.HandoverList.Count(x => x.ScheduleDate >= DateTime.Today && x.Status == "Pending");
        result.UpcomingReturn = result.ReturnList.Count(x => x.ScheduleDate >= DateTime.Today && x.Status == "Pending");

        return result;
    }

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    public async Task<IEnumerable<EmployeeSummaryAttachmentDto>> GetAttachmentsByEmployeeIdAsync(int employeeId)
    {
        const string sql = @"
            SELECT 
                fa.FileAttachmentId, fa.FileName, fa.OriginalFileName, fa.FileCategory, fa.IsPrimary,
                '/api/FileAttachment/download/' + CAST(fa.FileAttachmentId AS VARCHAR) as FileUrl
            FROM EmployeeAttachment ea
            INNER JOIN FileAttachment fa ON ea.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            WHERE ea.EmployeeId = @EmployeeId AND ea.IsActive = 1
            ORDER BY ea.Order ASC, fa.CreatedDate DESC";

        return await _context.QueryAsync<EmployeeSummaryAttachmentDto>(sql, new { EmployeeId = employeeId });
    }

    public async Task<EmployeeSummaryAttachmentDto?> GetPhotoProfileByEmployeeIdAsync(int employeeId)
    {
        const string sql = @"
            SELECT TOP 1
                fa.FileAttachmentId, fa.FileName, fa.OriginalFileName, fa.FileCategory, fa.IsPrimary,
                '/api/FileAttachment/download/' + CAST(fa.FileAttachmentId AS VARCHAR) as FileUrl
            FROM EmployeePhotoProfileAttachment eppa
            INNER JOIN FileAttachment fa ON eppa.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            WHERE eppa.EmployeeId = @EmployeeId AND eppa.IsActive = 1
            ORDER BY fa.IsPrimary DESC, fa.CreatedDate DESC";

        return await _context.QueryFirstOrDefaultAsync<EmployeeSummaryAttachmentDto>(sql, new { EmployeeId = employeeId });
    }
}