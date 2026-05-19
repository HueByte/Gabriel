using Microsoft.AspNetCore.Identity;

namespace Gabriel.Infrastructure.Identity;

// Identity user with a Guid key for consistency with the rest of the domain.
// No app-specific fields yet - add them here when needed (display name, avatar
// preferences, etc.). Stays in Infrastructure because Identity is a persistence
// concern; Core only references users by their Guid id.
public class ApplicationUser : IdentityUser<Guid>
{
}
