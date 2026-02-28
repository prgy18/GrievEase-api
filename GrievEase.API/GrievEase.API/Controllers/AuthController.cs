using GrievEase.API.Helpers;
using GrievEase.API.Models.DTOs.Auth;
using GrievEase.API.Models.DTOs.Common;
using GrievEase.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrievEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly JwtHelper _jwtHelper;

    public AuthController(IAuthService authService, JwtHelper jwtHelper)
    {
        _authService = authService;
        _jwtHelper = jwtHelper;
    }

    /// <summary>
    /// Register a new user (Locality Member or Government Official)
    /// POST /api/auth/register
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            var result = await _authService.RegisterAsync(registerDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(
                result,
                "Registration successful. You are now logged in."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<AuthResponseDto>.FailureResponse(
                "An error occurred during registration.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Login existing user
    /// POST /api/auth/login
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _authService.LoginAsync(loginDto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(
                result,
                "Login successful."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<AuthResponseDto>.FailureResponse(
                "An error occurred during login.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get current authenticated user
    /// GET /api/auth/me
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = GetUserIdFromToken();
            var user = await _authService.GetCurrentUserAsync(userId);
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
    /// Logout user (blacklist current token)
    /// POST /api/auth/logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = GetUserIdFromToken();
            var token = GetTokenFromHeader();

            await _authService.LogoutAsync(token, userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                "Logout successful."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse(
                "An error occurred during logout.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Change user password (invalidates all existing tokens)
    /// PUT /api/auth/change-password
    /// </summary>
    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var newToken = await _authService.ChangePasswordAsync(userId, changePasswordDto);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { Token = newToken },
                "Password changed successfully. Please use the new token for future requests."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<object>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.FailureResponse(
                "An error occurred while changing password.",
                new List<string> { ex.Message }));
        }
    }

    // Helper methods to extract data from JWT token
    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException("Invalid token.");
        }
        return Guid.Parse(userIdClaim);
    }

    private string GetTokenFromHeader()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            throw new UnauthorizedAccessException("Token not found in request header.");
        }
        return authHeader.Substring("Bearer ".Length).Trim();
    }
}