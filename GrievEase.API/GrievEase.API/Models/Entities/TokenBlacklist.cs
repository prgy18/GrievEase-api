
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;


namespace GrievEase.API.Models.Entities;

[Index(nameof(Token))]
[Index(nameof(UserId))]
public class TokenBlacklist
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(1000)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }
}
