using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Models;

public class EventFormViewModel
{
    public int? EventId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, DataType(DataType.DateTime), Display(Name = "Event date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

    [Required, MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required, Range(0.1, 999.99), Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; }

    [Required, Display(Name = "Event type")]
    public string EventType { get; set; } = "Run";

    [Display(Name = "Banner image")]
    public IFormFile? BannerImage { get; set; }

    public string? ExistingBannerUrl { get; set; }
}

public class EventListFilterViewModel
{
    public List<ApiEvent> Events { get; set; } = new();
    public string? Location { get; set; }
    public DateTime? AfterDate { get; set; }
}

public class DashboardEventDto
{
    public ApiEvent Event { get; set; } = null!;
    public int EnrolmentCount { get; set; }
}

public class OrganiserDashboardViewModel
{
    public List<DashboardEventDto> MyEvents { get; set; } = new(); // <-- to DTO
    public int TotalEnrolments { get; set; }
    public DateTime? NextEventDate { get; set; }
}

public class CategoriesPageViewModel
{
    public ApiEvent Event { get; set; } = null!;
    public List<ApiCategory> Categories { get; set; } = new();
    public CategoryFormViewModel NewCategory { get; set; } = new();
}

public class CategoryFormViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int? CategoryId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Distance (km) -- leave blank for age-based categories")]
    public decimal? DistanceKm { get; set; }

    [Required, Range(1, 100000), Display(Name = "Max participants")]
    public int MaxParticipants { get; set; } = 100;
}

public class ResultFormViewModel
{
    public int EnrolmentId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    [Required, Display(Name = "Finish time (hh:mm:ss)")]
    public string FinishTime { get; set; } = string.Empty;

    [Required, Range(1, 100000)]
    public int Position { get; set; }

    public string Status { get; set; } = "Finished";
}
