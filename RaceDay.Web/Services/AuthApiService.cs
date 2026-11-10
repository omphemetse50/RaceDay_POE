using System.Net.Http.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services;

public class AuthApiService
{
    private readonly HttpClient _http;

    public AuthApiService(HttpClient http)
    {
        _http = http;
    }

    public record LoginResult(bool Success, ApiAuthResponse? Response, string? SessionCookie, string? Error);

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterViewModel model)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new
        {
            fullName = model.FullName,
            email = model.Email,
            password = model.Password,
            role = model.Role
        });

        if (response.IsSuccessStatusCode) return (true, null);

        var error = await TryReadErrorAsync(response);
        return (false, error);
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });

        if (!response.IsSuccessStatusCode)
        {
            var error = await TryReadErrorAsync(response);
            return new LoginResult(false, null, null, error ?? "Login failed.");
        }

        // Capture the raw "RaceDay.Session=xyz; path=/; ..." cookie the API just issued.
        // Only the name=value pair is forwarded on later calls, not the full attribute string.
        string? sessionCookie = null;
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
        {
            foreach (var raw in setCookieValues)
            {
                if (raw.StartsWith("RaceDay.Session=", StringComparison.OrdinalIgnoreCase))
                {
                    sessionCookie = raw.Split(';')[0]; // "RaceDay.Session=xyz"
                    break;
                }
            }
        }

        var body = await response.Content.ReadFromJsonAsync<ApiAuthResponse>();

        if (body is null || sessionCookie is null)
        {
            return new LoginResult(false, null, null, "Login succeeded but no session was established. Please try again.");
        }

        return new LoginResult(true, body, sessionCookie, null);
    }

    /// <summary>Calls the API's logout endpoint using the caller-supplied session cookie.</summary>
    public async Task LogoutAsync(string sessionCookie)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        request.Headers.TryAddWithoutValidation("Cookie", sessionCookie);
        using var response = await _http.SendAsync(request);
        // Best-effort: even if this fails (e.g. session already expired), the MVC app
        // still clears its own cookie in AccountController, which is what actually
        // matters for "ending the session" from the user's point of view.
    }

    private static async Task<string?> TryReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return error?.Message;
        }
        catch
        {
            return null;
        }
    }
}
