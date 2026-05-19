using Gabriel.Core.Identity;
using Gabriel.Infrastructure.Identity;

namespace Gabriel.API.Identity;

// One place to set / clear the auth cookies. Kept aligned with the cookie name
// JwtBearer reads in OnMessageReceived (see IdentityServiceCollectionExtensions)
// so the access cookie produces a valid principal when the browser sends it back.
internal static class AuthCookies
{
    public const string AccessCookieName = GabrielIdentityExtensions.AccessCookieName;
    public const string RefreshCookieName = GabrielIdentityExtensions.RefreshCookieName;

    // Refresh cookie is scoped to the auth subtree - it should never travel on
    // ordinary API calls. Limits the blast radius if a single response is leaked.
    private const string RefreshCookiePath = "/api/auth";

    public static void Set(HttpResponse response, TokenPair pair)
    {
        var isHttps = response.HttpContext.Request.IsHttps;

        response.Cookies.Append(AccessCookieName, pair.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = pair.AccessExpiresAt,
        });

        response.Cookies.Append(RefreshCookieName, pair.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = SameSiteMode.Lax,
            Path = RefreshCookiePath,
            Expires = pair.RefreshExpiresAt,
        });
    }

    public static void Clear(HttpResponse response)
    {
        // Delete must echo the path the cookie was set with, otherwise the browser
        // treats the directive as a different cookie and ignores it.
        response.Cookies.Delete(AccessCookieName, new CookieOptions { Path = "/" });
        response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = RefreshCookiePath });
    }

    public static string? ReadRefresh(HttpRequest request)
        => request.Cookies.TryGetValue(RefreshCookieName, out var v) ? v : null;
}
