namespace RaceDay.Services;

/// <summary>
/// Abstraction over Azure Blob Storage. Nothing outside this service (and the two
/// controllers that call it) knows or cares that Azure is involved -- the MVC app
/// in Part 3 never talks to Azure directly, it only ever calls this API, which is
/// exactly what the Part 3 brief requires.
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a file to the given container (created if it doesn't exist) and
    /// returns its public URL.
    /// </summary>
    Task<string> UploadAsync(string containerName, string fileName, Stream content, string contentType);
}
