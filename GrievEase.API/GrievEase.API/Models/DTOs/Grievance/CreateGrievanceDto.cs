using System.ComponentModel.DataAnnotations;
using GrievEase.API.Constants;

namespace GrievEase.API.Models.DTOs.Grievance;

public class CreateGrievanceDto
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Street is required")]
    [MaxLength(255, ErrorMessage = "Street cannot exceed 255 characters")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Locality is required")]
    [MaxLength(100, ErrorMessage = "Locality cannot exceed 100 characters")]
    public string Locality { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required")]
    [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required")]
    [MaxLength(50, ErrorMessage = "Department cannot exceed 50 characters")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string PhoneNumber { get; set; } = string.Empty;

    // Sent by frontend after uploading to Cloudinary
    [Required(ErrorMessage = "Image URL is required")]
    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    public string ImageUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Image Public ID is required")]
    [MaxLength(255, ErrorMessage = "Image Public ID cannot exceed 255 characters")]
    public string ImagePublicId { get; set; } = string.Empty;
}