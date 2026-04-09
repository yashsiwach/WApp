using System.Security.Claims;
using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.DTOs;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/rewards/catalog")]
[Authorize(Roles = "Admin")]
public class RewardsCatalogController : ControllerBase
{
    private readonly IRewardsCatalogService _svc;
    public RewardsCatalogController(IRewardsCatalogService svc) => _svc = svc;

    private Guid GetAdminId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(raw, out var id) ? id : throw new UnauthorizedAccessException("User identity is invalid.");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCatalogItemRequest request)
    {
        var result = await _svc.CreateAsync(GetAdminId(), request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CatalogItemAdminDto>.Ok(result, "Catalog item created."));
    }
}
