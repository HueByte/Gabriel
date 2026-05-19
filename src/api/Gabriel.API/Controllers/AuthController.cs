using Gabriel.API.Contracts.Auth;
using Gabriel.API.Identity;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

// Single auth surface for the app:
//
//   POST /api/auth/register   anonymous   — create user, mint pair, set HttpOnly cookies, return tokens
//   POST /api/auth/login      anonymous   — verify creds, mint pair, set cookies, return tokens
//   POST /api/auth/refresh    anonymous   — rotate refresh (reads cookie OR body), set new cookies
//   POST /api/auth/logout     anonymous   — revoke refresh from cookie, clear cookies
//   POST /api/auth/revoke     anonymous   — revoke a specific refresh token from the body (external)
//   POST /api/auth/revoke-all [Authorize] — revoke every active refresh token for the current user
//   GET  /api/auth/me         [Authorize] — return id + email of the current user
//
// register / login / refresh: set cookies AND return tokens in the body. The webapp ignores
// the body and lets the browser handle cookies; external clients ignore the cookies and use
// the body. One endpoint serves both audiences.
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IJwtTokenService _jwt;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        IJwtTokenService jwt,
        ICurrentUser currentUser,
        ILogger<AuthController> logger)
    {
        _users = users;
        _signIn = signIn;
        _jwt = jwt;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<JwtResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
        };

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            // Surface Identity's complaints (password too short, email taken, etc.) as a clean 400.
            var detail = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new DomainException(detail);
        }

        var pair = await _jwt.IssueAsync(user.Id, user.Email!, ct);
        AuthCookies.Set(Response, pair);
        return Ok(ToResponse(pair));
    }

    [HttpPost("login")]
    public async Task<ActionResult<JwtResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // Identical error path to "wrong password" so we don't enumerate accounts.
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // CheckPasswordSignInAsync verifies the password and updates lockout counters
        // WITHOUT issuing any Identity cookies/sessions — perfect for our JWT flow.
        var result = await _signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var pair = await _jwt.IssueAsync(user.Id, user.Email!, ct);
        AuthCookies.Set(Response, pair);
        return Ok(ToResponse(pair));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<JwtResponse>> Refresh([FromBody] RefreshTokenRequest? bodyRequest, CancellationToken ct)
    {
        // Webapp sends nothing meaningful in the body and relies on the refresh
        // cookie; external clients send the refresh token in the body. Prefer
        // the cookie since it's authoritative for the active browser session.
        var refresh = AuthCookies.ReadRefresh(Request) ?? bodyRequest?.RefreshToken;
        var hasCookie = !string.IsNullOrEmpty(AuthCookies.ReadRefresh(Request));

        if (string.IsNullOrWhiteSpace(refresh))
        {
            // Nothing to refresh from. Clear any stale cookies in the same
            // response so the browser stops re-sending dead tokens on every
            // subsequent request.
            _logger.LogInformation("Refresh attempted with no token (cookie={HasCookie})", hasCookie);
            AuthCookies.Clear(Response);
            return BuildUnauthorized("Refresh token is required.");
        }

        try
        {
            var pair = await _jwt.RefreshAsync(refresh, ct);
            AuthCookies.Set(Response, pair);
            _logger.LogInformation("Refresh succeeded (cookie={HasCookie})", hasCookie);
            return Ok(ToResponse(pair));
        }
        catch (UnauthorizedAccessException ex)
        {
            // CRITICAL: we return Unauthorized() inline rather than re-throwing
            // because ASP.NET's UseExceptionHandler calls Response.Clear() on
            // throw, which wipes any Set-Cookie headers we'd want to attach.
            // Clearing cookies here means the browser immediately stops sending
            // dead tokens — the webapp's signal-expired flow can then redirect
            // to login cleanly.
            _logger.LogInformation("Refresh failed: {Reason}", ex.Message);
            AuthCookies.Clear(Response);
            return BuildUnauthorized(ex.Message);
        }
    }

    // Build a ProblemDetails 401 response that survives alongside our explicit
    // Response.Cookies edits — the global exception handler can't help us here
    // because it would Response.Clear() first.
    private ActionResult BuildUnauthorized(string detail)
    {
        return new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = detail,
            Instance = HttpContext.Request.Path,
        })
        {
            StatusCode = StatusCodes.Status401Unauthorized,
        };
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refresh = AuthCookies.ReadRefresh(Request);
        if (!string.IsNullOrEmpty(refresh))
        {
            await _jwt.RevokeAsync(refresh, ct);
        }
        AuthCookies.Clear(Response);
        return NoContent();
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        // Idempotent — always 204 so callers can't probe which tokens existed.
        await _jwt.RevokeAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("revoke-all")]
    public async Task<IActionResult> RevokeAll(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");
        await _jwt.RevokeAllForUserAsync(userId, ct);
        AuthCookies.Clear(Response);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<MeResponse> Me()
    {
        var id = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user required.");
        var email = _currentUser.Email ?? string.Empty;
        return Ok(new MeResponse(id, email));
    }

    private static JwtResponse ToResponse(TokenPair pair)
        => new(pair.AccessToken, pair.AccessExpiresAt, pair.RefreshToken, pair.RefreshExpiresAt);
}
