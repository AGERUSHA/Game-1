namespace Game1.Services;

public static class AuthSchemes
{
    /// <summary>Temporary cookie holding the Google / Microsoft identity until it is mapped to a local account.</summary>
    public const string External = "External";

    /// <summary>Provider scheme names (as registered by AddGoogle / AddMicrosoftAccount).</summary>
    public const string Google = "Google";
    public const string Microsoft = "Microsoft";
}
