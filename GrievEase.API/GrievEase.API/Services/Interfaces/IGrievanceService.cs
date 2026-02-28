using GrievEase.API.Models.DTOs.Common;
using GrievEase.API.Models.DTOs.Grievance;

namespace GrievEase.API.Services.Interfaces;

public interface IGrievanceService
{
    // ==================== CRUD OPERATIONS ====================

    /// <summary>
    /// Create a new grievance
    /// </summary>
    /// <param name="userId">User creating the grievance</param>
    /// <param name="createDto">Grievance details</param>
    /// <returns>Created grievance with details</returns>
    Task<GrievanceResponseDto> CreateGrievanceAsync(Guid userId, CreateGrievanceDto createDto);

    /// <summary>
    /// Get all grievances with pagination and optional filters
    /// </summary>
    /// <param name="currentUserId">Current logged-in user (to check if they upvoted)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10)</param>
    /// <param name="department">Filter by department (optional)</param>
    /// <param name="status">Filter by status (optional)</param>
    /// <param name="locality">Filter by locality (optional)</param>
    /// <param name="sortBy">Sort by: 'recent', 'upvotes', 'oldest' (default: 'recent')</param>
    /// <returns>Paginated list of grievances</returns>
    Task<PaginatedResponse<GrievanceResponseDto>> GetAllGrievancesAsync(
        Guid currentUserId,
        int pageNumber = 1,
        int pageSize = 10,
        string? department = null,
        string? status = null,
        string? locality = null,
        string sortBy = "recent");

    /// <summary>
    /// Get single grievance by ID
    /// </summary>
    /// <param name="grievanceId">Grievance ID</param>
    /// <param name="currentUserId">Current logged-in user</param>
    /// <returns>Grievance details</returns>
    /// <exception cref="KeyNotFoundException">If grievance not found</exception>
    Task<GrievanceResponseDto> GetGrievanceByIdAsync(Guid grievanceId, Guid currentUserId);

    /// <summary>
    /// Get all grievances created by a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="currentUserId">Current logged-in user</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated list of user's grievances</returns>
    Task<PaginatedResponse<GrievanceResponseDto>> GetMyGrievancesAsync(
        Guid userId,
        Guid currentUserId,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    /// Update grievance details (only by creator)
    /// Cannot update status - that's handled separately by UpdateStatusAsync
    /// </summary>
    /// <param name="grievanceId">Grievance ID</param>
    /// <param name="userId">User requesting update</param>
    /// <param name="updateDto">Fields to update</param>
    /// <returns>Updated grievance</returns>
    /// <exception cref="UnauthorizedAccessException">If user is not the creator</exception>
    /// <exception cref="InvalidOperationException">If grievance is already solved</exception>
    Task<GrievanceResponseDto> UpdateGrievanceAsync(
        Guid grievanceId,
        Guid userId,
        UpdateGrievanceDto updateDto);

    /// <summary>
    /// Delete grievance (only by creator, only if status is 'pending')
    /// </summary>
    /// <param name="grievanceId">Grievance ID</param>
    /// <param name="userId">User requesting deletion</param>
    /// <exception cref="UnauthorizedAccessException">If user is not the creator</exception>
    /// <exception cref="InvalidOperationException">If grievance is not pending</exception>
    Task DeleteGrievanceAsync(Guid grievanceId, Guid userId);

    // ==================== UPVOTE SYSTEM ====================

    /// <summary>
    /// Upvote a grievance (toggle behavior: upvote if not upvoted, remove upvote if already upvoted)
    /// </summary>
    /// <param name="grievanceId">Grievance ID</param>
    /// <param name="userId">User upvoting</param>
    /// <returns>Updated grievance with new upvote count</returns>
    Task<GrievanceResponseDto> ToggleUpvoteAsync(Guid grievanceId, Guid userId);

    // ==================== GOVERNMENT OFFICIAL ACTIONS ====================

    /// <summary>
    /// Update grievance status (pending → in process → solved)
    /// Only Government Officials can do this
    /// </summary>
    /// <param name="grievanceId">Grievance ID</param>
    /// <param name="userId">Government official ID</param>
    /// <param name="updateStatusDto">New status and optional solved image</param>
    /// <returns>Updated grievance</returns>
    /// <exception cref="UnauthorizedAccessException">If user is not Government Official</exception>
    /// <exception cref="InvalidOperationException">If status transition is invalid or grievance already solved</exception>
    Task<GrievanceResponseDto> UpdateGrievanceStatusAsync(
        Guid grievanceId,
        Guid userId,
        UpdateStatusDto updateStatusDto);

    // ==================== SEARCH & FILTER ====================

    /// <summary>
    /// Search grievances by keyword (searches in description, locality, department)
    /// </summary>
    /// <param name="currentUserId">Current logged-in user</param>
    /// <param name="searchQuery">Search keyword</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated search results</returns>
    Task<PaginatedResponse<GrievanceResponseDto>> SearchGrievancesAsync(
        Guid currentUserId,
        string searchQuery,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    /// Get grievances by department
    /// </summary>
    /// <param name="currentUserId">Current logged-in user</param>
    /// <param name="department">Department name</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated list of grievances in that department</returns>
    Task<PaginatedResponse<GrievanceResponseDto>> GetGrievancesByDepartmentAsync(
        Guid currentUserId,
        string department,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    /// Get grievances by status (pending/in process/solved)
    /// Only Government Officials can filter by status
    /// </summary>
    /// <param name="currentUserId">Current logged-in user (must be Government Official)</param>
    /// <param name="status">Status to filter by</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated list of grievances with that status</returns>
    /// <exception cref="UnauthorizedAccessException">If user is not Government Official</exception>
    Task<PaginatedResponse<GrievanceResponseDto>> GetGrievancesByStatusAsync(
        Guid currentUserId,
        string status,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    /// Get all solved grievances (visible to everyone)
    /// </summary>
    /// <param name="currentUserId">Current logged-in user</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated list of solved grievances</returns>
    Task<PaginatedResponse<GrievanceResponseDto>> GetSolvedGrievancesAsync(
        Guid currentUserId,
        int pageNumber = 1,
        int pageSize = 10);

    // ==================== STATISTICS ====================

    /// <summary>
    /// Get comprehensive statistics (Government Officials only)
    /// Includes: total counts by status, department-wise breakdown, top localities, avg resolution time
    /// </summary>
    /// <param name="userId">User requesting stats (must be Government Official)</param>
    /// <returns>Statistics object</returns>
    /// <exception cref="UnauthorizedAccessException">If user is not Government Official</exception>
    Task<StatisticsDto> GetStatisticsAsync(Guid userId);
}