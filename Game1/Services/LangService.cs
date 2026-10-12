namespace Game1.Services;

/// <summary>
/// Current UI language. Supported: ru (default), kk, en.
/// The language is selected in the UI (language switcher) and stored in the "edu_lang" cookie.
/// </summary>
public class LangService(IHttpContextAccessor http)
{
    public const string CookieName = "edu_lang";

    private string? _current;

    public string Current => _current ??= http.HttpContext?.Request.Cookies[CookieName] switch
    {
        "kk" => "kk",
        "en" => "en",
        _ => "ru"
    };

    /// <summary>Short label shown in the switcher: KZ / RU / EN.</summary>
    public string Code => Current switch { "kk" => "KZ", "en" => "EN", _ => "RU" };

    /// <summary>Pick the string for the current language.</summary>
    public string T(string ru, string kk, string en) => Current switch
    {
        "kk" => kk,
        "en" => en,
        _ => ru
    };

    public static readonly (string Value, string Code, string Flag, string Name)[] Languages =
    [
        ("kk", "KZ", "🇰🇿", "Қазақша"),
        ("ru", "RU", "🇷🇺", "Русский"),
        ("en", "EN", "🇬🇧", "English")
    ];
}
