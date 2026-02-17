using System.ComponentModel.DataAnnotations;
using GrievEase.API.Constants;

namespace GrievEase.API.Models.DTOs.Grievance;

public class UpdateStatusDto
{
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;

    // Only required when status = "solved"
    // Frontend uploads solved image to Cloudinary and sends URLs
    [MaxLength(500)]
    public string? SolvedImageUrl { get; set; }

    [MaxLength(255)]
    public string? SolvedImagePublicId { get; set; }
}