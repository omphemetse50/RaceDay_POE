using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Models;

public class ProfileViewModel
{
    [Required, MaxLength(100), Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    [Display(Name = "New profile picture")]
    public IFormFile? ProfilePicture { get; set; }
}

public class MyResultsViewModel
{
    public List<ApiResult> Results { get; set; } = new();
}
