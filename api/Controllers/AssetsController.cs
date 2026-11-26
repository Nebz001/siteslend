using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SitesLend.Api.Dtos;
using SitesLend.Api.Services;

namespace SitesLend.Api.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetsController(IAssetService assetService) : ControllerBase
{
    // TEMPORARILY ANONYMOUS: login does not exist yet. A11 adds login, and this endpoint
    // must then lose [AllowAnonymous] and get an authorization policy (deny by default).
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType<PagedResult<AssetUnitDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<AssetUnitDto>>> Get(
        [FromQuery, Range(1, 1_000_000)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await assetService.GetPageAsync(page, pageSize, cancellationToken));
}
