using Whitebird.Domain.Features.Location.Entities;

namespace Whitebird.Infra.Features.Location;

public interface ILocationReps
{
    Task<LocationEntity?> GetByIdRawAsync(int locationId);
    Task<LocationEntity?> GetByIdWithRelationsAsync(int locationId);
    Task<IEnumerable<LocationEntity>> GetAllWithRelationsAsync();
    Task<IEnumerable<LocationEntity>> GetActiveOnlyWithRelationsAsync();
    Task<IEnumerable<LocationEntity>> GetSubLocationAsync(int parentLocationId);
    Task<bool> IsLocationCodeExistsAsync(string locationCode, int? excludeLocationId = null);
    Task<bool> IsLocationNameExistsAsync(string locationName, int? excludeLocationId = null);
    Task<int> GetChildCountAsync(int locationId);
}