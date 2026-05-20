namespace Whitebird.Domain.Features.MasterData;

public class MasterDataListViewModel
{
    public int MasterDataId { get; set; }
    public int ReferenceCode { get; set; }
    public string ReferenceName { get; set; } = default!;
    public string MasterDataName { get; set; } = default!;
    public bool IsActive { get; set; }
}

public class MasterDataDetailViewModel
{
    public int MasterDataId { get; set; }
    public int ReferenceCode { get; set; }
    public string ReferenceName { get; set; } = default!;
    public string MasterDataName { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class MasterDataDto
{
    public int Code { get; set; }
    public string Name { get; set; } = default!;
}

public class MasterDataGroupDto
{
    public string ReferenceName { get; set; } = default!;
    public List<MasterDataDto> Values { get; set; } = new();
}