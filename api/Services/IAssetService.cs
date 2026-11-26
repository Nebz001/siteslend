using SitesLend.Api.Dtos;

namespace SitesLend.Api.Services;

public interface IAssetService
{
    Task<PagedResult<AssetUnitDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);
}
