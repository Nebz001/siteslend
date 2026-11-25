using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SitesLend.Api.Dtos;
using SitesLend.Api.Services;

namespace SitesLend.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(IHealthService healthService) : ControllerBase
{
    // Public on purpose: it reports only "ok" and "up"/"down", nothing sensitive.
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken cancellationToken)
        => Ok(await healthService.CheckAsync(cancellationToken));
}
