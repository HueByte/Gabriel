namespace Gabriel.Core.Configuration;

// Auth-surface knobs that aren't part of JWT issuance itself:
//   * RegistrationEnabled — a kill-switch for /api/auth/register. Useful for
//     single-tenant / private deployments where only the seeded user should
//     ever exist.
//   * Seed — a bootstrap user created at startup. Idempotent: skipped if a
//     user with the configured UserName already exists.
public class AuthOptions : IConfigSection<AuthOptions>
{
    public static string SectionName => "Auth";

    // When false, POST /api/auth/register returns 403. Login / refresh /
    // logout stay open so existing accounts (e.g. the seeded one) keep working.
    public bool RegistrationEnabled { get; set; } = true;

    public SeedUserOptions Seed { get; set; } = new();
}

public class SeedUserOptions
{
    // Opt-in. Default off so a misconfigured deployment doesn't silently mint
    // an account with whatever placeholder values are in the example file.
    public bool Enabled { get; set; }

    // Identity user fields. UserName is what shows in JWTs and is what
    // ApplicationUser keys on; Email is what login() looks up by. The
    // registration endpoint sets UserName = Email, but the seed path lets
    // them differ so a deployment can have, say, UserName "admin" with a
    // real mailbox address as Email.
    public string UserName { get; set; } = string.Empty;

    // Falls back to UserName if blank, mirroring the register endpoint's
    // behavior. RequireUniqueEmail is on, so this must parse as an email
    // when Identity validates it.
    public string Email { get; set; } = string.Empty;

    // Plaintext only on the way in — Identity hashes it via the configured
    // PasswordHasher. Never logged.
    public string Password { get; set; } = string.Empty;

    public string ResolvedEmail => string.IsNullOrWhiteSpace(Email) ? UserName : Email;

    public bool IsConfigured =>
        Enabled
        && !string.IsNullOrWhiteSpace(UserName)
        && !string.IsNullOrWhiteSpace(Password);
}
