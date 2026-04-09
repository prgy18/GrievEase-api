using System.ComponentModel.DataAnnotations;

namespace GrievEase.API.Models.DTOs.Grievance
{
    public class RejectResolutionDto
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}
