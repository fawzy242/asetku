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
    // CRUD - return Entity
    // ============================================================

    public async Task<AssetTransactionEntity?> GetByIdRawAsync(int transactionId)
    {
        const string sql = "SELECT * FROM AssetTransaction WHERE AssetTransactionId = @TransactionId";
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { TransactionId = transactionId });
    }

    public async Task<int> GetTransactionCountByAssetAsync(int assetId)
    {
        const string sql = "SELECT COUNT(*) FROM AssetTransaction WHERE AssetId = @AssetId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { AssetId = assetId });
    }

    public async Task<bool> HasOpenPairedTransactionAsync(int assetId, int transactionType)
    {
        var pairingTypes = new[] { TransactionTypeConstants.LOAN, TransactionTypeConstants.MAINTENANCE };
        if (!pairingTypes.Contains(transactionType))
            return false;

        const string sql = @"
            SELECT COUNT(1) FROM AssetTransaction
            WHERE AssetId = @AssetId
              AND TransactionType = @TransactionType
              AND Approved = 1
              AND FromAssetTransactionId IS NULL
              AND ActualReturnDate IS NULL
              AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { AssetId = assetId, TransactionType = transactionType }) > 0;
    }

    // ============================================================
    // GET METHODS - return View
    // ============================================================

    public async Task<AssetTransactionDetailView?> GetDetailByIdAsync(int transactionId)
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.AssetTransactionId = @TransactionId AND t.IsActive = 1";
        
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionDetailView>(sql, new { TransactionId = transactionId });
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        
        return await _context.QueryAsync<AssetTransactionListView>(sql);
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetByAssetIdListViewAsync(int assetId)
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.AssetId = @AssetId AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        
        return await _context.QueryAsync<AssetTransactionListView>(sql, new { AssetId = assetId });
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetByEmployeeIdListViewAsync(int employeeId)
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE (t.FromEmployeeId = @EmployeeId OR t.ToEmployeeId = @EmployeeId) AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        
        return await _context.QueryAsync<AssetTransactionListView>(sql, new { EmployeeId = employeeId });
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetByApprovalStatusListViewAsync(bool? approved)
    {
        var sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.IsActive = 1";

        if (approved == true)
        {
            sql += " AND t.Approved = 1";
        }
        else if (approved == false)
        {
            sql += " AND t.Approved = 0";
        }

        sql += " ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<AssetTransactionListView>(sql);
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetPendingApprovalsListViewAsync()
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.Approved IS NULL AND t.IsActive = 1
            ORDER BY t.TransactionDate ASC";
        
        return await _context.QueryAsync<AssetTransactionListView>(sql);
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetApprovedListViewAsync()
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.Approved = 1 AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        
        return await _context.QueryAsync<AssetTransactionListView>(sql);
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetRejectedListViewAsync()
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.Approved = 0 AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        
        return await _context.QueryAsync<AssetTransactionListView>(sql);
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetActiveLoansListViewAsync()
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.TransactionType = @LoanType
              AND t.Approved = 1
              AND t.FromAssetTransactionId IS NULL
              AND t.ActualReturnDate IS NULL
              AND t.IsActive = 1
            ORDER BY t.ExpectedReturnDate";
        
        return await _context.QueryAsync<AssetTransactionListView>(sql, new { LoanType = TransactionTypeConstants.LOAN });
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetOverdueLoansListViewAsync()
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.TransactionType = @LoanType
              AND t.Approved = 1
              AND t.FromAssetTransactionId IS NULL
              AND t.ExpectedReturnDate < GETDATE()
              AND t.ActualReturnDate IS NULL
              AND t.IsActive = 1
            ORDER BY t.ExpectedReturnDate";
        
        return await _context.QueryAsync<AssetTransactionListView>(sql, new { LoanType = TransactionTypeConstants.LOAN });
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetByDateRangeListViewAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.TransactionDate BETWEEN @StartDate AND @EndDate AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        
        return await _context.QueryAsync<AssetTransactionListView>(sql, new { StartDate = startDate, EndDate = endDate });
    }

    public async Task<AssetTransactionListView?> GetActiveTransactionByAssetIdAsync(int assetId)
    {
        const string sql = @"
            SELECT TOP 1
                t.AssetTransactionId, t.AssetId,
                a.AssetCode, a.AssetName,
                t.TransactionType, md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId, fe.FullName as FromEmployeeName,
                t.ToEmployeeId, te.FullName as ToEmployeeName,
                t.ToLocationId, tl.OfficeName as ToLocationName,
                t.TransactionDate, t.ExpectedReturnDate, t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore, md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter, md3.MasterDataName as ConditionAfterName,
                t.Approved, t.ApprovedBy,
                t.MaintenanceType, md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost, t.FromAssetTransactionId,
                t.IsActive, t.CreatedDate, t.CreatedBy, t.ModifiedDate, t.ModifiedBy
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition'
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType'
            WHERE t.AssetId = @AssetId 
              AND t.Approved = 1 
              AND t.FromAssetTransactionId IS NULL
              AND t.TransactionType IN (@HandoverType, @TransferType, @LoanType, @MaintenanceType)
              AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";
        
        var parameters = new
        {
            AssetId = assetId,
            HandoverType = TransactionTypeConstants.HANDOVER,
            TransferType = TransactionTypeConstants.TRANSFER,
            LoanType = TransactionTypeConstants.LOAN,
            MaintenanceType = TransactionTypeConstants.MAINTENANCE
        };
        
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionListView>(sql, parameters);
    }

    public async Task<AssetTransactionEntity?> GetPairedTransactionAsync(int transactionId)
    {
        const string sql = @"
            SELECT t.*, a.AssetCode, a.AssetName, 
                   fe.FullName as FromEmployeeName, te.FullName as ToEmployeeName,
                   md1.MasterDataName as TransactionTypeName
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType'
            WHERE t.FromAssetTransactionId = @TransactionId AND t.IsActive = 1";
        
        return await _context.QueryFirstOrDefaultAsync<AssetTransactionEntity>(sql, new { TransactionId = transactionId });
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetAssetTransactionHistoryAsync(int assetId)
    {
        return await GetByAssetIdListViewAsync(assetId);
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetEmployeeTransactionHistoryAsync(int employeeId)
    {
        return await GetByEmployeeIdListViewAsync(employeeId);
    }

    // ============================================================
    // GRID/LIST METHODS
    // ============================================================

    private string BuildBaseTransactionQuery(string selectClause, string whereClause, string orderBy, bool includePagination = false, int offset = 0, int pageSize = 0)
    {
        var sql = $@"
            {selectClause}
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            LEFT JOIN Office tl ON t.ToLocationId = tl.OfficeId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON t.ConditionBefore = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            LEFT JOIN MasterData md3 ON t.ConditionAfter = md3.ReferenceCode AND md3.ReferenceName = 'AssetCondition' AND md3.IsActive = 1
            LEFT JOIN MasterData md4 ON t.MaintenanceType = md4.ReferenceCode AND md4.ReferenceName = 'MaintenanceType' AND md4.IsActive = 1
            {whereClause}
            {orderBy}";

        if (includePagination)
        {
            sql += $" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        }

        return sql;
    }

    private string BuildTransactionListViewSelectClause()
    {
        return @"
            SELECT 
                t.AssetTransactionId,
                t.AssetId,
                a.AssetCode,
                a.AssetName,
                t.TransactionType,
                md1.MasterDataName as TransactionTypeName,
                t.FromEmployeeId,
                fe.FullName as FromEmployeeName,
                t.ToEmployeeId,
                te.FullName as ToEmployeeName,
                t.ToLocationId,
                tl.OfficeName as ToLocationName,
                t.TransactionDate,
                t.ExpectedReturnDate,
                t.ActualReturnDate,
                t.Notes,
                t.ConditionBefore,
                md2.MasterDataName as ConditionBeforeName,
                t.ConditionAfter,
                md3.MasterDataName as ConditionAfterName,
                t.Approved,
                t.ApprovedBy,
                t.MaintenanceType,
                md4.MasterDataName as MaintenanceTypeName,
                t.MaintenanceCost,
                t.FromAssetTransactionId,
                t.IsActive,
                t.CreatedDate,
                t.CreatedBy,
                t.ModifiedDate,
                t.ModifiedBy";
    }

    private (string WhereClause, DynamicParameters Parameters) BuildTransactionWhereClause(
        string? search = null,
        bool? approved = null,
        int? assetId = null,
        int? transactionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        bool includeOnlyActive = true)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (includeOnlyActive)
        {
            conditions.Add("t.IsActive = 1");
        }

        if (approved == true)
        {
            conditions.Add("t.Approved = 1");
        }
        else if (approved == false)
        {
            conditions.Add("t.Approved = 0");
        }

        if (assetId.HasValue && assetId.Value > 0)
        {
            conditions.Add("t.AssetId = @AssetId");
            parameters.Add("@AssetId", assetId.Value);
        }

        if (transactionType.HasValue && transactionType.Value > 0)
        {
            conditions.Add("t.TransactionType = @TransactionType");
            parameters.Add("@TransactionType", transactionType.Value);
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

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR fe.FullName LIKE @Search OR te.FullName LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        return (whereClause, parameters);
    }

    public async Task<PaginatedResult<AssetTransactionListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null)
    {
        var (whereClause, parameters) = BuildTransactionWhereClause(search, approved, assetId);

        var countSql = $@"
            SELECT COUNT(*) FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            {whereClause}";
        
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = BuildTransactionListViewSelectClause();
        var orderBy = "ORDER BY t.TransactionDate DESC";
        var offset = (page - 1) * pageSize;
        var dataSql = BuildBaseTransactionQuery(selectClause, whereClause, orderBy, true, offset, pageSize);

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<AssetTransactionListView>(dataSql, parameters);

        return new PaginatedResult<AssetTransactionListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<PaginatedResult<AssetTransactionListView>> GetPendingPagedListAsync(
        int page, int pageSize, string? search = null, int? assetId = null)
    {
        var conditions = new List<string> { "t.IsActive = 1", "t.Approved IS NULL" };
        var parameters = new DynamicParameters();

        if (assetId.HasValue)
        {
            conditions.Add("t.AssetId = @AssetId");
            parameters.Add("@AssetId", assetId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(a.AssetCode LIKE @Search OR a.AssetName LIKE @Search OR fe.FullName LIKE @Search OR te.FullName LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        var whereClause = $"WHERE {string.Join(" AND ", conditions)}";

        var countSql = $@"
            SELECT COUNT(*) FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN Employee fe ON t.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON t.ToEmployeeId = te.EmployeeId
            {whereClause}";
        
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = BuildTransactionListViewSelectClause();
        var orderBy = "ORDER BY t.TransactionDate ASC";
        var offset = (page - 1) * pageSize;
        var dataSql = BuildBaseTransactionQuery(selectClause, whereClause, orderBy, true, offset, pageSize);

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<AssetTransactionListView>(dataSql, parameters);

        return new PaginatedResult<AssetTransactionListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetActiveLoansListAsync()
    {
        return await GetActiveLoansListViewAsync();
    }

    public async Task<IEnumerable<AssetTransactionListView>> GetOverdueLoansListAsync()
    {
        return await GetOverdueLoansListViewAsync();
    }

    public async Task<IEnumerable<AssetTransactionDropdownView>> GetAvailablePairedTransactionsAsync(int assetId, int pairSourceType)
    {
        const string sql = @"
            SELECT 
                t.AssetTransactionId,
                t.AssetId,
                a.AssetCode,
                t.TransactionType,
                md1.MasterDataName as TransactionTypeName,
                t.TransactionDate,
                t.Approved,
                CASE WHEN t.FromAssetTransactionId IS NOT NULL THEN 1 ELSE 0 END as IsPaired
            FROM AssetTransaction t
            LEFT JOIN Asset a ON t.AssetId = a.AssetId
            LEFT JOIN MasterData md1 ON t.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            WHERE t.AssetId = @AssetId
              AND t.TransactionType = @PairSourceType
              AND t.Approved = 1
              AND t.FromAssetTransactionId IS NULL
              AND t.IsActive = 1
            ORDER BY t.TransactionDate DESC";

        return await _context.QueryAsync<AssetTransactionDropdownView>(sql, new { AssetId = assetId, PairSourceType = pairSourceType });
    }
}