using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;

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
