using System.Net;

namespace RaceDay.Web.Services;

/// <summary>
/// Every call this app makes to the RaceDay API (except Register/Login, which happen
/// before any session exists) goes through this handler. It reads the API session
/// cookie out of the current user's MVC authentication claims and resends it as a
/// Cookie header, so the API's [Authorize]/[Authorize(Roles = ...)] checks see the
/// same authenticated session that was established at login.
/// </summary>
public class ApiSessionCookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiSessionCookieHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cookieValue = _httpContextAccessor.HttpContext?.User?
            .FindFirst(AppClaimTypes.ApiSessionCookie)?.Value;

        if (!string.IsNullOrEmpty(cookieValue))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookieValue);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
