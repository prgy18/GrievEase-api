using GrievEase.API.Models.DTOs.Auth;
using GrievEase.API.Models.DTOs.Common;
using GrievEase.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrievEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get current user's profile
    /// GET /api/user/profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetUserIdFromToken();
            var user = await _userService.GetUserProfileAsync(userId);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.FailureResponse(
                "An error occurred.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update user profile (name, phone, address)
    /// PUT /api/user/profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var updatedUser = await _userService.UpdateUserProfileAsync(
                userId,
                updateProfileDto.Name,
                updateProfileDto.PhoneNumber,
                updateProfileDto.Address);

            return Ok(ApiResponse<UserDto>.SuccessResponse(
                updatedUser,
                "Profile updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<UserDto>.FailureResponse(
                "An error occurred while updating profile.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Deactivate user account (soft delete)
    /// PUT /api/user/deactivate
    /// </summary>
    [HttpPut("deactivate")]
    public async Task<IActionResult> DeactivateAccount()
    {
        try
        {
            var userId = GetUserIdFromToken();
            await _userService.DeactivateAccountAsync(userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                "Account deactivated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse(
                "An error occurred while deactivating account.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Reactivate previously deactivated account
    /// PUT /api/user/reactivate
    /// </summary>
    [HttpPut("reactivate")]
    public async Task<IActionResult> ReactivateAccount()
    {
        try
        {
            var userId = GetUserIdFromToken();
            await _userService.ReactivateAccountAsync(userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                "Account reactivated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse(
                "An error occurred while reactivating account.",
                new List<string> { ex.Message }));
        }
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("Invalid token.");
        }
        return Guid.Parse(userIdClaim);
    }
}

// DTO for updating profile
public class UpdateProfileDto
{
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}