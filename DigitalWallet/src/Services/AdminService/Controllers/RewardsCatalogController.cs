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
/// <summary>
/// Exposes admin endpoints for managing the rewards catalog items.
/// </summary>
public class RewardsCatalogController : ControllerBase
{
    private readonly IRewardsCatalogService _svc;
    /// <summary>
    /// Injects the rewards catalog service used for create, update, and delete operations.
    /// </summary>
    public RewardsCatalogController(IRewardsCatalogService svc) => _svc = svc;

    /// <summary>
    /// Extracts and parses the authenticated admin's user ID from the current HTTP context claims.
    /// </summary>
    private Guid GetAdminId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(raw, out var id) ? id : throw new UnauthorizedAccessException("User identity is invalid.");
    }

    /// <summary>
    /// Creates a new rewards catalog item and returns the created item with a 201 status.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCatalogItemRequest request)
    {
        var result = await _svc.CreateAsync(GetAdminId(), request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CatalogItemAdminDto>.Ok(result, "Catalog item created."));
    }

    /// <summary>
    /// Updates an existing catalog item's details by ID and returns the updated item.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCatalogItemRequest request)
    {
        var result = await _svc.UpdateAsync(GetAdminId(), id, request);
        return Ok(ApiResponse<CatalogItemAdminDto>.Ok(result, "Catalog item updated."));
    }

    /// <summary>
    /// Soft-deletes a catalog item by ID, marking it unavailable, and returns a confirmation response.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _svc.DeleteAsync(GetAdminId(), id);
        return Ok(ApiResponse<object>.Ok(null, "Catalog item deleted."));
    }
}
