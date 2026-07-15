using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTransaction;

/// <summary>
/// Repository implementation for Asset Transaction operations using Dapper
/// </summary>
public class AssetTransactionReps : IAssetTransactionReps
{
    private readonly DapperContext _context;

    public AssetTransactionReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // ENTITY
    // ============================================================

    public async Task<AssetTransactionEntity?> GetByIdRawAsync(int id)
    {
        const string sql = "SELECT * FROM AssetTransaction WHERE AssetTransactionId = @Id";
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { Id = id });
    }

    public async Task<int> GetTransactionCountByAssetAsync(int assetId)
    {
        const string sql = "SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = @AssetId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { AssetId = assetId });
    }

    public async Task<bool> HasOpenPairedTransactionAsync(int assetId, int transactionType)
    {
        const string sql = @"
            SELECT COUNT(1) FROM AssetTransaction
            WHERE AssetId = @AssetId
              AND TransactionType = @TransactionType
              AND Approved = 1
              AND FromAssetTransactionId IS NULL
              AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { AssetId = assetId, TransactionType = transactionType }) > 0;
    }

    // ============================================================
    // DETAIL VIEW
    // ============================================================

    public async Task<AssetTransactionDetailView?> GetDetailByIdAsync(int id)
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId, t.AssetTransactionCode, t.TransactionDate,
                t.ExpectedLoanReturnDate, t.Notes, t.Condition, t.Approved, t.ApprovedBy,
                t.Completed, t.CompletedBy, t.MaintenanceType, t.MaintenanceCost,
                t.FromAssetTransactionId, t.IsActive, t.CreatedDate, t.CreatedBy,
                t.ModifiedDate, t.ModifiedBy,
                a.AssetCode, a.AssetName,
                e.EmployeeId, e.FullName as EmployeeName, e.EmployeeCode,
                o.OfficeId, o.OfficeName,
                md1.MasterDataName as TransactionTypeName,
                md2.MasterDataName as ConditionName,
                md3.MasterDataName as MaintenanceTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN Office o ON t.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.Condition = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            LEFT JOIN MasterData md3 ON t.MaintenanceType = md3.ReferenceCode AND md3.ReferenceName = 'MaintenanceType' AND md3.IsActive = 1
            WHERE t.AssetTransactionId = @Id AND t.IsActive = 1";

        return await _context.QueryFirstOrDefaultAsync<AssetTransactionDetailView>(sql, new { Id = id });
    }

    // ============================================================
    // GRID - ADD
    // ============================================================

    public async Task<PaginatedResult<AssetTransactionListView>> GetGridAddAsync(AssetTransactionGridRequest request)
    {
        var addTypes = new[] {
            TransactionTypeConstants.HANDOVER,
            TransactionTypeConstants.TRANSFER,
            TransactionTypeConstants.LOAN,
            TransactionTypeConstants.MAINTENANCE,
            TransactionTypeConstants.DISPOSAL,
            TransactionTypeConstants.OPERATIONAL_OFFICE
        };

        return await GetGridBaseAsync(request, addTypes, false);
    }

    // ============================================================
    // GRID - RETURN
    // ============================================================

    public async Task<PaginatedResult<AssetTransactionListView>> GetGridReturnAsync(AssetTransactionGridRequest request)
    {
        var returnTypes = new[] {
            TransactionTypeConstants.RETURN,
            TransactionTypeConstants.LOAN_RETURN,
            TransactionTypeConstants.POST_MAINTENANCE
        };

        return await GetGridBaseAsync(request, returnTypes, true);
    }

    // ============================================================
    // GRID - BASE
    // ============================================================

    private async Task<PaginatedResult<AssetTransactionListView>> GetGridBaseAsync(
        AssetTransactionGridRequest request, int[] transactionTypes, bool isReturnGrid)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("t.TransactionType IN @TransactionTypes");
        parameters.Add("@TransactionTypes", transactionTypes);
        conditions.Add("t.IsActive = 1");

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR e.FullName LIKE @Search OR e.EmployeeCode LIKE @Search OR t.AssetTransactionCode LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        if (request.DateFrom.HasValue)
        {
            conditions.Add("t.TransactionDate >= @DateFrom");
            parameters.Add("@DateFrom", request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            conditions.Add("t.TransactionDate <= @DateTo");
            parameters.Add("@DateTo", request.DateTo.Value);
        }

        if (request.EmployeeId.HasValue)
        {
            conditions.Add("t.EmployeeId = @EmployeeId");
            parameters.Add("@EmployeeId", request.EmployeeId.Value);
        }

        if (request.AssetId.HasValue)
        {
            conditions.Add("t.AssetId = @AssetId");
            parameters.Add("@AssetId", request.AssetId.Value);
        }

        if (request.TransactionType.HasValue)
        {
            conditions.Add("t.TransactionType = @TransactionType");
            parameters.Add("@TransactionType", request.TransactionType.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sortBy = string.IsNullOrEmpty(request.SortBy) ? "t.TransactionDate" : request.SortBy;
        if (!sortBy.StartsWith("t.") && !sortBy.StartsWith("a.") && !sortBy.StartsWith("e."))
        {
            sortBy = $"t.{sortBy}";
        }

        var orderBy = $"ORDER BY {sortBy} {(request.SortDescending ? "DESC" : "ASC")}";

        // Count
        var countSql = $@"
            SELECT COUNT(*)
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            {whereClause}";

        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        // Data
        var offset = (request.Page - 1) * request.PageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", request.PageSize);

        var selectClause = @"
            SELECT 
                t.AssetTransactionId, t.AssetTransactionCode, t.AssetId, t.TransactionType, 
                t.TransactionDate, t.ExpectedLoanReturnDate, t.Notes, t.Condition,
                t.Approved, t.ApprovedBy, t.Completed, t.CompletedBy,
                t.MaintenanceType, t.MaintenanceCost, t.FromAssetTransactionId, t.IsActive,
                t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy,
                a.AssetCode, a.AssetName,
                e.EmployeeId, e.EmployeeCode, e.FullName as EmployeeName,
                md1.MasterDataName as TransactionTypeName,
                md2.MasterDataName as ConditionName,
                md3.MasterDataName as MaintenanceTypeName,
                0 as CanEdit, 0 as CanDelete, 0 as CanReturn, 0 as CanPost";

        var fromClause = @"
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.Condition = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            LEFT JOIN MasterData md3 ON t.MaintenanceType = md3.ReferenceCode AND md3.ReferenceName = 'MaintenanceType' AND md3.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await _context.QueryAsync<AssetTransactionListView>(dataSql, parameters);

        // Calculate CanEdit, CanDelete, CanReturn, CanPost
        var result = data.ToList();
        foreach (var item in result)
        {
            var flags = await CalculateActionFlagsAsync(item, isReturnGrid);
            item.CanEdit = flags.CanEdit;
            item.CanDelete = flags.CanDelete;
            item.CanReturn = flags.CanReturn;
            item.CanPost = flags.CanPost;
        }

        return new PaginatedResult<AssetTransactionListView>
        {
            Data = result,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // ACTION FLAGS
    // ============================================================

    private async Task<(bool CanEdit, bool CanDelete, bool CanReturn, bool CanPost)> CalculateActionFlagsAsync(
        AssetTransactionListView item, bool isReturnGrid)
    {
        bool canEdit = false, canDelete = false, canReturn = false, canPost = false;

        if (isReturnGrid)
        {
            // RETURN grid: hanya bisa edit/delete jika pending
            if (item.Approved == null)
            {
                canEdit = true;
                canDelete = true;
            }
        }
        else
        {
            // ADD grid
            if (item.Approved == null && item.Completed == null)
            {
                canEdit = true;
                canDelete = true;
            }

            // Cek apakah sudah ada transaksi pairing
            if (item.FromAssetTransactionId == null && item.Approved == true)
            {
                // CEK APAKAH SUDAH ADA PAIRING
                var hasReturn = await HasPairedTransactionAsync(item.AssetTransactionId, item.TransactionType);
                var hasPost = await HasPostMaintenanceAsync(item.AssetTransactionId);

                if (item.TransactionType == TransactionTypeConstants.MAINTENANCE)
                {
                    canPost = !hasPost;
                }
                else
                {
                    canReturn = !hasReturn;
                }
            }
        }

        return (canEdit, canDelete, canReturn, canPost);
    }

    private async Task<bool> HasPairedTransactionAsync(int transactionId, int transactionType)
    {
        if (transactionType == TransactionTypeConstants.LOAN)
        {
            const string sql = "SELECT COUNT(1) FROM AssetTransaction WHERE FromAssetTransactionId = @Id AND TransactionType = @ReturnType AND IsActive = 1";
            return await _context.ExecuteScalarAsync<int>(sql, new { Id = transactionId, ReturnType = TransactionTypeConstants.LOAN_RETURN }) > 0;
        }
        else
        {
            const string sql = "SELECT COUNT(1) FROM AssetTransaction WHERE FromAssetTransactionId = @Id AND TransactionType = @ReturnType AND IsActive = 1";
            return await _context.ExecuteScalarAsync<int>(sql, new { Id = transactionId, ReturnType = TransactionTypeConstants.RETURN }) > 0;
        }
    }

    private async Task<bool> HasPostMaintenanceAsync(int transactionId)
    {
        const string sql = "SELECT COUNT(1) FROM AssetTransaction WHERE FromAssetTransactionId = @Id AND TransactionType = @PostType AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { Id = transactionId, PostType = TransactionTypeConstants.POST_MAINTENANCE }) > 0;
    }

    // ============================================================
    // GRID - PENDING APPROVAL
    // ============================================================

    public async Task<PaginatedResult<AssetTransactionPendingApprovalView>> GetGridPendingApprovalAsync(AssetTransactionGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("t.Approved = 1");
        conditions.Add("t.Completed IS NULL");
        conditions.Add("t.IsActive = 1");

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR e.FullName LIKE @Search OR e.EmployeeCode LIKE @Search OR t.AssetTransactionCode LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        if (request.DateFrom.HasValue)
        {
            conditions.Add("t.TransactionDate >= @DateFrom");
            parameters.Add("@DateFrom", request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            conditions.Add("t.TransactionDate <= @DateTo");
            parameters.Add("@DateTo", request.DateTo.Value);
        }

        if (request.EmployeeId.HasValue)
        {
            conditions.Add("t.EmployeeId = @EmployeeId");
            parameters.Add("@EmployeeId", request.EmployeeId.Value);
        }

        if (request.AssetId.HasValue)
        {
            conditions.Add("t.AssetId = @AssetId");
            parameters.Add("@AssetId", request.AssetId.Value);
        }

        if (request.TransactionType.HasValue)
        {
            conditions.Add("t.TransactionType = @TransactionType");
            parameters.Add("@TransactionType", request.TransactionType.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sortBy = string.IsNullOrEmpty(request.SortBy) ? "t.TransactionDate" : request.SortBy;
        if (!sortBy.StartsWith("t.") && !sortBy.StartsWith("a.") && !sortBy.StartsWith("e."))
        {
            sortBy = $"t.{sortBy}";
        }

        var orderBy = $"ORDER BY {sortBy} {(request.SortDescending ? "DESC" : "ASC")}";

        // Count
        var countSql = $@"
            SELECT COUNT(*)
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            {whereClause}";

        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        // Data
        var offset = (request.Page - 1) * request.PageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", request.PageSize);

        var dataSql = $@"
            SELECT 
                t.AssetTransactionId, t.AssetTransactionCode, t.TransactionDate,
                t.Approved, t.Completed,
                a.AssetCode, a.AssetName,
                e.EmployeeCode, e.FullName as EmployeeName,
                md1.MasterDataName as TransactionTypeName,
                md2.MasterDataName as ConditionName,
                0 as CanEdit, 0 as CanDelete, 0 as CanReturn, 0 as CanPost,
                0 as AllAttachmentsApproved, 0 as HasNeedToChange
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.Condition = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await _context.QueryAsync<AssetTransactionPendingApprovalView>(dataSql, parameters);

        var result = data.ToList();
        foreach (var item in result)
        {
            // Get attachments status
            var attachments = await GetApproveAttachmentsByTransactionIdAsync(item.AssetTransactionId);
            item.Attachments = attachments.Select(a => new ApproveAttachmentStatusDto
            {
                AttachmentId = a.AssetTransactionApproveAttachmentId,
                DocumentType = a.DocumentType,
                DocumentTypeName = a.DocumentTypeName,
                IsRequired = a.IsRequired,
                IsUploaded = true,
                NeedToChange = a.NeedToChange,
                Approved = a.Approved,
                FileName = a.OriginalFileName
            }).ToList();

            // Tambahkan required document yang belum diupload
            var requiredDocs = await GetRequiredDocumentTypesAsync(item.AssetTransactionId);
            foreach (var doc in requiredDocs.Where(d => !d.IsUploaded))
            {
                item.Attachments.Add(new ApproveAttachmentStatusDto
                {
                    DocumentType = doc.DocumentType,
                    DocumentTypeName = doc.DocumentTypeName,
                    IsRequired = doc.IsRequired,
                    IsUploaded = false,
                    NeedToChange = false,
                    Approved = false
                });
            }

            item.AllAttachmentsApproved = item.Attachments.All(a => !a.IsRequired || a.Approved);
            item.HasNeedToChange = item.Attachments.Any(a => a.NeedToChange);

            // Action flags
            if (item.AllAttachmentsApproved && !item.HasNeedToChange)
            {
                item.CanEdit = false;
                item.CanDelete = false;
                item.CanReturn = false;
                item.CanPost = false;
            }
        }

        return new PaginatedResult<AssetTransactionPendingApprovalView>
        {
            Data = result,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // OUTSTANDING - ACTIVE LOANS
    // ============================================================

    public async Task<PaginatedResult<AssetTransactionListView>> GetActiveLoansAsync(AssetTransactionGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("t.TransactionType = @LoanType");
        conditions.Add("t.Approved = 1");
        conditions.Add("t.Completed IS NULL");
        conditions.Add("t.FromAssetTransactionId IS NULL");
        conditions.Add("t.IsActive = 1");
        parameters.Add("@LoanType", TransactionTypeConstants.LOAN);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR e.FullName LIKE @Search OR e.EmployeeCode LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        if (request.DateFrom.HasValue)
        {
            conditions.Add("t.TransactionDate >= @DateFrom");
            parameters.Add("@DateFrom", request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            conditions.Add("t.TransactionDate <= @DateTo");
            parameters.Add("@DateTo", request.DateTo.Value);
        }

        if (request.EmployeeId.HasValue)
        {
            conditions.Add("t.EmployeeId = @EmployeeId");
            parameters.Add("@EmployeeId", request.EmployeeId.Value);
        }

        if (request.AssetId.HasValue)
        {
            conditions.Add("t.AssetId = @AssetId");
            parameters.Add("@AssetId", request.AssetId.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sortBy = string.IsNullOrEmpty(request.SortBy) ? "t.TransactionDate" : request.SortBy;
        if (!sortBy.StartsWith("t.") && !sortBy.StartsWith("a.") && !sortBy.StartsWith("e."))
        {
            sortBy = $"t.{sortBy}";
        }

        var orderBy = $"ORDER BY {sortBy} {(request.SortDescending ? "DESC" : "ASC")}";

        // Count
        var countSql = $@"
            SELECT COUNT(*)
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            {whereClause}";

        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        // Data
        var offset = (request.Page - 1) * request.PageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", request.PageSize);

        var dataSql = $@"
            SELECT 
                t.AssetTransactionId, t.AssetTransactionCode, t.AssetId, t.TransactionType,
                t.TransactionDate, t.ExpectedLoanReturnDate, t.Notes, t.Condition,
                t.Approved, t.ApprovedBy, t.Completed, t.CompletedBy,
                t.MaintenanceType, t.MaintenanceCost, t.FromAssetTransactionId, t.IsActive,
                t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy,
                a.AssetCode, a.AssetName,
                e.EmployeeId, e.EmployeeCode, e.FullName as EmployeeName,
                md1.MasterDataName as TransactionTypeName,
                md2.MasterDataName as ConditionName,
                CASE WHEN t.ExpectedLoanReturnDate < GETDATE() THEN 1 ELSE 0 END as IsOverdue,
                0 as CanEdit, 0 as CanDelete, 0 as CanReturn, 0 as CanPost
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.Condition = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await _context.QueryAsync<AssetTransactionListView>(dataSql, parameters);

        return new PaginatedResult<AssetTransactionListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // OUTSTANDING - OVERDUE LOANS
    // ============================================================

    public async Task<PaginatedResult<AssetTransactionListView>> GetOverdueLoansAsync(AssetTransactionGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("t.TransactionType = @LoanType");
        conditions.Add("t.Approved = 1");
        conditions.Add("t.Completed IS NULL");
        conditions.Add("t.FromAssetTransactionId IS NULL");
        conditions.Add("t.IsActive = 1");
        conditions.Add("t.ExpectedLoanReturnDate < GETDATE()");
        parameters.Add("@LoanType", TransactionTypeConstants.LOAN);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR e.FullName LIKE @Search OR e.EmployeeCode LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        if (request.DateFrom.HasValue)
        {
            conditions.Add("t.TransactionDate >= @DateFrom");
            parameters.Add("@DateFrom", request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            conditions.Add("t.TransactionDate <= @DateTo");
            parameters.Add("@DateTo", request.DateTo.Value);
        }

        if (request.EmployeeId.HasValue)
        {
            conditions.Add("t.EmployeeId = @EmployeeId");
            parameters.Add("@EmployeeId", request.EmployeeId.Value);
        }

        if (request.AssetId.HasValue)
        {
            conditions.Add("t.AssetId = @AssetId");
            parameters.Add("@AssetId", request.AssetId.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sortBy = string.IsNullOrEmpty(request.SortBy) ? "t.ExpectedLoanReturnDate" : request.SortBy;
        if (!sortBy.StartsWith("t.") && !sortBy.StartsWith("a.") && !sortBy.StartsWith("e."))
        {
            sortBy = $"t.{sortBy}";
        }

        var orderBy = $"ORDER BY {sortBy} {(request.SortDescending ? "DESC" : "ASC")}";

        // Count
        var countSql = $@"
            SELECT COUNT(*)
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            {whereClause}";

        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        // Data
        var offset = (request.Page - 1) * request.PageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", request.PageSize);

        var dataSql = $@"
            SELECT 
                t.AssetTransactionId, t.AssetTransactionCode, t.AssetId, t.TransactionType,
                t.TransactionDate, t.ExpectedLoanReturnDate, t.Notes, t.Condition,
                t.Approved, t.ApprovedBy, t.Completed, t.CompletedBy,
                t.MaintenanceType, t.MaintenanceCost, t.FromAssetTransactionId, t.IsActive,
                t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy,
                a.AssetCode, a.AssetName,
                e.EmployeeId, e.EmployeeCode, e.FullName as EmployeeName,
                md1.MasterDataName as TransactionTypeName,
                md2.MasterDataName as ConditionName,
                1 as IsOverdue,
                0 as CanEdit, 0 as CanDelete, 0 as CanReturn, 0 as CanPost
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.Condition = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await _context.QueryAsync<AssetTransactionListView>(dataSql, parameters);

        return new PaginatedResult<AssetTransactionListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // OUTSTANDING - UNDER MAINTENANCE
    // ============================================================

    public async Task<PaginatedResult<AssetTransactionListView>> GetUnderMaintenanceAsync(AssetTransactionGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        conditions.Add("t.TransactionType = @MaintenanceType");
        conditions.Add("t.Approved = 1");
        conditions.Add("t.Completed IS NULL");
        conditions.Add("t.FromAssetTransactionId IS NULL");
        conditions.Add("t.IsActive = 1");
        parameters.Add("@MaintenanceType", TransactionTypeConstants.MAINTENANCE);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search)");
            parameters.Add("@Search", $"%{request.Search}%");
        }

        if (request.DateFrom.HasValue)
        {
            conditions.Add("t.TransactionDate >= @DateFrom");
            parameters.Add("@DateFrom", request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            conditions.Add("t.TransactionDate <= @DateTo");
            parameters.Add("@DateTo", request.DateTo.Value);
        }

        if (request.AssetId.HasValue)
        {
            conditions.Add("t.AssetId = @AssetId");
            parameters.Add("@AssetId", request.AssetId.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var sortBy = string.IsNullOrEmpty(request.SortBy) ? "t.TransactionDate" : request.SortBy;
        if (!sortBy.StartsWith("t.") && !sortBy.StartsWith("a.") && !sortBy.StartsWith("e."))
        {
            sortBy = $"t.{sortBy}";
        }

        var orderBy = $"ORDER BY {sortBy} {(request.SortDescending ? "DESC" : "ASC")}";

        // Count
        var countSql = $@"
            SELECT COUNT(*)
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            {whereClause}";

        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        // Data
        var offset = (request.Page - 1) * request.PageSize;
        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", request.PageSize);

        var dataSql = $@"
            SELECT 
                t.AssetTransactionId, t.AssetTransactionCode, t.AssetId, t.TransactionType,
                t.TransactionDate, t.ExpectedLoanReturnDate, t.Notes, t.Condition,
                t.Approved, t.ApprovedBy, t.Completed, t.CompletedBy,
                t.MaintenanceType, t.MaintenanceCost, t.FromAssetTransactionId, t.IsActive,
                t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy,
                a.AssetCode, a.AssetName,
                md1.MasterDataName as TransactionTypeName,
                md2.MasterDataName as ConditionName,
                md3.MasterDataName as MaintenanceTypeName,
                0 as CanEdit, 0 as CanDelete, 0 as CanReturn, 0 as CanPost
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.Condition = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            LEFT JOIN MasterData md3 ON t.MaintenanceType = md3.ReferenceCode AND md3.ReferenceName = 'MaintenanceType' AND md3.IsActive = 1
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var data = await _context.QueryAsync<AssetTransactionListView>(dataSql, parameters);

        return new PaginatedResult<AssetTransactionListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // SHORTCUT / PAIRING
    // ============================================================

    public async Task<AssetTransactionListView?> GetActiveTransactionByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT TOP 1
                t.AssetTransactionId, t.AssetTransactionCode, t.AssetId, t.TransactionType,
                t.TransactionDate, t.ExpectedLoanReturnDate, t.Notes, t.Condition,
                t.Approved, t.ApprovedBy, t.Completed, t.CompletedBy,
                t.MaintenanceType, t.MaintenanceCost, t.FromAssetTransactionId, t.IsActive,
                t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy,
                a.AssetCode, a.AssetName,
                e.EmployeeId, e.EmployeeCode, e.FullName as EmployeeName,
                md1.MasterDataName as TransactionTypeName,
                md2.MasterDataName as ConditionName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee e ON t.EmployeeId = e.EmployeeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.Condition = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            WHERE t.AssetId = @AssetId
              AND t.Approved = 1
              AND t.FromAssetTransactionId IS NULL
              AND t.TransactionType IN (@HandoverType, @TransferType, @LoanType, @MaintenanceType, @OperationalOffice)
              AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";

        var parameters = new
        {
            AssetId = assetId,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            LoanType = TransactionTypeConstants.LOAN,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE,
            OperationalOffice = TransactionTypeConstants.OPERATIONAL_OFFICE
        };

        return await _context.QueryFirstOrDefaultAsync<AssetTransactionListView>(sql, parameters);
    }

    public async Task<AssetTransactionEntity?> GetPairedTransactionAsync(int transactionId)
    {
        const string sql = @"
            SELECT t.*
            FROM AssetTransaction t
            WHERE t.FromAssetTransactionId = @TransactionId AND t.IsActive = 1";

        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { TransactionId = transactionId });
    }

    // ============================================================
    // CRUD
    // ============================================================

    public async Task<int> InsertAsync(AssetTransactionEntity entity)
    {
        const string sql = @"
            INSERT INTO AssetTransaction (
                Notes, IsActive, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy,
                Approved, ApprovedBy, Completed, CompletedBy, AssetTransactionCode,
                TransactionDate, TransactionType, AssetId, EmployeeId, OfficeId,
                Condition, FromAssetTransactionId, ExpectedLoanReturnDate,
                MaintenanceEmployeeId, MaintenanceType, MaintenanceCost
            ) VALUES (
                @Notes, 1, GETDATE(), @CreatedBy, NULL, NULL,
                NULL, NULL, NULL, NULL, @AssetTransactionCode,
                @TransactionDate, @TransactionType, @AssetId, @EmployeeId, @OfficeId,
                @Condition, @FromAssetTransactionId, @ExpectedLoanReturnDate,
                @MaintenanceEmployeeId, @MaintenanceType, @MaintenanceCost
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<int> UpdateAsync(AssetTransactionEntity entity)
    {
        const string sql = @"
            UPDATE AssetTransaction SET
                Notes = @Notes,
                ModifiedDate = GETDATE(),
                ModifiedBy = @ModifiedBy,
                Approved = @Approved,
                ApprovedBy = @ApprovedBy,
                Completed = @Completed,
                CompletedBy = @CompletedBy,
                AssetTransactionCode = @AssetTransactionCode,
                TransactionDate = @TransactionDate,
                TransactionType = @TransactionType,
                AssetId = @AssetId,
                EmployeeId = @EmployeeId,
                OfficeId = @OfficeId,
                Condition = @Condition,
                FromAssetTransactionId = @FromAssetTransactionId,
                ExpectedLoanReturnDate = @ExpectedLoanReturnDate,
                MaintenanceEmployeeId = @MaintenanceEmployeeId,
                MaintenanceType = @MaintenanceType,
                MaintenanceCost = @MaintenanceCost
            WHERE AssetTransactionId = @AssetTransactionId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM AssetTransaction WHERE AssetTransactionId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<int> SoftDeleteAsync(int id)
    {
        const string sql = "UPDATE AssetTransaction SET IsActive = 0, ModifiedDate = GETDATE() WHERE AssetTransactionId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = id });
    }

    // ============================================================
    // ATTACHMENT (UMUM)
    // ============================================================

    public async Task<int> InsertAttachmentAsync(AssetTransactionAttachmentEntity entity)
    {
        const string sql = @"
            INSERT INTO AssetTransactionAttachment (
                AssetTransactionId, FileAttachmentId, [Order], IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @AssetTransactionId, @FileAttachmentId, @Order, 1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<IEnumerable<AssetTransactionAttachmentView>> GetAttachmentsByTransactionIdAsync(int transactionId)
    {
        const string sql = @"
            SELECT 
                ata.AssetTransactionAttachmentId, ata.AssetTransactionId, ata.FileAttachmentId,
                ata.[Order], ata.CreatedDate, ata.CreatedBy,
                fa.FileName, fa.OriginalFileName, fa.FileCategory, fa.IsPrimary,
                '/api/FileAttachment/download/' + CAST(fa.FileAttachmentId AS VARCHAR) as FileUrl
            FROM AssetTransactionAttachment ata
            INNER JOIN FileAttachment fa ON ata.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            WHERE ata.AssetTransactionId = @TransactionId AND ata.IsActive = 1
            ORDER BY ata.[Order] ASC, fa.CreatedDate DESC";

        return await _context.QueryAsync<AssetTransactionAttachmentView>(sql, new { TransactionId = transactionId });
    }

    public async Task<AssetTransactionAttachmentEntity?> GetAttachmentByIdAsync(int attachmentId)
    {
        const string sql = "SELECT * FROM AssetTransactionAttachment WHERE AssetTransactionAttachmentId = @Id AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionAttachmentEntity>(sql, new { Id = attachmentId });
    }

    public async Task<int> DeleteAttachmentAsync(int attachmentId)
    {
        const string sql = "UPDATE AssetTransactionAttachment SET IsActive = 0, ModifiedDate = GETDATE() WHERE AssetTransactionAttachmentId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = attachmentId });
    }

    public async Task<int> DeleteAttachmentsByTransactionIdAsync(int transactionId)
    {
        const string sql = "UPDATE AssetTransactionAttachment SET IsActive = 0, ModifiedDate = GETDATE() WHERE AssetTransactionId = @TransactionId";
        return await _context.ExecuteAsync(sql, new { TransactionId = transactionId });
    }

    // ============================================================
    // APPROVE ATTACHMENT
    // ============================================================

    public async Task<int> InsertApproveAttachmentAsync(AssetTransactionApproveAttachmentEntity entity)
    {
        const string sql = @"
            INSERT INTO AssetTransactionApproveAttachment (
                AssetTransactionId, FileAttachmentId, [Order], DocumentType,
                NeedToChange, Approved, IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @AssetTransactionId, @FileAttachmentId, @Order, @DocumentType,
                0, 0, 1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<IEnumerable<AssetTransactionApproveAttachmentView>> GetApproveAttachmentsByTransactionIdAsync(int transactionId)
    {
        const string sql = @"
            SELECT 
                ataa.AssetTransactionApproveAttachmentId, ataa.AssetTransactionId, ataa.FileAttachmentId,
                ataa.[Order], ataa.DocumentType, ataa.NeedToChange, ataa.Approved,
                ataa.CreatedDate, ataa.CreatedBy,
                fa.FileName, fa.OriginalFileName,
                '/api/FileAttachment/download/' + CAST(fa.FileAttachmentId AS VARCHAR) as FileUrl,
                md.MasterDataName as DocumentTypeName,
                CASE WHEN md.ReferenceCode IS NOT NULL THEN 1 ELSE 0 END as IsRequired
            FROM AssetTransactionApproveAttachment ataa
            INNER JOIN FileAttachment fa ON ataa.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            LEFT JOIN MasterData md ON ataa.DocumentType = md.ReferenceCode AND md.ReferenceName = 'ApproveDocumentType' AND md.IsActive = 1
            WHERE ataa.AssetTransactionId = @TransactionId AND ataa.IsActive = 1
            ORDER BY ataa.[Order] ASC, fa.CreatedDate DESC";

        return await _context.QueryAsync<AssetTransactionApproveAttachmentView>(sql, new { TransactionId = transactionId });
    }

    public async Task<AssetTransactionApproveAttachmentEntity?> GetApproveAttachmentByIdAsync(int attachmentId)
    {
        const string sql = "SELECT * FROM AssetTransactionApproveAttachment WHERE AssetTransactionApproveAttachmentId = @Id AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionApproveAttachmentEntity>(sql, new { Id = attachmentId });
    }

    public async Task<int> UpdateApproveAttachmentAsync(AssetTransactionApproveAttachmentEntity entity)
    {
        const string sql = @"
            UPDATE AssetTransactionApproveAttachment SET
                NeedToChange = @NeedToChange,
                Approved = @Approved,
                ModifiedDate = GETDATE(),
                ModifiedBy = @ModifiedBy
            WHERE AssetTransactionApproveAttachmentId = @AssetTransactionApproveAttachmentId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, entity);
    }

    public async Task<int> DeleteApproveAttachmentAsync(int attachmentId)
    {
        const string sql = "UPDATE AssetTransactionApproveAttachment SET IsActive = 0, ModifiedDate = GETDATE() WHERE AssetTransactionApproveAttachmentId = @Id";
        return await _context.ExecuteAsync(sql, new { Id = attachmentId });
    }

    public async Task<int> DeleteApproveAttachmentsByTransactionIdAsync(int transactionId)
    {
        const string sql = "UPDATE AssetTransactionApproveAttachment SET IsActive = 0, ModifiedDate = GETDATE() WHERE AssetTransactionId = @TransactionId";
        return await _context.ExecuteAsync(sql, new { TransactionId = transactionId });
    }

    public async Task<IEnumerable<RequiredDocumentTypeDto>> GetRequiredDocumentTypesAsync(int transactionId)
    {
        const string sql = @"
            SELECT 
                md.ReferenceCode as DocumentType,
                md.MasterDataName as DocumentTypeName,
                CASE WHEN md.ReferenceCode IS NOT NULL THEN 1 ELSE 0 END as IsRequired,
                CASE WHEN ataa.AssetTransactionApproveAttachmentId IS NOT NULL THEN 1 ELSE 0 END as IsUploaded,
                ataa.AssetTransactionApproveAttachmentId as AttachmentId
            FROM MasterData md
            LEFT JOIN AssetTransactionApproveAttachment ataa ON md.ReferenceCode = ataa.DocumentType AND ataa.AssetTransactionId = @TransactionId AND ataa.IsActive = 1
            WHERE md.ReferenceName = 'ApproveDocumentType' AND md.IsActive = 1
            ORDER BY md.ReferenceCode";

        return await _context.QueryAsync<RequiredDocumentTypeDto>(sql, new { TransactionId = transactionId });
    }

    public async Task<bool> IsAllRequiredDocumentsUploadedAsync(int transactionId)
    {
        var requiredDocs = await GetRequiredDocumentTypesAsync(transactionId);
        return requiredDocs.All(d => d.IsUploaded);
    }

    public async Task<bool> IsAllRequiredDocumentsApprovedAsync(int transactionId)
    {
        const string sql = @"
            SELECT COUNT(*)
            FROM MasterData md
            LEFT JOIN AssetTransactionApproveAttachment ataa ON md.ReferenceCode = ataa.DocumentType AND ataa.AssetTransactionId = @TransactionId AND ataa.IsActive = 1
            WHERE md.ReferenceName = 'ApproveDocumentType'
              AND md.IsActive = 1
              AND (ataa.AssetTransactionApproveAttachmentId IS NULL OR ataa.Approved = 0 OR ataa.NeedToChange = 1)";

        var count = await _context.ExecuteScalarAsync<int>(sql, new { TransactionId = transactionId });
        return count == 0;
    }
}