using System.Security.Claims;
using Gabriel.Core.Identity;

namespace Gabriel.API.Identity;

// Pulls the current user from HttpContext.User. Works across all three auth
// schemes (Identity cookie, Identity opaque bearer, our minted JWT) because
// they all populate the same ClaimsPrincipal.
public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return null;

            // Identity stores the user id under ClaimTypes.NameIdentifier; JWTs we mint
            // use "sub" per the JWT registered-claim convention. Check both.
            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? user.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated
        => _accessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public string? Email
        => _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
}
