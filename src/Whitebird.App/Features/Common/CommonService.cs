using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Common;

/// <summary>
/// Base class implementing the shared CRUD / grid / bulk pattern used by
/// Category, Department, Office, Supplier, Roles (and, via the hybrid pattern,
/// the three Schedule modules).
///
/// Hardening applied:
///   * SoftDeleteAsync and the BR-01 path in UpdateAsync now defer to
///     CanDeactivateAsync before transitioning IsActive from true to false.
///     A blocked deactivation returns 400 with the guard's reason instead of
///     silently deactivating or ignoring the guard.
///   * CreateAsync converts SQL unique-constraint exceptions (2601/2627) into
///     a clean 409 Conflict so the advisory uniqueness check + DB constraint
///     race surfaces as a deterministic error instead of a 500.
/// </summary>
public abstract class CommonService<TEntity, TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM>
    : BaseService, ICommonService<TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM>
    where TEntity : AuditableEntity, new()
    where TDetailView : class
    where TListView : class
    where TGridRequest : GridRequestBase
    where TCreateVM : class
    where TUpdateVM : class
{
    protected readonly ICommonReps Repository;
    protected readonly ICurrentUserService CurrentUserService;
    protected readonly IActivityLogService ActivityLogService;

    protected CommonService(
        ICommonReps repository,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger logger) : base(logger)
    {
        Repository = repository;
        CurrentUserService = currentUserService;
        ActivityLogService = activityLogService;
    }

    // ============================================================
    // ABSTRACT HOOKS
    // ============================================================

    protected abstract string EntityLabel { get; }
    protected abstract string TableName { get; }
    protected abstract Task<TDetailView?> LoadDetailAsync(int id);
    protected abstract Task<IEnumerable<TListView>> LoadAllAsync();
    protected abstract Task<PaginatedResult<TListView>> LoadPageAsync(TGridRequest request);
    protected abstract Task<ServiceResult<TDetailView>?> ValidateCreateAsync(TCreateVM model);
    protected abstract Task<ServiceResult<TDetailView>?> ValidateUpdateAsync(int id, TUpdateVM model);
    protected abstract void MapCreate(TCreateVM model, TEntity entity);
    protected abstract void MapUpdate(TUpdateVM model, TEntity entity);
    protected abstract string Describe(TEntity entity);

    // ============================================================
    // VIRTUAL HOOKS
    // ============================================================

    protected virtual string EntityLabelPlural => $"{EntityLabel}s";
    protected virtual bool ApplyBr01 => true;

    protected virtual Task<(bool CanProceed, string? Reason)> CanDeleteAsync(int id) =>
        Task.FromResult((true, (string?)null));

    protected virtual Task<(bool CanProceed, string? Reason)> CanDeactivateAsync(int id) =>
        Task.FromResult((true, (string?)null));

    protected virtual string BulkDeactivateEmptyMessage =>
        $"No {EntityLabelPlural.ToLowerInvariant()} were deactivated.";

    protected virtual string BulkDeleteEmptyMessage =>
        $"No {EntityLabelPlural.ToLowerInvariant()} were deleted.";

    // ============================================================
    // GET
    // ============================================================

    public virtual async Task<ServiceResult<TDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var detail = await LoadDetailAsync(id);
            if (detail == null)
            {
                return ServiceResult<TDetailView>.NotFound($"{EntityLabel} with id {id} not found");
            }
            return ServiceResult<TDetailView>.Success(detail);
        }, $"get {EntityLabel.ToLowerInvariant()} by id");
    }

    public virtual async Task<ServiceResult<IEnumerable<TListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var items = await LoadAllAsync();
            return ServiceResult<IEnumerable<TListView>>.Success(items);
        }, $"get all {EntityLabelPlural.ToLowerInvariant()}");
    }

    // ============================================================
    // CREATE
    // ============================================================

    public virtual async Task<ServiceResult<TDetailView>> CreateAsync(TCreateVM model)
    {
        var validation = await ValidateCreateAsync(model);
        if (validation != null) return validation;

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = new TEntity();
            MapCreate(model, entity);
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = CurrentUserService.GetDisplayName();

            object insertedId;
            try
            {
                insertedId = await Repository.InsertAsync(entity);
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                // Advisory ValidateCreateAsync usually catches duplicates, but a
                // concurrent insert can slip past the advisory check. Convert the
                // DB unique-constraint violation into a deterministic 409.
                return ServiceResult<TDetailView>.Conflict(
                    $"{EntityLabel} with the same unique key already exists");
            }

            var id = Convert.ToInt32(insertedId);

            var created = await LoadDetailAsync(id);

            if (created != null)
            {
                await ActivityLogService.LogCreateAsync(
                    TableName,
                    id,
                    $"{EntityLabel} '{Describe(entity)}' created successfully",
                    CurrentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<TDetailView>.Failure($"Failed to retrieve created {EntityLabel.ToLowerInvariant()}")
                : ServiceResult<TDetailView>.Success(created, $"{EntityLabel} created successfully");
        }, $"create {EntityLabel.ToLowerInvariant()}");
    }

    // ============================================================
    // UPDATE
    // ============================================================

    public virtual async Task<ServiceResult<TDetailView>> UpdateAsync(int id, TUpdateVM model)
    {
        var validation = await ValidateUpdateAsync(id, model);
        if (validation != null) return validation;

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await Repository.GetByIdAsync<TEntity>(id);
            if (existing == null)
            {
                return ServiceResult<TDetailView>.NotFound($"{EntityLabel} with id {id} not found");
            }

            var wasActive = existing.IsActive;
            var beforeDescription = Describe(existing);

            MapUpdate(model, existing);

            // Determine the target IsActive:
            //   * BR-01 forces Inactive when the record was Active on entry.
            //   * Otherwise the value set by MapUpdate (from the client) is used.
            var targetIsActive = ApplyBr01 && wasActive ? false : existing.IsActive;

            // Deactivation (true → false) must pass CanDeactivateAsync. This closes
            // the pre-hardening gap where BR-01 and a client-supplied IsActive = false
            // both silently bypassed the guard (e.g. deactivating a role that still
            // has assigned users, or a category that still has subcategories).
            if (wasActive && !targetIsActive)
            {
                var (canDeactivate, reason) = await CanDeactivateAsync(id);
                if (!canDeactivate)
                {
                    return ServiceResult<TDetailView>.BadRequest(
                        reason ?? $"Cannot deactivate {EntityLabel.ToLowerInvariant()}");
                }
            }

            existing.IsActive = targetIsActive;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = CurrentUserService.GetDisplayName();

            var result = await Repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<TDetailView>.Failure($"Failed to update {EntityLabel.ToLowerInvariant()}");
            }

            var updated = await LoadDetailAsync(id);
            var afterDescription = Describe(existing);

            await ActivityLogService.LogUpdateAsync(
                TableName,
                id,
                $"{EntityLabel} updated: '{beforeDescription}' -> '{afterDescription}'",
                CurrentUserService.GetDisplayName());

            return updated == null
                ? ServiceResult<TDetailView>.Failure($"Failed to retrieve updated {EntityLabel.ToLowerInvariant()}")
                : ServiceResult<TDetailView>.Success(updated, $"{EntityLabel} updated successfully");
        }, $"update {EntityLabel.ToLowerInvariant()}");
    }

    // ============================================================
    // DELETE
    // ============================================================

    public virtual async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await Repository.GetByIdAsync<TEntity>(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"{EntityLabel} with id {id} not found");
            }

            var (canDelete, reason) = await CanDeleteAsync(id);
            if (!canDelete)
            {
                return ServiceResult.BadRequest(reason ?? $"Cannot delete {EntityLabel.ToLowerInvariant()}");
            }

            var result = await Repository.DeleteAsync<TEntity>(id);

            if (result > 0)
            {
                await ActivityLogService.LogDeleteAsync(
                    TableName,
                    id,
                    $"{EntityLabel} '{Describe(existing)}' deleted permanently",
                    CurrentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure($"Failed to delete {EntityLabel.ToLowerInvariant()}")
                : ServiceResult.Success($"{EntityLabel} deleted successfully");
        }, $"delete {EntityLabel.ToLowerInvariant()}");
    }

    public virtual async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await Repository.GetByIdAsync<TEntity>(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"{EntityLabel} with id {id} not found");
            }

            // Defer to the deactivation guard — same rule BulkDeactivateAsync already
            // applies — so a soft-deleted role/category/office/supplier with active
            // references cannot be flipped inactive here while the guard blocks it
            // elsewhere.
            var (canDeactivate, reason) = await CanDeactivateAsync(id);
            if (!canDeactivate)
            {
                return ServiceResult.BadRequest(
                    reason ?? $"Cannot soft delete {EntityLabel.ToLowerInvariant()}");
            }

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = CurrentUserService.GetDisplayName();

            var result = await Repository.UpdateAsync(existing);

            if (result > 0)
            {
                await ActivityLogService.LogSoftDeleteAsync(
                    TableName,
                    id,
                    $"{EntityLabel} '{Describe(existing)}' soft deleted",
                    CurrentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure($"Failed to soft delete {EntityLabel.ToLowerInvariant()}")
                : ServiceResult.Success($"{EntityLabel} soft deleted successfully");
        }, $"soft delete {EntityLabel.ToLowerInvariant()}");
    }

    // ============================================================
    // GRID
    // ============================================================

    public virtual async Task<ServiceResult<PaginatedResult<TListView>>> GetGridDataAsync(TGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await LoadPageAsync(request);
            return ServiceResult<PaginatedResult<TListView>>.Success(result);
        }, $"get {EntityLabel.ToLowerInvariant()} grid data");
    }

    // ============================================================
    // BULK
    // ============================================================

    public virtual async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request?.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest($"No {EntityLabel.ToLowerInvariant()} IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var count = 0;

            foreach (var id in request.Ids)
            {
                var entity = await Repository.GetByIdAsync<TEntity>(id);
                if (entity == null || entity.IsActive) continue;

                entity.IsActive = true;
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = CurrentUserService.GetDisplayName();
                await Repository.UpdateAsync(entity);
                count++;

                await ActivityLogService.LogUpdateAsync(
                    TableName,
                    id,
                    $"{EntityLabel} '{Describe(entity)}' activated via bulk operation",
                    CurrentUserService.GetDisplayName());
            }

            return ServiceResult<int>.Success(
                count,
                $"{count} {EntityLabel.ToLowerInvariant()}(s) activated successfully");
        }, $"bulk activate {EntityLabelPlural.ToLowerInvariant()}");
    }

    public virtual async Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request)
    {
        if (request?.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest($"No {EntityLabel.ToLowerInvariant()} IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var count = 0;

            foreach (var id in request.Ids)
            {
                var entity = await Repository.GetByIdAsync<TEntity>(id);
                if (entity == null || !entity.IsActive) continue;

                var (canDeactivate, _) = await CanDeactivateAsync(id);
                if (!canDeactivate) continue;

                entity.IsActive = false;
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = CurrentUserService.GetDisplayName();
                await Repository.UpdateAsync(entity);
                count++;

                await ActivityLogService.LogUpdateAsync(
                    TableName,
                    id,
                    $"{EntityLabel} '{Describe(entity)}' deactivated via bulk operation",
                    CurrentUserService.GetDisplayName());
            }

            var message = count > 0
                ? $"{count} {EntityLabel.ToLowerInvariant()}(s) deactivated successfully"
                : BulkDeactivateEmptyMessage;

            return ServiceResult<int>.Success(count, message);
        }, $"bulk deactivate {EntityLabelPlural.ToLowerInvariant()}");
    }

    public virtual async Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request)
    {
        if (request?.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest($"No {EntityLabel.ToLowerInvariant()} IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var count = 0;

            foreach (var id in request.Ids)
            {
                var entity = await Repository.GetByIdAsync<TEntity>(id);
                if (entity == null) continue;

                var (canDelete, _) = await CanDeleteAsync(id);
                if (!canDelete) continue;

                var result = await Repository.DeleteAsync<TEntity>(id);
                if (result > 0)
                {
                    count++;

                    await ActivityLogService.LogDeleteAsync(
                        TableName,
                        id,
                        $"{EntityLabel} '{Describe(entity)}' deleted via bulk operation",
                        CurrentUserService.GetDisplayName());
                }
            }

            var message = count > 0
                ? $"{count} {EntityLabel.ToLowerInvariant()}(s) deleted successfully"
                : BulkDeleteEmptyMessage;

            return ServiceResult<int>.Success(count, message);
        }, $"bulk delete {EntityLabelPlural.ToLowerInvariant()}");
    }
}