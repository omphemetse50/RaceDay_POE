namespace RaceDay.Web.Services;

/// <summary>Maps enrolment/result status strings to a consistent CSS class across every view.</summary>
public static class StatusBadgeHelper
{
    public static string CssClass(string? status) => status?.ToLowerInvariant() switch
    {
        "confirmed" => "status-confirmed",
        "pending" => "status-pending",
        "cancelled" => "status-cancelled",
        "finished" => "status-finished",
        _ => "status-default"
    };
}
