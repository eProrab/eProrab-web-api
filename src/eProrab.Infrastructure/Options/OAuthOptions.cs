namespace eProrab.Infrastructure.Options;

/// <summary>
/// OAuth configuration options for Google and Facebook sign-in.
/// </summary>
public class OAuthOptions
{
    public const string SectionName = "OAuth";

    public GoogleOAuthOptions Google { get; set; } = new();

    public FacebookOAuthOptions Facebook { get; set; } = new();
}

public class GoogleOAuthOptions
{
    /// <summary>
    /// Google OAuth 2.0 Client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Google OAuth 2.0 Client Secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}

public class FacebookOAuthOptions
{
    /// <summary>
    /// Facebook App ID
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// Facebook App Secret
    /// </summary>
    public string AppSecret { get; set; } = string.Empty;
}
