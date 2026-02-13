using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using GrievEase.API.Constants;

namespace GrievEase.API.Models.Entities;

[Index(nameof(UserId))]
[Index(nameof(Status))]
[Index(nameof(Department))]
public class Grievance
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Locality { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ImagePublicId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? SolvedImageUrl { get; set; }

    [MaxLength(255)]
    public string? SolvedImagePublicId { get; set; }

    public int Upvotes { get; set; } = 0;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = GrievanceStatus.Pending;

    [MaxLength(20)]
    public string Priority { get; set; } = "medium";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SolvedOn { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<GrievanceUpvote> GrievanceUpvotes { get; set; } = new List<GrievanceUpvote>();
}