using System.Net.Http.Headers;
using System.Text.Json;

namespace FileInfoViewer;

/// <summary>
/// Checks GitHub Releases for a newer version of the application.
/// Generic and reusable — just pass owner, repo and current version.
/// </summary>
internal static class GitHubUpdateChecker
{
    private static readonly HttpClient _http = new();

    static GitHubUpdateChecker()
    {
        // GitHub API requires a User-Agent header
        _http.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("GitHubUpdateChecker", "1.0"));
        _http.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>Result of an update check.</summary>
    public record UpdateCheckResult(
        bool   IsUpdateAvailable,
        string LatestVersion,
        string ReleasePageUrl);

    /// <summary>
    /// Asynchronously checks the latest GitHub release and compares it with
    /// <paramref name="currentVersion"/>. Returns null if the check could not
    /// be completed (network error, API error, etc.) so callers can fail silently.
    /// </summary>
    /// <param name="owner">GitHub username or organisation (e.g. "SweWolf")</param>
    /// <param name="repo">Repository name (e.g. "FileInfoViewer")</param>
    /// <param name="currentVersion">The version currently running</param>
    public static async Task<UpdateCheckResult?> CheckAsync(
        string owner, string repo, Version currentVersion)
    {
        try
        {
            string url  = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            string json = await _http.GetStringAsync(url);

            using var doc  = JsonDocument.Parse(json);
            var       root = doc.RootElement;

            string tagName    = root.GetProperty("tag_name").GetString() ?? string.Empty;
            string releaseUrl = root.GetProperty("html_url").GetString() ?? string.Empty;

            // Strip leading 'v' or 'V' from tag (e.g. "v1.2.0" → "1.2.0")
            string versionStr = tagName.TrimStart('v', 'V');

            if (Version.TryParse(versionStr, out var latestVersion)
                && latestVersion > currentVersion)
            {
                return new UpdateCheckResult(true, tagName, releaseUrl);
            }

            return new UpdateCheckResult(false, tagName, releaseUrl);
        }
        catch
        {
            // Network unavailable, API rate-limited, JSON malformed, etc.
            // Fail silently — update checks should never crash the app.
            return null;
        }
    }
}
