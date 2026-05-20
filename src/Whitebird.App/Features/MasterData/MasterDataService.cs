using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.MasterData;

namespace Whitebird.App.Features.MasterData;

public class MasterDataService : BaseService, IMasterDataService
{
    private readonly IMasterDataReps _masterDataReps;

    public MasterDataService(IMasterDataReps masterDataReps, ILogger<MasterDataService> logger)
        : base(logger)
    {
        _masterDataReps = masterDataReps;
    }

    public async Task<ServiceResult<IEnumerable<MasterDataGroupDto>>> GetAllGroupedAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var allData = await _masterDataReps.GetAllAsync();
            var grouped = allData
                .GroupBy(x => x.ReferenceName)
                .Select(g => new MasterDataGroupDto
                {
                    ReferenceName = g.Key,
                    Values = g.Select(v => new MasterDataDto
                    {
                        Code = v.ReferenceCode,
                        Name = v.MasterDataName
                    }).ToList()
                })
                .ToList();

            return ServiceResult<IEnumerable<MasterDataGroupDto>>.Success(grouped);
        }, "get all master data grouped");
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetByReferenceNameAsync(string referenceName)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _masterDataReps.GetByReferenceNameAsync(referenceName);
            var result = data.Select(x => new MasterDataDto
            {
                Code = x.ReferenceCode,
                Name = x.MasterDataName
            }).ToList();

            return ServiceResult<IEnumerable<MasterDataDto>>.Success(result);
        }, $"get master data by reference name: {referenceName}");
    }

    public async Task<ServiceResult<string?>> GetValueAsync(string referenceName, int code)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _masterDataReps.GetByReferenceNameAndCodeAsync(referenceName, code);
            return ServiceResult<string?>.Success(data?.MasterDataName);
        }, $"get master data value: {referenceName}/{code}");
    }

    public async Task<ServiceResult<int?>> GetCodeAsync(string referenceName, string value)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var allData = await _masterDataReps.GetByReferenceNameAsync(referenceName);
            var match = allData.FirstOrDefault(x =>
                x.MasterDataName.Equals(value, StringComparison.OrdinalIgnoreCase));

            return ServiceResult<int?>.Success(match?.ReferenceCode);
        }, $"get master data code: {referenceName}/{value}");
    }

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypesAsync()
        => await GetByReferenceNameAsync("TransactionType");

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetAssetConditionsAsync()
        => await GetByReferenceNameAsync("AssetCondition");

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetEmployeePositionsAsync()
        => await GetByReferenceNameAsync("Position");

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetEmployeeStatusesAsync()
        => await GetByReferenceNameAsync("EmployeeStatus");

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetOfficeTypesAsync()
        => await GetByReferenceNameAsync("OfficeType");

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMaintenanceTypesAsync()
        => await GetByReferenceNameAsync("MaintenanceType");

    public async Task<ServiceResult<IEnumerable<MasterDataDto>>> GetAssetConditionPurchasesAsync()
        => await GetByReferenceNameAsync("AssetConditionPurchase");
}