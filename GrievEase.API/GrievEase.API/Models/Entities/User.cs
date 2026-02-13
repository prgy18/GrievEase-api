using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using GrievEase.API.Constants;

namespace GrievEase.API.Models.Entities;

[Index(nameof(Email), IsUnique = true)]           // Index on Email (unique)
[Index(nameof(SignInType))]                        // Index on SignInType
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [MaxLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sign-in type is required")]
    public SignInType SignInType { get; set; } = SignInType.LocalityMember;

    public bool IsActive { get; set; } = true;

    public int TokenVersion { get; set; } = 0;

    public DateTime? LastLogin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public virtual ICollection<Grievance> Grievances { get; set; } = new List<Grievance>();
    public virtual ICollection<GrievanceUpvote> GrievanceUpvotes { get; set; } = new List<GrievanceUpvote>();
}