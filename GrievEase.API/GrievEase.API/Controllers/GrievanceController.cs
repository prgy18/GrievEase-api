using GrievEase.API.Models.DTOs.Common;
using GrievEase.API.Models.DTOs.Grievance;
using GrievEase.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GrievEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GrievanceController : ControllerBase
{
    private readonly IGrievanceService _grievanceService;

    public GrievanceController(IGrievanceService grievanceService)
    {
        _grievanceService = grievanceService;
    }

    // ==================== CRUD OPERATIONS ====================

    /// <summary>
    /// Get all grievances with pagination and filters
    /// GET /api/grievance?pageNumber=1&pageSize=10&department=Water-Works&sortBy=upvotes
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllGrievances(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? department = null,
        [FromQuery] string? status = null,
        [FromQuery] string? locality = null,
        [FromQuery] string sortBy = "recent")
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _grievanceService.GetAllGrievancesAsync(
                userId, pageNumber, pageSize, department, status, locality, sortBy);

            return Ok(ApiResponse<PaginatedResponse<GrievanceResponseDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaginatedResponse<GrievanceResponseDto>>.FailureResponse(
                "An error occurred while fetching grievances.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get single grievance by ID
    /// GET /api/grievance/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGrievanceById(Guid id)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var grievance = await _grievanceService.GetGrievanceByIdAsync(id, userId);
            return Ok(ApiResponse<GrievanceResponseDto>.SuccessResponse(grievance));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<GrievanceResponseDto>.FailureResponse(
                "An error occurred.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Create a new grievance
    /// POST /api/grievance
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateGrievance([FromBody] CreateGrievanceDto createDto)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var grievance = await _grievanceService.CreateGrievanceAsync(userId, createDto);
            return CreatedAtAction(
                nameof(GetGrievanceById),
                new { id = grievance.Id },
                ApiResponse<GrievanceResponseDto>.SuccessResponse(
                    grievance,
                    "Grievance created successfully."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<GrievanceResponseDto>.FailureResponse(
                "An error occurred while creating grievance.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update grievance details (only by creator)
    /// PUT /api/grievance/{id}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGrievance(Guid id, [FromBody] UpdateGrievanceDto updateDto)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var grievance = await _grievanceService.UpdateGrievanceAsync(id, userId, updateDto);
            return Ok(ApiResponse<GrievanceResponseDto>.SuccessResponse(
                grievance,
                "Grievance updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<GrievanceResponseDto>.FailureResponse(
                "An error occurred while updating grievance.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete grievance (only by creator, only if pending)
    /// DELETE /api/grievance/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGrievance(Guid id)
    {
        try
        {
            var userId = GetUserIdFromToken();
            await _grievanceService.DeleteGrievanceAsync(id, userId);
            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                "Grievance deleted successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.FailureResponse(
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
                "An error occurred while deleting grievance.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get current user's grievances
    /// GET /api/grievance/my-grievances
    /// </summary>
    [HttpGet("my-grievances")]
    public async Task<IActionResult> GetMyGrievances(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _grievanceService.GetMyGrievancesAsync(userId, userId, pageNumber, pageSize);
            return Ok(ApiResponse<PaginatedResponse<GrievanceResponseDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaginatedResponse<GrievanceResponseDto>>.FailureResponse(
                "An error occurred.",
                new List<string> { ex.Message }));
        }
    }

    // ==================== UPVOTE SYSTEM ====================

    /// <summary>
    /// Toggle upvote on a grievance
    /// PUT /api/grievance/{id}/upvote
    /// </summary>
    [HttpPut("{id}/upvote")]
    public async Task<IActionResult> ToggleUpvote(Guid id)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var grievance = await _grievanceService.ToggleUpvoteAsync(id, userId);
            return Ok(ApiResponse<GrievanceResponseDto>.SuccessResponse(
                grievance,
                "Upvote toggled successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<GrievanceResponseDto>.FailureResponse(
                "An error occurred while toggling upvote.",
                new List<string> { ex.Message }));
        }
    }

    // ==================== GOVERNMENT OFFICIAL ACTIONS ====================

    /// <summary>
    /// Update grievance status (Government Officials only)
    /// PUT /api/grievance/{id}/status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateGrievanceStatus(Guid id, [FromBody] UpdateStatusDto updateStatusDto)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var grievance = await _grievanceService.UpdateGrievanceStatusAsync(id, userId, updateStatusDto);
            return Ok(ApiResponse<GrievanceResponseDto>.SuccessResponse(
                grievance,
                "Grievance status updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<GrievanceResponseDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<GrievanceResponseDto>.FailureResponse(
                "An error occurred while updating status.",
                new List<string> { ex.Message }));
        }
    }

    // ==================== SEARCH & FILTER ====================

    /// <summary>
    /// Search grievances by keyword
    /// GET /api/grievance/search?query=pothole
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchGrievances(
        [FromQuery] string query,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _grievanceService.SearchGrievancesAsync(userId, query, pageNumber, pageSize);
            return Ok(ApiResponse<PaginatedResponse<GrievanceResponseDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaginatedResponse<GrievanceResponseDto>>.FailureResponse(
                "An error occurred during search.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get grievances by department
    /// GET /api/grievance/department/{dept}
    /// </summary>
    [HttpGet("department/{dept}")]
    public async Task<IActionResult> GetGrievancesByDepartment(
        string dept,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _grievanceService.GetGrievancesByDepartmentAsync(userId, dept, pageNumber, pageSize);
            return Ok(ApiResponse<PaginatedResponse<GrievanceResponseDto>>.SuccessResponse(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<GrievanceResponseDto>>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaginatedResponse<GrievanceResponseDto>>.FailureResponse(
                "An error occurred.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get grievances by status (Government Officials only)
    /// GET /api/grievance/status/{status}
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetGrievancesByStatus(
        string status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _grievanceService.GetGrievancesByStatusAsync(userId, status, pageNumber, pageSize);
            return Ok(ApiResponse<PaginatedResponse<GrievanceResponseDto>>.SuccessResponse(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<PaginatedResponse<GrievanceResponseDto>>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PaginatedResponse<GrievanceResponseDto>>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaginatedResponse<GrievanceResponseDto>>.FailureResponse(
                "An error occurred.",
                new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get all solved grievances
    /// GET /api/grievance/solved
    /// </summary>
    [HttpGet("solved")]
    public async Task<IActionResult> GetSolvedGrievances(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetUserIdFromToken();
            var result = await _grievanceService.GetSolvedGrievancesAsync(userId, pageNumber, pageSize);
            return Ok(ApiResponse<PaginatedResponse<GrievanceResponseDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PaginatedResponse<GrievanceResponseDto>>.FailureResponse(
                "An error occurred.",
                new List<string> { ex.Message }));
        }
    }

    // ==================== STATISTICS ====================

    /// <summary>
    /// Get comprehensive statistics (Government Officials only)
    /// GET /api/grievance/stats
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var userId = GetUserIdFromToken();
            var stats = await _grievanceService.GetStatisticsAsync(userId);
            return Ok(ApiResponse<StatisticsDto>.SuccessResponse(stats));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<StatisticsDto>.FailureResponse(
                ex.Message,
                new List<string> { ex.Message }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<StatisticsDto>.FailureResponse(
                "An error occurred while fetching statistics.",
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