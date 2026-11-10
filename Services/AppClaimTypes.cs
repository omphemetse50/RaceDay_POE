namespace RaceDay.Web.Services;

public static class AppClaimTypes
{
    /// <summary>
    /// Holds the raw "RaceDay.Session=xyz" cookie value returned by the API's
    /// /api/auth/login response. Stored as a claim inside the MVC app's OWN
    /// authentication cookie (set via SignInAsync), so it survives across
    /// requests without any server-side session dictionary of our own.
    /// The browser never sees this value directly -- only the MVC server does,
    /// and only to attach it to its own outgoing calls to the API.
    /// </summary>
    public const string ApiSessionCookie = "racedayapi_session_cookie";
}
