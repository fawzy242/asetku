using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.MasterData;

public class MasterDataEntity : AuditableEntity
{
    public int MasterDataId { get; set; }
    public int ReferenceCode { get; set; }
    public string ReferenceName { get; set; } = default!;
    public string MasterDataName { get; set; } = default!;
}