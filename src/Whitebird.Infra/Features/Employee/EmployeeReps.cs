using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Employee;

/// <summary>
/// Repository implementation for Employee operations using Dapper
/// </summary>
public class EmployeeReps : IEmployeeReps
{
    private readonly DapperContext _context;

    public EmployeeReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // CRUD - return Entity
    // ============================================================

    public async Task<EmployeeEntity?> GetByIdRawAsync(int employeeId)
    {
        const string sql = "SELECT * FROM Employee WHERE EmployeeId = @EmployeeId";
        return await _context.QueryFirstOrDefaultAsync<EmployeeEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null)
    {
        var sql = "SELECT COUNT(1) FROM Employee WHERE EmployeeCode = @EmployeeCode";
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeCode", employeeCode);

        if (excludeEmployeeId.HasValue)
        {
            sql += " AND EmployeeId != @ExcludeEmployeeId";
            parameters.Add("@ExcludeEmployeeId", excludeEmployeeId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    // ============================================================
    // GET METHODS - return View
    // ============================================================

    public async Task<EmployeeDetailView?> GetDetailByIdAsync(int employeeId)
    {
        const string sql = @"
        SELECT 
            e.EmployeeId, e.EmployeeCode, e.FullName, e.Address,
            e.DepartmentId, d.DepartmentName,
            e.Position, md1.MasterDataName as PositionName,
            e.EmploymentStatus, md2.MasterDataName as EmploymentStatusName,
            e.PhoneNumber, e.Email,
            e.OfficeId, o.OfficeName,
            e.JoinDate, e.ResignDate,
            e.IsActive, e.CreatedDate, e.CreatedBy, e.ModifiedDate, e.ModifiedBy
        FROM Employee e
        LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
        LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
        LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
        LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
        WHERE e.EmployeeId = @EmployeeId";

        var result = await _context.QueryFirstOrDefaultAsync<EmployeeDetailView>(sql, new { EmployeeId = employeeId });

        if (result != null)
        {
            // Photo profile — resolves through the Employee menu's own photo endpoint
            // (GET /api/Employee/{id}/photo). FileAttachment has no HTTP surface of its own.
            var photoSql = @"
            SELECT TOP 1
                fa.FileAttachmentId,
                '/api/Employee/' + CAST(eppa.EmployeeId AS VARCHAR) + '/photo' as FileUrl
            FROM EmployeePhotoProfileAttachment eppa
            INNER JOIN FileAttachment fa ON eppa.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            WHERE eppa.EmployeeId = @EmployeeId AND eppa.IsActive = 1
            ORDER BY fa.IsPrimary DESC, fa.CreatedDate DESC";

            var photo = await _context.QueryFirstOrDefaultAsync<dynamic>(photoSql, new { EmployeeId = employeeId });
            if (photo != null)
            {
                result.PhotoProfileUrl = photo.FileUrl;
            }

            // Attachments — resolve through the Employee menu's own download endpoint
            // (GET /api/Employee/{id}/attachments/{fileAttachmentId}/download).
            var attSql = @"
            SELECT 
                ea.EmployeeAttachmentId,
                ea.FileAttachmentId,
                fa.FileName,
                fa.OriginalFileName,
                fa.FileCategory,
                fa.IsPrimary,
                ea.[Order],
                ea.CreatedDate,
                '/api/Employee/' + CAST(ea.EmployeeId AS VARCHAR) + '/attachments/' + CAST(fa.FileAttachmentId AS VARCHAR) + '/download' as FileUrl
            FROM EmployeeAttachment ea
            INNER JOIN FileAttachment fa ON ea.FileAttachmentId = fa.FileAttachmentId AND fa.IsActive = 1
            WHERE ea.EmployeeId = @EmployeeId AND ea.IsActive = 1
            ORDER BY ea.[Order] ASC, fa.CreatedDate DESC";

            var attachments = await _context.QueryAsync<EmployeeAttachmentViewDto>(attSql, new { EmployeeId = employeeId });
            result.Attachments = attachments.ToList();
        }

        return result;
    }

    public async Task<IEnumerable<EmployeeListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                e.EmployeeId, e.EmployeeCode, e.FullName, e.Address,
                e.DepartmentId, d.DepartmentName,
                e.Position, md1.MasterDataName as PositionName,
                e.EmploymentStatus, md2.MasterDataName as EmploymentStatusName,
                e.PhoneNumber, e.Email,
                e.OfficeId, o.OfficeName,
                e.JoinDate, e.ResignDate,
                e.IsActive, e.CreatedDate, e.CreatedBy, e.ModifiedDate, e.ModifiedBy
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            ORDER BY e.FullName";

        return await _context.QueryAsync<EmployeeListView>(sql);
    }

    // ============================================================
    // PAGINATION
    // ============================================================

    public async Task<PaginatedResult<EmployeeListView>> GetPagedListAsync(EmployeeGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "e.IsActive", request.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "e.EmployeeCode", "e.FullName", "e.Email", "e.PhoneNumber" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "e.DepartmentId", request.DepartmentId);
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "e.OfficeId", request.OfficeId);
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "e.EmploymentStatus", request.EmploymentStatus);

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "e.FullName", false);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Employee e
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                e.EmployeeId, e.EmployeeCode, e.FullName, e.Address,
                e.DepartmentId, d.DepartmentName,
                e.Position, md1.MasterDataName as PositionName,
                e.EmploymentStatus, md2.MasterDataName as EmploymentStatusName,
                e.PhoneNumber, e.Email,
                e.OfficeId, o.OfficeName,
                e.JoinDate, e.ResignDate,
                e.IsActive, e.CreatedDate, e.CreatedBy, e.ModifiedDate, e.ModifiedBy,
                1 as CanEdit,
                1 as CanDelete";

        var fromClause = @"
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<EmployeeListView>(dataSql, parameters);

        return new PaginatedResult<EmployeeListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    public async Task<IEnumerable<EmployeeDropdownView>> GetDropdownListAsync()
    {
        const string sql = @"
            SELECT EmployeeId, EmployeeCode, FullName
            FROM Employee
            WHERE IsActive = 1
            ORDER BY FullName";

        return await _context.QueryAsync<EmployeeDropdownView>(sql);
    }

    // ============================================================
    // EMPLOYEE ATTACHMENT
    // ============================================================

    public async Task<int> InsertAttachmentAsync(EmployeeAttachmentEntity entity)
    {
        const string sql = @"
            INSERT INTO EmployeeAttachment (
                EmployeeId, FileAttachmentId, [Order], IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @EmployeeId, @FileAttachmentId, @Order, 1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<IEnumerable<EmployeeAttachmentEntity>> GetAttachmentsByEmployeeIdAsync(int employeeId)
    {
        const string sql = @"
            SELECT * FROM EmployeeAttachment 
            WHERE EmployeeId = @EmployeeId AND IsActive = 1
            ORDER BY [Order] ASC, CreatedDate DESC";

        return await _context.QueryAsync<EmployeeAttachmentEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<EmployeeAttachmentEntity?> GetAttachmentByIdAsync(int attachmentId)
    {
        const string sql = "SELECT * FROM EmployeeAttachment WHERE EmployeeAttachmentId = @AttachmentId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<EmployeeAttachmentEntity>(sql, new { AttachmentId = attachmentId });
    }

    public async Task<int> DeleteAttachmentAsync(int attachmentId)
    {
        const string sql = "UPDATE EmployeeAttachment SET IsActive = 0, ModifiedDate = GETDATE() WHERE EmployeeAttachmentId = @AttachmentId";
        return await _context.ExecuteAsync(sql, new { AttachmentId = attachmentId });
    }

    public async Task<int> DeleteAttachmentsByEmployeeIdAsync(int employeeId)
    {
        const string sql = "UPDATE EmployeeAttachment SET IsActive = 0, ModifiedDate = GETDATE() WHERE EmployeeId = @EmployeeId";
        return await _context.ExecuteAsync(sql, new { EmployeeId = employeeId });
    }

    // ============================================================
    // EMPLOYEE PHOTO PROFILE
    // ============================================================

    public async Task<int> InsertPhotoProfileAsync(EmployeePhotoProfileAttachmentEntity entity)
    {
        const string sql = @"
            INSERT INTO EmployeePhotoProfileAttachment (
                EmployeeId, FileAttachmentId, IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @EmployeeId, @FileAttachmentId, 1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<EmployeePhotoProfileAttachmentEntity?> GetPhotoProfileByEmployeeIdAsync(int employeeId)
    {
        const string sql = @"
            SELECT * FROM EmployeePhotoProfileAttachment 
            WHERE EmployeeId = @EmployeeId AND IsActive = 1";

        return await _context.QueryFirstOrDefaultAsync<EmployeePhotoProfileAttachmentEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> DeletePhotoProfileByEmployeeIdAsync(int employeeId)
    {
        const string sql = @"
            UPDATE EmployeePhotoProfileAttachment 
            SET IsActive = 0, ModifiedDate = GETDATE() 
            WHERE EmployeeId = @EmployeeId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, new { EmployeeId = employeeId });
    }
}