using System.Text.RegularExpressions;

namespace Game1.Services;

/// <summary>Turns a YouTube link into an embeddable player URL. Other hosts are not embedded (the iframe src comes from teacher input).</summary>
public static partial class VideoLinks
{
    [GeneratedRegex(@"^https?://(?:www\.|m\.)?(?:youtube\.com/(?:watch\?(?:.*&)?v=|embed/|shorts/)|youtu\.be/)([A-Za-z0-9_-]{11})")]
    private static partial Regex YouTube();

    public static string? Embed(string? url) =>
        url != null && YouTube().Match(url.Trim()) is { Success: true } m
            ? $"https://www.youtube-nocookie.com/embed/{m.Groups[1].Value}"
            : null;
}
