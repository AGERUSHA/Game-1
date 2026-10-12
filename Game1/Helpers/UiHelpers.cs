using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Game1.Helpers;

public static class UiHelpers
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Round avatar: uploaded photo if any, otherwise initials on a gradient.</summary>
    public static IHtmlContent Avatar(this IHtmlHelper _, string initials, string? photoUrl, string css = "av-1")
    {
        var enc = System.Text.Encodings.Web.HtmlEncoder.Default;
        var inner = string.IsNullOrEmpty(photoUrl)
            ? enc.Encode(initials)
            : $"<img src=\"{enc.Encode(photoUrl)}\" alt=\"\" loading=\"lazy\" />";
        return new HtmlString($"<span class=\"avatar {enc.Encode(css)}\">{inner}</span>");
    }

    /// <summary>Lucide icon placeholder; replaced with inline SVG by lucide.createIcons().</summary>
    public static IHtmlContent Icon(this IHtmlHelper _, string name, string? css = null) =>
        new HtmlString($"<i data-lucide=\"{name}\" class=\"icon {css}\" aria-hidden=\"true\"></i>");

    /// <summary>Circular progress ring (SVG).</summary>
    public static IHtmlContent Ring(this IHtmlHelper _, int value, int size = 120, int stroke = 10, string? label = null, string? css = null)
    {
        var r = (size - stroke) / 2.0;
        var c = 2 * Math.PI * r;
        var offset = c * (1 - Math.Clamp(value, 0, 100) / 100.0);
        string F(double d) => d.ToString("0.##", CultureInfo.InvariantCulture);
        var text = label ?? $"{value}%";
        return new HtmlString($"""
            <div class="ring {css}" style="--size:{size}px" role="img" aria-label="{value}%">
              <svg viewBox="0 0 {size} {size}" width="{size}" height="{size}">
                <circle class="ring-track" cx="{F(size / 2.0)}" cy="{F(size / 2.0)}" r="{F(r)}" stroke-width="{stroke}"/>
                <circle class="ring-value" cx="{F(size / 2.0)}" cy="{F(size / 2.0)}" r="{F(r)}" stroke-width="{stroke}"
                        stroke-dasharray="{F(c)}" stroke-dashoffset="{F(c)}" data-offset="{F(offset)}"/>
              </svg>
              <span class="ring-label">{text}</span>
            </div>
            """);
    }

    /// <summary>Progress bar; width animates from 0 on page load.</summary>
    public static IHtmlContent Progress(this IHtmlHelper _, int value, string? css = null) =>
        new HtmlString($"<div class=\"progress {css}\" role=\"progressbar\" aria-valuenow=\"{value}\" aria-valuemin=\"0\" aria-valuemax=\"100\"><span style=\"--value:{Math.Clamp(value, 0, 100)}%\"></span></div>");

    /// <summary>JSON payload for a chart canvas (read by edu.js).</summary>
    public static string ChartJson(object spec) => JsonSerializer.Serialize(spec, JsonOpts);

    public static string Num(int n) => n.ToString("#,0", CultureInfo.InvariantCulture).Replace(",", " ");
}
