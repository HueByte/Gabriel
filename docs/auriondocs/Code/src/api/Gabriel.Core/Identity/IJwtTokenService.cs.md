# IJwtTokenService.cs

> **Source:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`

## Contents

- [IJwtTokenService](#ijwttokenservice)
- [TokenPair](#tokenpair)

---

## IJwtTokenService
> **File:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`  
> **Kind:** interface

```csharp
public interface IJwtTokenService
```


IJwtTokenService defines a contract for issuing short-lived JWT access tokens paired with rotatable refresh tokens and for managing their lifecycle across a user’s devices. Implementations issue a TokenPair after authentication, refresh tokens on demand, and support revocation of single tokens or all tokens for a user, enabling sign-out everywhere and recovery from credential compromise.

## Remarks
This abstraction centralizes token lifecycle concerns (issuance, rotation, and revocation) behind a single interface, enabling consistent security policies and easier testing. By separating stateless access tokens from stateful refresh tokens, it allows scalable token validation while retaining the ability to revoke tokens when necessary (e.g., on sign-out or credential compromise).

## Notes
- Refresh token rotation means a reused token can indicate theft; revocation of all active tokens for the user may be triggered in response.
- Treat refresh tokens as secrets: protect them at rest and in transit; do not expose them to insecure frontends or logs.

---

## TokenPair
> **File:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`  
> **Kind:** record

```csharp
public record TokenPair(
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `AccessToken` | `string` | — |
| `AccessExpiresAt` | `DateTimeOffset` | — |
| [`RefreshToken`](RefreshToken.cs.md) | `string` | — |
| `RefreshExpiresAt` | `DateTimeOffset` | — |


TokenPair is a compact, immutable value object that holds an access token, its expiration time, a refresh token, and its expiration time. It is intended to be produced by identity/token services when issuing tokens and carried as a single unit through the authentication workflow. By representing both tokens and their lifetimes together, TokenPair reduces the risk of mismatched tokens and expiry data during transport or storage. As a C# record, it provides value-based equality and built-in immutability; when the access token is rotated, you can create a new TokenPair using a with-expression, updating only the fields you need while preserving the rest.

## Remarks
TokenPair serves as the single container for token information in the authentication pipeline. It encapsulates the two tokens and their lifetimes as a unified contract, reducing boilerplate for callers that need to pass token data around. Its immutable, value-based nature makes it safe to share across boundaries and to use in dictionaries or caches as a key or value.

## Notes
- TokenPair is a value object; equality is by value across all four properties.
- It is immutable; to create a modified version use the with-expression.
- Treat tokens as sensitive data; avoid logging the actual token strings; use redaction in logs and UIs.

---