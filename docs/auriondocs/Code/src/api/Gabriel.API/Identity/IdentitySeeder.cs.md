# IdentitySeeder

> **File:** `src/api/Gabriel.API/Identity/IdentitySeeder.cs`  
> **Kind:** class

```csharp
public static class IdentitySeeder
```


IdentitySeeder is an idempotent startup seeder that runs after EF migrations so the Users table exists. It seeds a single Identity user when Auth:Seed is enabled and properly configured; if the user already exists or the seed configuration is incomplete, it logs and exits without creating duplicates or half-initialized accounts. The seeder uses `UserManager<ApplicationUser>` to create the user with the configured UserName and Email, seeded with the configured plaintext password (which Identity hashes via the registered PasswordHasher).