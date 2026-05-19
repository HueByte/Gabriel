namespace Gabriel.Core.Identity;

// Per-user preferences read/written outside the JWT (claims would balloon if
// every preference lived there). Currently scoped to model selection but the
// shape is intentionally a record so adding fields is a one-line change.
public sealed record UserPreferences(string? PreferredProvider, string? PreferredModel);

// Implemented by Gabriel.Infrastructure; AgentService consumes via DI. Always
// scoped per-request so it can lean on ICurrentUser / DbContext.
public interface IUserPreferences
{
    Task<UserPreferences> GetAsync(CancellationToken ct = default);

    // Pass null/null to clear back to the config default.
    Task SetPreferredModelAsync(string? provider, string? model, CancellationToken ct = default);
}
