using Gabriel.Core.Identity;
using Microsoft.AspNetCore.Identity;

namespace Gabriel.Infrastructure.Identity;

// IUserPreferences impl. Reads/writes the small set of preference fields
// directly on ApplicationUser — they're cheap enough that a dedicated table
// would just be ceremony. Both calls are no-ops when there's no authenticated
// user (e.g. background processes); AgentService still works because the
// catalog falls back to the config default in that case.
internal sealed class UserPreferencesService : IUserPreferences
{
    private readonly ICurrentUser _currentUser;
    private readonly UserManager<ApplicationUser> _users;

    public UserPreferencesService(ICurrentUser currentUser, UserManager<ApplicationUser> users)
    {
        _currentUser = currentUser;
        _users = users;
    }

    public async Task<UserPreferences> GetAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return new UserPreferences(null, null);

        var user = await _users.FindByIdAsync(userId.Value.ToString());
        if (user is null) return new UserPreferences(null, null);

        return new UserPreferences(user.PreferredProvider, user.PreferredModel);
    }

    public async Task SetPreferredModelAsync(string? provider, string? model, CancellationToken ct = default)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required to update preferences.");

        var user = await _users.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"User {userId} not found.");

        // Normalize empty strings to null so the catalog's "preference unset"
        // branch fires consistently regardless of how the client cleared it.
        user.PreferredProvider = string.IsNullOrWhiteSpace(provider) ? null : provider;
        user.PreferredModel = string.IsNullOrWhiteSpace(model) ? null : model;

        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var detail = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user preferences: {detail}");
        }
    }
}
