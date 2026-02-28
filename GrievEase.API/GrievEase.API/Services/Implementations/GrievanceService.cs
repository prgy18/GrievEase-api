using GrievEase.API.Constants;
using GrievEase.API.Data;
using GrievEase.API.Models.DTOs.Common;
using GrievEase.API.Models.DTOs.Grievance;
using GrievEase.API.Models.Entities;
using GrievEase.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrievEase.API.Services.Implementations;

/// <summary>
/// Implementation of grievance management service
/// Handles all grievance CRUD, upvotes, search, and statistics
/// </summary>
public class GrievanceService : IGrievanceService
{
    private readonly ApplicationDbContext _context;

    public GrievanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==================== CRUD OPERATIONS ====================

    /// <summary>
    /// Create a new grievance
    /// </summary>
    public async Task<GrievanceResponseDto> CreateGrievanceAsync(Guid userId, CreateGrievanceDto createDto)
    {
        // Validate department
        if (!Departments.IsValid(createDto.Department))
        {
            throw new ArgumentException(
                $"Invalid department. Valid departments: {string.Join(", ", Departments.GetAllDepartments())}");
        }

        // Create grievance entity
        var grievance = new Grievance
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = createDto.Name,
            Street = createDto.Street,
            Locality = createDto.Locality,
            City = createDto.City,
            State = createDto.State,
            Department = createDto.Department,
            Description = createDto.Description,
            PhoneNumber = createDto.PhoneNumber,
            ImageUrl = createDto.ImageUrl,
            ImagePublicId = createDto.ImagePublicId,
            Status = GrievanceStatus.Pending,
            Priority = "medium",
            Upvotes = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Grievances.Add(grievance);
        await _context.SaveChangesAsync();

        // Load user info for response
        await _context.Entry(grievance).Reference(g => g.User).LoadAsync();

        return await MapToGrievanceResponseDto(grievance, userId);
    }

    /// <summary>
    /// Get all grievances with pagination and filters
    /// </summary>
    public async Task<PaginatedResponse<GrievanceResponseDto>> GetAllGrievancesAsync(
        Guid currentUserId,
        int pageNumber = 1,
        int pageSize = 10,
        string? department = null,
        string? status = null,
        string? locality = null,
        string sortBy = "recent")
    {
        // Start with base query
        var query = _context.Grievances
            .Include(g => g.User)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(g => g.Department.ToLower() == department.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(g => g.Status.ToLower() == status.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(locality))
        {
            query = query.Where(g => g.Locality.ToLower().Contains(locality.ToLower()));
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "upvotes" => query.OrderByDescending(g => g.Upvotes),
            "oldest" => query.OrderBy(g => g.CreatedAt),
            _ => query.OrderByDescending(g => g.CreatedAt) // "recent" is default
        };

        // Get total count for pagination
        var totalRecords = await query.CountAsync();

        // Apply pagination
        var grievances = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map to DTOs
        var grievanceDtos = new List<GrievanceResponseDto>();
        foreach (var grievance in grievances)
        {
            grievanceDtos.Add(await MapToGrievanceResponseDto(grievance, currentUserId));
        }

        return PaginatedResponse<GrievanceResponseDto>.Create(
            grievanceDtos,
            pageNumber,
            pageSize,
            totalRecords);
    }

    /// <summary>
    /// Get single grievance by ID
    /// </summary>
    public async Task<GrievanceResponseDto> GetGrievanceByIdAsync(Guid grievanceId, Guid currentUserId)
    {
        var grievance = await _context.Grievances
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.Id == grievanceId);

        if (grievance == null)
        {
            throw new KeyNotFoundException("Grievance not found.");
        }

        return await MapToGrievanceResponseDto(grievance, currentUserId);
    }

    /// <summary>
    /// Get all grievances created by a specific user
    /// </summary>
    public async Task<PaginatedResponse<GrievanceResponseDto>> GetMyGrievancesAsync(
        Guid userId,
        Guid currentUserId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.Grievances
            .Include(g => g.User)
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt);

        var totalRecords = await query.CountAsync();

        var grievances = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var grievanceDtos = new List<GrievanceResponseDto>();
        foreach (var grievance in grievances)
        {
            grievanceDtos.Add(await MapToGrievanceResponseDto(grievance, currentUserId));
        }

