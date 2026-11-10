using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services;

/// <summary>
/// All RaceDay data operations the MVC app needs, after the user is already logged
/// in (the session cookie is attached automatically by ApiSessionCookieHandler).
/// This is the ONLY place the MVC app talks to the API for data -- per the Part 3
/// brief, "The MVC project must not contain any direct database access or
/// replicated business logic."
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // ---------------- Events ----------------

    public async Task<ApiCallResult<List<ApiEvent>>> GetEventsAsync(string? location = null, DateTime? afterDate = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location)}");
        if (afterDate.HasValue) query.Add($"afterDate={afterDate.Value:O}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";

        return await GetAsync<List<ApiEvent>>($"api/events{qs}");
    }

    public Task<ApiCallResult<ApiEvent>> GetEventAsync(int id) => GetAsync<ApiEvent>($"api/events/{id}");

    public Task<ApiCallResult<ApiEvent>> CreateEventAsync(EventFormViewModel model) =>
        PostAsync<ApiEvent>("api/events", new
        {
            name = model.Name,
            description = model.Description,
            eventDate = model.EventDate,
            location = model.Location,
            distanceKm = model.DistanceKm,
            eventType = model.EventType
        });

    public Task<ApiCallResult<ApiEvent>> UpdateEventAsync(int id, EventFormViewModel model) =>
        PutAsync<ApiEvent>($"api/events/{id}", new
        {
            name = model.Name,
            description = model.Description,
            eventDate = model.EventDate,
            location = model.Location,
            distanceKm = model.DistanceKm,
            eventType = model.EventType
        });

    public Task<ApiCallResult<object>> DeleteEventAsync(int id) => DeleteAsync($"api/events/{id}");

    public async Task<ApiCallResult<string>> UploadEventBannerAsync(int eventId, IFormFile file)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        var response = await _http.PostAsync($"api/events/{eventId}/banner", content);
        if (!response.IsSuccessStatusCode) return ApiCallResult<string>.Fail(await ReadErrorAsync(response), (int)response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return ApiCallResult<string>.Ok(body?.GetValueOrDefault("bannerImageUrl") ?? string.Empty);
    }

    public Task<ApiCallResult<List<ApiEnrolment>>> GetEventEnrolmentsAsync(int eventId) =>
        GetAsync<List<ApiEnrolment>>($"api/events/{eventId}/enrolments");

    // ---------------- Categories ----------------

    public Task<ApiCallResult<List<ApiCategory>>> GetCategoriesAsync(int eventId) =>
        GetAsync<List<ApiCategory>>($"api/events/{eventId}/categories");

    public Task<ApiCallResult<ApiCategory>> CreateCategoryAsync(int eventId, CategoryFormViewModel model) =>
        PostAsync<ApiCategory>($"api/events/{eventId}/categories", new
        {
            name = model.Name,
            distanceKm = model.DistanceKm,
            maxParticipants = model.MaxParticipants
        });

    public Task<ApiCallResult<ApiCategory>> UpdateCategoryAsync(int categoryId, CategoryFormViewModel model) =>
        PutAsync<ApiCategory>($"api/categories/{categoryId}", new
        {
            name = model.Name,
            distanceKm = model.DistanceKm,
            maxParticipants = model.MaxParticipants
        });

    public Task<ApiCallResult<object>> DeleteCategoryAsync(int categoryId) => DeleteAsync($"api/categories/{categoryId}");

    // ---------------- Enrolments ----------------

    public Task<ApiCallResult<ApiEnrolment>> EnrolAsync(int categoryId) =>
        PostAsync<ApiEnrolment>($"api/categories/{categoryId}/enrolments", new { });

    public Task<ApiCallResult<List<ApiEnrolment>>> GetMyEnrolmentsAsync() =>
        GetAsync<List<ApiEnrolment>>("api/users/me/enrolments");

    public Task<ApiCallResult<object>> CancelEnrolmentAsync(int enrolmentId) => DeleteAsync($"api/enrolments/{enrolmentId}");

    // ---------------- Results ----------------

    public Task<ApiCallResult<ApiResult>> CaptureResultAsync(int enrolmentId, ResultFormViewModel model) =>
        PostAsync<ApiResult>($"api/enrolments/{enrolmentId}/result", new
        {
            finishTime = model.FinishTime,
            position = model.Position,
            status = model.Status
        });

    public Task<ApiCallResult<List<ApiResult>>> GetMyResultsAsync() => GetAsync<List<ApiResult>>("api/users/me/results");

    public Task<ApiCallResult<ApiResult>> GetResultAsync(int enrolmentId) => GetAsync<ApiResult>($"api/enrolments/{enrolmentId}/result");

    // ---------------- Profile ----------------

    public Task<ApiCallResult<ApiUserProfile>> GetMyProfileAsync() => GetAsync<ApiUserProfile>("api/users/me");

    public Task<ApiCallResult<ApiUserProfile>> UpdateMyProfileAsync(string fullName, string email) =>
        PutAsync<ApiUserProfile>("api/users/me", new { fullName, email });

    public async Task<ApiCallResult<string>> UploadProfilePictureAsync(IFormFile file)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        var response = await _http.PostAsync("api/users/me/profile-picture", content);
        if (!response.IsSuccessStatusCode) return ApiCallResult<string>.Fail(await ReadErrorAsync(response), (int)response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        return ApiCallResult<string>.Ok(body?.GetValueOrDefault("profilePictureUrl") ?? string.Empty);
    }

    // ---------------- Generic helpers ----------------

    private async Task<ApiCallResult<T>> GetAsync<T>(string url)
    {
        try
        {
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return ApiCallResult<T>.Fail(await ReadErrorAsync(response), (int)response.StatusCode);
            var data = await response.Content.ReadFromJsonAsync<T>();
            return ApiCallResult<T>.Ok(data!, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ApiCallResult<T>.Fail("Could not reach the RaceDay API. Please try again shortly.");
        }
    }

    private async Task<ApiCallResult<T>> PostAsync<T>(string url, object body)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(url, body);
            if (!response.IsSuccessStatusCode) return ApiCallResult<T>.Fail(await ReadErrorAsync(response), (int)response.StatusCode);
            var data = await response.Content.ReadFromJsonAsync<T>();
            return ApiCallResult<T>.Ok(data!, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ApiCallResult<T>.Fail("Could not reach the RaceDay API. Please try again shortly.");
        }
    }

    private async Task<ApiCallResult<T>> PutAsync<T>(string url, object body)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(url, body);
            if (!response.IsSuccessStatusCode) return ApiCallResult<T>.Fail(await ReadErrorAsync(response), (int)response.StatusCode);
            var data = await response.Content.ReadFromJsonAsync<T>();
            return ApiCallResult<T>.Ok(data!, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ApiCallResult<T>.Fail("Could not reach the RaceDay API. Please try again shortly.");
        }
    }

    private async Task<ApiCallResult<object>> DeleteAsync(string url)
    {
        try
        {
            var response = await _http.DeleteAsync(url);
            if (!response.IsSuccessStatusCode) return ApiCallResult<object>.Fail(await ReadErrorAsync(response), (int)response.StatusCode);
            return ApiCallResult<object>.Ok(new { }, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return ApiCallResult<object>.Fail("Could not reach the RaceDay API. Please try again shortly.");
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            if (!string.IsNullOrWhiteSpace(error?.Message)) return error.Message;
        }
        catch
        {
            // fall through to generic message below
        }

        return response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
            System.Net.HttpStatusCode.Forbidden => "You don't have permission to do that.",
            System.Net.HttpStatusCode.NotFound => "That item could not be found.",
            System.Net.HttpStatusCode.Conflict => "That action conflicts with existing data.",
            _ => "Something went wrong. Please try again."
        };
    }
}
