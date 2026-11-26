using Microsoft.EntityFrameworkCore;
using SitesLend.Api.Data;
using SitesLend.Api.Dtos;

namespace SitesLend.Api.Services;

public class AssetService(AppDbContext db) : IAssetService
{
    public async Task<PagedResult<AssetUnitDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var units = db.AssetUnits.AsNoTracking();

        var totalCount = await units.CountAsync(cancellationToken);

        // Select straight into the DTO so entities and internal columns never leave this method.
        var items = await units
            .OrderBy(u => u.AssetTag)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AssetUnitDto(u.Id, u.AssetTag, u.Category.Name, u.Description, u.Condition, u.State))
            .ToListAsync(cancellationToken);

        return new PagedResult<AssetUnitDto>(items, totalCount, page, pageSize);
    }
}
