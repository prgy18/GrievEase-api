using System.ComponentModel.DataAnnotations;

namespace GrievEase.API.Models.DTOs.Grievance
{
    public class RequestApprovalDto
    {
        [Required]
        public string SolvedImageUrl { get; set; } = string.Empty;

        public string? SolvedImagePublicId { get; set; }
    }
}
