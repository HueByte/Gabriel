namespace Gabriel.API.Contracts.Auth;

// Returned by POST /api/auth/jwt and POST /api/auth/jwt/refresh.
// Access token is a short-lived signed JWT (decode for claims).
// Refresh token is an opaque high-entropy string — store it server-side or in
// a secure client-side store, then trade it via /jwt/refresh when access expires.
// /refresh rotates the refresh token on every call.
public record JwtResponse(
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt);
