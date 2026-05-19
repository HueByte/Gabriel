using Microsoft.AspNetCore.Identity;

namespace Gabriel.Infrastructure.Identity;

// Identity user with a Guid key for consistency with the rest of the domain.
// Stays in Infrastructure because Identity is a persistence concern; Core
// only references users by their Guid id.
public class ApplicationUser : IdentityUser<Guid>
{
    // Per-user model selection. Both nullable: when unset, the agent falls
    // back to whichever Providers:*:Models entry has IsActive=true in config.
    //
    // PreferredProvider matches IChatProvider.Name (e.g. "grok"); PreferredModel
    // is the wire-level model identifier sent to the provider (e.g.
    // "grok-4-latest"). Stored as plain strings — the model catalog handles
    // stale references gracefully by falling back to the default.
    public string? PreferredProvider { get; set; }
    public string? PreferredModel { get; set; }
}
