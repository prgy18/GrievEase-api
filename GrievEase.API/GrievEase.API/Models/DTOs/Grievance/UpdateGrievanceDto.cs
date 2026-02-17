using System.ComponentModel.DataAnnotations;

namespace GrievEase.API.Models.DTOs.Grievance;

public class UpdateGrievanceDto
{
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string? Name { get; set; }

    [MaxLength(255, ErrorMessage = "Street cannot exceed 255 characters")]
    public string? Street { get; set; }

    [MaxLength(100, ErrorMessage = "Locality cannot exceed 100 characters")]
    public string? Locality { get; set; }

    [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }

    [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters")]
    public string? State { get; set; }

    [MaxLength(50, ErrorMessage = "Department cannot exceed 50 characters")]
    public string? Department { get; set; }

    [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }
}
