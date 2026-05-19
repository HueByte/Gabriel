using Gabriel.Core.Configuration;
using Gabriel.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Gabriel.API.Identity;

// Idempotent startup seeder. Runs after EF migrations so the Users table
// exists. Behaviors:
//   * Auth:Seed:Enabled=false → no-op.
//   * UserName or Password blank → log a warning and no-op (we don't want a
//     missing config silently minting a half-initialized account).
//   * User with the configured UserName already exists → no-op.
//   * Otherwise → create the user with the configured plaintext password,
//     which Identity hashes via the registered PasswordHasher.
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var auth = services.GetRequiredService<IOptions<AuthOptions>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Gabriel.IdentitySeeder");

        var seed = auth.Seed;
        if (!seed.Enabled)
        {
            return;
        }

        if (!seed.IsConfigured)
        {
            logger.LogWarning(
                "Auth:Seed is enabled but UserName or Password is blank — skipping seed. " +
                "Set Auth:Seed:UserName and Auth:Seed:Password (env: AUTH__SEED__USERNAME / AUTH__SEED__PASSWORD).");
            return;
        }

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();

        var existing = await users.FindByNameAsync(seed.UserName);
        if (existing is not null)
        {
            logger.LogInformation("Seed user {UserName} already exists — skipping seed.", seed.UserName);
            return;
        }

        var user = new ApplicationUser
        {
            UserName = seed.UserName,
            Email = seed.ResolvedEmail,
            EmailConfirmed = true,
        };

        var result = await users.CreateAsync(user, seed.Password);
        if (!result.Succeeded)
        {
            var detail = string.Join("; ", result.Errors.Select(e => e.Description));
            logger.LogError(
                "Failed to seed user {UserName}: {Errors}. Check Identity password policy + email uniqueness.",
                seed.UserName, detail);
            return;
        }

        logger.LogInformation("Seeded user {UserName} <{Email}>.", user.UserName, user.Email);
    }
}
