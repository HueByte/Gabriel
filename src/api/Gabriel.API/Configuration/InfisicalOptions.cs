namespace Gabriel.API.Configuration;

public class InfisicalOptions
{
    public const string SectionName = "Infisical";

    public string Host { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string Environment { get; set; } = "dev";
    public string SecretPath { get; set; } = "/";

    // Non-sensitive: safe in appsettings.json. Override per-env via env var if needed.
    public string ClientId { get; set; } = string.Empty;

    // Sensitive: supply via user-secrets (Infisical:ClientSecret) or env var INFISICAL__CLIENTSECRET.
    public string ClientSecret { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 15;

    // If any of the four essentials are missing, the provider silently skips —
    // local dev without Infisical access still boots cleanly.
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(ProjectId) &&
        !string.IsNullOrWhiteSpace(ClientId) &&
        !string.IsNullOrWhiteSpace(ClientSecret);
}
