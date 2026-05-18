using System.Text;
using Gabriel.Core.Identity;
using Gabriel.Infrastructure.Persistence;
using Gabriel.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Gabriel.Infrastructure.Identity;

// Renamed off the framework's IdentityServiceCollectionExtensions to avoid the
// classic CS0433 collision when consumers `using` both this namespace and the
// stock Microsoft.AspNetCore.Identity / Microsoft.Extensions.Identity.Core ones.
public static class GabrielIdentityExtensions
{
    // Cookie names — kept here so the JwtBearer cookie-fallback and the AuthController
    // cookie writer can't drift apart.
    public const string AccessCookieName = "gabriel.access";
    public const string RefreshCookieName = "gabriel.refresh";

    // The single source of truth for the auth stack:
    //   - IdentityCore + EF store for UserManager / SignInManager (we never call
    //     PasswordSignInAsync since we don't use Identity cookies; CheckPasswordSignInAsync
    //     is enough for credential validation)
    //   - JwtBearer as the ONLY auth scheme. Reads from Authorization: Bearer normally,
    //     falls back to the HttpOnly access cookie set by AuthController on login.
    //   - IJwtTokenService for mint/refresh/revoke (handles refresh-token rotation + theft detection)
    public static IServiceCollection AddIdentityAndAuth(this IServiceCollection services, IConfiguration config)
    {
        // JwtOptions — bound + validated. SKIP_DB_INIT (build-time swagger CLI) skips validation
        // since the signing key isn't available yet during the codegen pass.
        var jwtOptionsBuilder = services.AddOptions<JwtOptions>()
            .Bind(config.GetSection(JwtOptions.SectionName));

        if (Environment.GetEnvironmentVariable("SKIP_DB_INIT") != "true")
        {
            jwtOptionsBuilder
                .Validate(
                    o => o.IsConfigured,
                    $"{JwtOptions.SectionName}:SigningKey is required and must be at least 32 characters. Set via Infisical (JWT__SIGNINGKEY) or user-secrets.")
                .ValidateOnStart();
        }

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();

        // IdentityCore — user store + password hashing + UserManager + SignInManager.
        // No AddApiEndpoints (no MapIdentityApi), no AddDefaultUI.
        services.AddIdentityCore<ApplicationUser>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                opts.SignIn.RequireConfirmedAccount = false;  // email confirmation not wired yet
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Single auth scheme: JwtBearer. Reads from Authorization header OR the
        // HttpOnly access cookie — whichever is present.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, _ => { /* configured via PostConfigure below */ });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOpts) =>
            {
                var jwt = jwtOpts.Value;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = string.IsNullOrEmpty(jwt.SigningKey)
                        ? null
                        : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                bearer.Events = new JwtBearerEvents
                {
                    // Webapp flow: browser holds the JWT in an HttpOnly cookie (set on /login by
                    // AuthController). Authorization header isn't present, so we copy the cookie
                    // token over so the standard validator picks it up.
                    OnMessageReceived = ctx =>
                    {
                        if (string.IsNullOrEmpty(ctx.Token) &&
                            ctx.Request.Cookies.TryGetValue(AccessCookieName, out var cookieToken))
                        {
                            ctx.Token = cookieToken;
                        }
                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization(opts =>
        {
            opts.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            opts.FallbackPolicy = null; // [Authorize] is opt-in per endpoint
        });

        return services;
    }
}
