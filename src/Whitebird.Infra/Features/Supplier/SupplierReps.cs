using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Supplier.Entities;

namespace Whitebird.Infra.Features.Supplier;

public class SupplierReps : ISupplierReps
{
    private readonly DapperContext _context;

    public SupplierReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<SupplierEntity?> GetByIdAsync(int supplierId)
    {
        const string sql = "SELECT * FROM Supplier WHERE SupplierId = @SupplierId";
        return await _context.QueryFirstOrDefaultAsync<SupplierEntity>(sql, new { SupplierId = supplierId });
    }

    public async Task<IEnumerable<SupplierEntity>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Supplier ORDER BY SupplierName";
        return await _context.QueryAsync<SupplierEntity>(sql);
    }

    public async Task<IEnumerable<SupplierEntity>> GetActiveOnlyAsync()
    {
        const string sql = "SELECT * FROM Supplier WHERE IsActive = 1 ORDER BY SupplierName";
        return await _context.QueryAsync<SupplierEntity>(sql);
    }

    public async Task<bool> IsSupplierNameExistsAsync(string supplierName, int? excludeSupplierId = null)
    {
        var sql = "SELECT COUNT(1) FROM Supplier WHERE SupplierName = @SupplierName";
        var parameters = new DynamicParameters();
        parameters.Add("@SupplierName", supplierName);

        if (excludeSupplierId.HasValue)
        {
            sql += " AND SupplierId != @ExcludeSupplierId";
            parameters.Add("@ExcludeSupplierId", excludeSupplierId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<int> GetAssetCountAsync(int supplierId)
    {
        const string sql = "SELECT COUNT(*) FROM Asset WHERE SupplierId = @SupplierId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { SupplierId = supplierId });
    }
}