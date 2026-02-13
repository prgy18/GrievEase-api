using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GrievEase.API.Models.Entities;

[Index(nameof(GrievanceId), nameof(UserId), IsUnique = true)]
public class GrievanceUpvote
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey(nameof(Grievance))]
    public Guid GrievanceId { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public Guid UserId { get; set; }

    public DateTime UpvotedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual Grievance Grievance { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}