        return PaginatedResponse<GrievanceResponseDto>.Create(
            grievanceDtos,
            pageNumber,
            pageSize,
            totalRecords);
    }

    /// <summary>
    /// Update grievance details (only by creator, cannot update status)
    /// </summary>
    public async Task<GrievanceResponseDto> UpdateGrievanceAsync(
        Guid grievanceId,
        Guid userId,
        UpdateGrievanceDto updateDto)
    {
        var grievance = await _context.Grievances
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.Id == grievanceId);

        if (grievance == null)
        {
            throw new KeyNotFoundException("Grievance not found.");
        }

        // Check if user is the creator
        if (grievance.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own grievances.");
        }

        // Cannot update if already solved
        if (grievance.Status == GrievanceStatus.Solved)
        {
            throw new InvalidOperationException("Cannot update a solved grievance.");
        }

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(updateDto.Name))
            grievance.Name = updateDto.Name;

        if (!string.IsNullOrWhiteSpace(updateDto.Street))
            grievance.Street = updateDto.Street;

        if (!string.IsNullOrWhiteSpace(updateDto.Locality))
            grievance.Locality = updateDto.Locality;

        if (!string.IsNullOrWhiteSpace(updateDto.City))
            grievance.City = updateDto.City;

        if (!string.IsNullOrWhiteSpace(updateDto.State))
            grievance.State = updateDto.State;

        if (!string.IsNullOrWhiteSpace(updateDto.Department))
        {
            if (!Departments.IsValid(updateDto.Department))
            {
                throw new ArgumentException(
                    $"Invalid department. Valid departments: {string.Join(", ", Departments.GetAllDepartments())}");
            }
            grievance.Department = updateDto.Department;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.Description))
            grievance.Description = updateDto.Description;

        if (!string.IsNullOrWhiteSpace(updateDto.PhoneNumber))
            grievance.PhoneNumber = updateDto.PhoneNumber;

        grievance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToGrievanceResponseDto(grievance, userId);
    }

    /// <summary>
    /// Delete grievance (only by creator, only if pending)
    /// </summary>
    public async Task DeleteGrievanceAsync(Guid grievanceId, Guid userId)
    {
        var grievance = await _context.Grievances
            .FirstOrDefaultAsync(g => g.Id == grievanceId);

        if (grievance == null)
        {
            throw new KeyNotFoundException("Grievance not found.");
        }

        // Check if user is the creator
        if (grievance.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own grievances.");
        }

        // Can only delete if status is pending
        if (grievance.Status != GrievanceStatus.Pending)
        {
            throw new InvalidOperationException(
                "Cannot delete grievance. Only pending grievances can be deleted.");
        }

        _context.Grievances.Remove(grievance);
        await _context.SaveChangesAsync();
    }

    // ==================== UPVOTE SYSTEM ====================

    /// <summary>
    /// Toggle upvote on a grievance
    /// If not upvoted: add upvote
    /// If already upvoted: remove upvote
    /// </summary>
    public async Task<GrievanceResponseDto> ToggleUpvoteAsync(Guid grievanceId, Guid userId)
    {
        var grievance = await _context.Grievances
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.Id == grievanceId);

        if (grievance == null)
        {
            throw new KeyNotFoundException("Grievance not found.");
        }

        // Check if user already upvoted
        var existingUpvote = await _context.GrievanceUpvotes
            .FirstOrDefaultAsync(gu => gu.GrievanceId == grievanceId && gu.UserId == userId);

        if (existingUpvote != null)
        {
            // Remove upvote
            _context.GrievanceUpvotes.Remove(existingUpvote);
            grievance.Upvotes--;
        }
        else
        {
            // Add upvote
            var upvote = new GrievanceUpvote
            {
                Id = Guid.NewGuid(),
                GrievanceId = grievanceId,
                UserId = userId,
                UpvotedAt = DateTime.UtcNow
            };

            _context.GrievanceUpvotes.Add(upvote);
            grievance.Upvotes++;
        }

        grievance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToGrievanceResponseDto(grievance, userId);
    }

    // ==================== GOVERNMENT OFFICIAL ACTIONS ====================

    /// <summary>
    /// Update grievance status (pending → in process → solved)
    /// Only Government Officials can call this
    /// </summary>
    public async Task<GrievanceResponseDto> UpdateGrievanceStatusAsync(
        Guid grievanceId,
        Guid userId,
        UpdateStatusDto updateStatusDto)
    {
        // Check if user is Government Official
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.SignInType != SignInType.GovernmentOfficial)
        {
            throw new UnauthorizedAccessException("Only Government Officials can update grievance status.");
        }

        // Validate status
        if (!GrievanceStatus.IsValid(updateStatusDto.Status))
        {
            throw new ArgumentException(
                $"Invalid status. Valid statuses: {string.Join(", ", GrievanceStatus.GetAllStatuses())}");
        }

        var grievance = await _context.Grievances
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.Id == grievanceId);

        if (grievance == null)
        {
            throw new KeyNotFoundException("Grievance not found.");
        }

        // Cannot change status of already solved grievance
        if (grievance.Status == GrievanceStatus.Solved)
        {
            throw new InvalidOperationException("Cannot change status of a solved grievance.");
        }

        // Update status
        grievance.Status = updateStatusDto.Status;

        // If status is "solved", set solved timestamp and image
        if (updateStatusDto.Status == GrievanceStatus.Solved)
        {
            grievance.SolvedOn = DateTime.UtcNow;

            // Solved image is optional but recommended
            if (!string.IsNullOrWhiteSpace(updateStatusDto.SolvedImageUrl))
            {
                grievance.SolvedImageUrl = updateStatusDto.SolvedImageUrl;
                grievance.SolvedImagePublicId = updateStatusDto.SolvedImagePublicId;
            }
        }

        grievance.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToGrievanceResponseDto(grievance, userId);
    }

    // ==================== SEARCH & FILTER ====================

    /// <summary>
    /// Search grievances by keyword (description, locality, department)
    /// </summary>
    public async Task<PaginatedResponse<GrievanceResponseDto>> SearchGrievancesAsync(
        Guid currentUserId,
        string searchQuery,
        int pageNumber = 1,
        int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return await GetAllGrievancesAsync(currentUserId, pageNumber, pageSize);
        }

        var query = _context.Grievances
            .Include(g => g.User)
            .Where(g =>
                g.Description.Contains(searchQuery) ||
                g.Locality.Contains(searchQuery) ||
                g.Department.Contains(searchQuery))
            .OrderByDescending(g => g.CreatedAt);

        var totalRecords = await query.CountAsync();

        var grievances = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var grievanceDtos = new List<GrievanceResponseDto>();
        foreach (var grievance in grievances)
        {
            grievanceDtos.Add(await MapToGrievanceResponseDto(grievance, currentUserId));
        }

        return PaginatedResponse<GrievanceResponseDto>.Create(
            grievanceDtos,
            pageNumber,
            pageSize,
            totalRecords);
    }

    /// <summary>
    /// Get grievances by department
    /// </summary>
    public async Task<PaginatedResponse<GrievanceResponseDto>> GetGrievancesByDepartmentAsync(
        Guid currentUserId,
        string department,
        int pageNumber = 1,
        int pageSize = 10)
    {
        if (!Departments.IsValid(department))
        {
            throw new ArgumentException(
                $"Invalid department. Valid departments: {string.Join(", ", Departments.GetAllDepartments())}");
        }

        return await GetAllGrievancesAsync(
            currentUserId,
            pageNumber,
            pageSize,
            department: department);
    }

    /// <summary>
    /// Get grievances by status (only Government Officials)
    /// </summary>
    public async Task<PaginatedResponse<GrievanceResponseDto>> GetGrievancesByStatusAsync(
        Guid currentUserId,
        string status,
        int pageNumber = 1,
        int pageSize = 10)
    {
        // Check if user is Government Official
        var user = await _context.Users.FindAsync(currentUserId);
        if (user == null || user.SignInType != SignInType.GovernmentOfficial)
        {
            throw new UnauthorizedAccessException("Only Government Officials can filter by status.");
        }

        if (!GrievanceStatus.IsValid(status))
        {
            throw new ArgumentException(
                $"Invalid status. Valid statuses: {string.Join(", ", GrievanceStatus.GetAllStatuses())}");
        }

        return await GetAllGrievancesAsync(
            currentUserId,
            pageNumber,
            pageSize,
            status: status);
    }

    /// <summary>
    /// Get all solved grievances (visible to everyone)
    /// </summary>
    public async Task<PaginatedResponse<GrievanceResponseDto>> GetSolvedGrievancesAsync(
        Guid currentUserId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await GetAllGrievancesAsync(
            currentUserId,
            pageNumber,
            pageSize,
            status: GrievanceStatus.Solved);
    }

    // ==================== STATISTICS ====================

    /// <summary>
    /// Get comprehensive statistics (Government Officials only)
    /// </summary>
    public async Task<StatisticsDto> GetStatisticsAsync(Guid userId)
    {
        // Check if user is Government Official
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.SignInType != SignInType.GovernmentOfficial)
        {
            throw new UnauthorizedAccessException("Only Government Officials can view statistics.");
        }

        // Overall counts
        var totalGrievances = await _context.Grievances.CountAsync();
        var pendingCount = await _context.Grievances.CountAsync(g => g.Status == GrievanceStatus.Pending);
        var inProcessCount = await _context.Grievances.CountAsync(g => g.Status == GrievanceStatus.InProcess);
        var solvedCount = await _context.Grievances.CountAsync(g => g.Status == GrievanceStatus.Solved);

        // Average resolution time (in days)
        var solvedGrievances = await _context.Grievances
            .Where(g => g.Status == GrievanceStatus.Solved && g.SolvedOn.HasValue)
            .Select(g => new { g.CreatedAt, g.SolvedOn })
            .ToListAsync();

        double averageResolutionDays = 0;
        if (solvedGrievances.Any())
        {
            var resolutionTimes = solvedGrievances
                .Select(g => (g.SolvedOn!.Value - g.CreatedAt).TotalDays);
            averageResolutionDays = resolutionTimes.Average();
        }

        // Department-wise statistics
        var departmentStats = await _context.Grievances
            .GroupBy(g => g.Department)
            .Select(group => new DepartmentStats
            {
                Department = group.Key,
                Total = group.Count(),
                Pending = group.Count(g => g.Status == GrievanceStatus.Pending),
                InProcess = group.Count(g => g.Status == GrievanceStatus.InProcess),
                Solved = group.Count(g => g.Status == GrievanceStatus.Solved)
            })
            .ToListAsync();

        // Top 10 localities with most grievances
        var topLocalities = await _context.Grievances
            .GroupBy(g => g.Locality)
            .Select(group => new LocalityStats
            {
                Locality = group.Key,
                TotalGrievances = group.Count(),
                SolvedGrievances = group.Count(g => g.Status == GrievanceStatus.Solved)
            })
            .OrderByDescending(l => l.TotalGrievances)
            .Take(10)
            .ToListAsync();

        return new StatisticsDto
        {
            TotalGrievances = totalGrievances,
            PendingGrievances = pendingCount,
            InProcessGrievances = inProcessCount,
            SolvedGrievances = solvedCount,
            AverageResolutionDays = Math.Round(averageResolutionDays, 2),
            DepartmentWiseStats = departmentStats,
            TopLocalities = topLocalities
        };
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Map Grievance entity to GrievanceResponseDto
    /// Includes HasUpvoted check for current user
    /// </summary>
    private async Task<GrievanceResponseDto> MapToGrievanceResponseDto(Grievance grievance, Guid currentUserId)
    {
        // Check if current user has upvoted this grievance
        var hasUpvoted = await _context.GrievanceUpvotes
            .AnyAsync(gu => gu.GrievanceId == grievance.Id && gu.UserId == currentUserId);

        return new GrievanceResponseDto
        {
            Id = grievance.Id,
            UserId = grievance.UserId,
            UserName = grievance.User.Name,
            Name = grievance.Name,
            Street = grievance.Street,
            Locality = grievance.Locality,
            City = grievance.City,
            State = grievance.State,
            Department = grievance.Department,
            Description = grievance.Description,
            PhoneNumber = grievance.PhoneNumber,
            ImageUrl = grievance.ImageUrl,
            ImagePublicId = grievance.ImagePublicId,
            SolvedImageUrl = grievance.SolvedImageUrl,
            SolvedImagePublicId = grievance.SolvedImagePublicId,
            Upvotes = grievance.Upvotes,
            Status = grievance.Status,
            Priority = grievance.Priority,
            HasUpvoted = hasUpvoted,
            CreatedAt = grievance.CreatedAt,
            UpdatedAt = grievance.UpdatedAt,
            SolvedOn = grievance.SolvedOn
        };
    }
}