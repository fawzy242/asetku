using System.Data;

namespace Whitebird.Infra.Features.Common;

/// <summary>
/// Single infrastructure abstraction for generic repository primitives.
///
/// Replaces the previous <c>IGenericRepository&lt;T&gt;</c> + <c>GenericRepository&lt;T&gt;</c>
/// pair — this project has exactly one generic repository abstraction, not two.
///
/// Design principles
/// -----------------
/// * Method-level generics (<c>&lt;TEntity&gt;</c>) — a single injected instance serves
///   every entity type, unlike the old class-level generic repository.
/// * No general-purpose ORM: only the operations that are genuinely reused across
///   multiple feature services live here.
/// * Feature-specific SQL (joins, projections, dropdown shapes, uniqueness with
///   exclusions, pagination, filtered status queries) belongs to the concrete
///   repository (e.g. <c>ICategoryReps</c>), not here.
///
/// Conventions preserved from <c>GenericRepository&lt;T&gt;</c>:
/// * Table name = entity type name with the <c>Entity</c> suffix stripped.
/// * Primary key inference order: <c>{Table}Id</c>, <c>Id</c>, <c>{Table}_Id</c>.
/// * Insert excludes PK + CreatedDate/CreatedBy/ModifiedDate/ModifiedBy.
/// * Update excludes PK + CreatedDate/CreatedBy (ModifiedDate/ModifiedBy are written).
/// * Insert uses <c>OUTPUT INSERTED.{Pk}</c> with a <c>SCOPE_IDENTITY()</c> fallback for
///   trigger-populated tables.
/// * <c>BulkInsertOptimizedAsync</c> uses <c>SqlBulkCopy</c> when an explicit
///   <c>SqlTransaction</c> is available; otherwise falls back to per-row inserts
///   (compatible with ambient <c>TransactionScope</c>).
/// </summary>
public interface ICommonReps
{
    /// <summary>
    /// Loads an entity by primary key. Returns <c>null</c> if no row matches.
    /// </summary>
    Task<TEntity?> GetByIdAsync<TEntity>(object id) where TEntity : class;

    /// <summary>
    /// Inserts a new entity, returns the new identity value (boxed int), and writes
    /// the new ID back onto <paramref name="entity"/>'s PK property.
    /// </summary>
    Task<object> InsertAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null) where TEntity : class;

    /// <summary>
    /// Updates an existing entity by primary key. Returns the number of rows affected.
    /// </summary>
    Task<int> UpdateAsync<TEntity>(TEntity entity, IDbTransaction? transaction = null) where TEntity : class;

    /// <summary>
    /// Permanently deletes a single row by primary key.
    /// </summary>
    Task<int> DeleteAsync<TEntity>(object id, IDbTransaction? transaction = null) where TEntity : class;

    /// <summary>
    /// Soft-deletes a single row (sets IsActive = 0, ModifiedDate = GETDATE(),
    /// ModifiedBy = <paramref name="modifiedBy"/>). Requires the entity to expose
    /// those audit properties.
    /// </summary>
    Task<int> SoftDeleteAsync<TEntity>(object id, string modifiedBy) where TEntity : class;

    /// <summary>
    /// Inserts many entities. Uses SqlBulkCopy when an explicit SqlTransaction is
    /// available, otherwise falls back to per-row inserts (compatible with ambient
    /// TransactionScope).
    /// </summary>
    Task<int> BulkInsertOptimizedAsync<TEntity>(IEnumerable<TEntity> entities, IDbTransaction? transaction = null) where TEntity : class;
}