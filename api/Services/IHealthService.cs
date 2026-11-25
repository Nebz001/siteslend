using SitesLend.Api.Dtos;

namespace SitesLend.Api.Services;

public interface IHealthService
{
    Task<HealthResponse> CheckAsync(CancellationToken cancellationToken);
}
