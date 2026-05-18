namespace Gabriel.Core.Identity;

// Per-request view of the authenticated user. Implemented in the API layer by
// pulling from HttpContext.User; Core stays unaware of HTTP plumbing.
public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    string? Email { get; }
}
