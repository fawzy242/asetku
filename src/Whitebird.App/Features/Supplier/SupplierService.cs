using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Supplier;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Supplier;

namespace Whitebird.App.Features.Supplier;

public class SupplierService
    : CommonService<SupplierEntity, SupplierDetailView, SupplierListView, SupplierGridRequest, SupplierCreateViewModel, SupplierUpdateViewModel>,
      ISupplierService
{
    private readonly ISupplierReps _supplierReps;

    public SupplierService(
        ICommonReps repository,
        ISupplierReps supplierReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<SupplierService> logger)
        : base(repository, currentUserService, activityLogService, logger)
    {
        _supplierReps = supplierReps;
    }

    protected override string EntityLabel => "Supplier";
    protected override string TableName => TableNames.Supplier;

    protected override string BulkDeactivateEmptyMessage =>
        "No suppliers were deactivated. Suppliers with associated assets cannot be deactivated.";
    protected override string BulkDeleteEmptyMessage =>
        "No suppliers were deleted. Suppliers with associated assets cannot be deleted.";

    protected override async Task<SupplierDetailView?> LoadDetailAsync(int id) =>
        await _supplierReps.GetDetailByIdAsync(id);

    protected override async Task<IEnumerable<SupplierListView>> LoadAllAsync() =>
        await _supplierReps.GetAllListViewAsync();

    protected override async Task<PaginatedResult<SupplierListView>> LoadPageAsync(SupplierGridRequest request) =>
        await _supplierReps.GetPagedListAsync(request);

    protected override async Task<ServiceResult<SupplierDetailView>?> ValidateCreateAsync(SupplierCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SupplierName))
            return ServiceResult<SupplierDetailView>.BadRequest("Supplier name is required");

        if (await _supplierReps.IsSupplierNameExistsAsync(model.SupplierName))
            return ServiceResult<SupplierDetailView>.Conflict($"Supplier '{model.SupplierName}' already exists");

        return null;
    }

    protected override async Task<ServiceResult<SupplierDetailView>?> ValidateUpdateAsync(int id, SupplierUpdateViewModel model)
    {
        if (await _supplierReps.IsSupplierNameExistsAsync(model.SupplierName, id))
            return ServiceResult<SupplierDetailView>.Conflict($"Supplier '{model.SupplierName}' already exists");

        return null;
    }

    protected override void MapCreate(SupplierCreateViewModel model, SupplierEntity entity) => model.Adapt(entity);
    protected override void MapUpdate(SupplierUpdateViewModel model, SupplierEntity entity) => model.Adapt(entity);
    protected override string Describe(SupplierEntity entity) => entity.SupplierName;

    protected override async Task<(bool CanProceed, string? Reason)> CanDeleteAsync(int id)
    {
        var assetCount = await _supplierReps.GetAssetCountAsync(id);
        return assetCount > 0
            ? (false, $"Cannot delete supplier with {assetCount} associated assets")
            : (true, (string?)null);
    }

    protected override async Task<(bool CanProceed, string? Reason)> CanDeactivateAsync(int id)
    {
        var assetCount = await _supplierReps.GetAssetCountAsync(id);
        return assetCount > 0 ? (false, (string?)null) : (true, (string?)null);
    }

    public async Task<ServiceResult<IEnumerable<SupplierDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var suppliers = await _supplierReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<SupplierDropdownView>>.Success(suppliers);
        }, "get supplier dropdown list");
    }
}