using Whitebird.Domain.Features.Office;

namespace Whitebird.Infra.Features.Office;

public interface IOfficeReps
{
    Task<OfficeEntity?> GetByIdAsync(int officeId);
    Task<OfficeEntity?> GetByIdWithRelationsAsync(int officeId);
    Task<IEnumerable<OfficeEntity>> GetAllAsync();
    Task<IEnumerable<OfficeEntity>> GetActiveOnlyAsync();
    Task<IEnumerable<OfficeEntity>> GetSubOfficesAsync(int parentOfficeId);
    Task<bool> IsOfficeCodeExistsAsync(string officeCode, int? excludeOfficeId = null);
    Task<bool> IsOfficeNameExistsAsync(string officeName, int? excludeOfficeId = null);
    Task<int> GetChildCountAsync(int officeId);
}