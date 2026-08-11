# Authentication, tokens, and user sessions

> How the system authenticates users, issues and rotates tokens, and manages session cookies across API layers. This topic covers the surface API, token providers, and identity persistence.

How the system performs authentication, issues and rotates tokens, and persists session identity across API layers. It describes the HTTP surface for login/refresh/logout, the cookie helper used to read and write tokens in responses, the JWT service that mints and rotates tokens, the interface abstraction used by the controller, and the persisted application user record that stores identity and refresh state.

## AuthController.cs
Exposes login/registration HTTP endpoints and token refresh surface.

This controller is the HTTP entry point for authentication actions (register, login, refresh, logout, revoke, revoke-all, and me). It orchestrates requests from clients and delegates token-related work to the token service abstraction ([IJwtTokenService.cs](Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md)), while using the cookie helper ([AuthCookies.cs](Code/src/api/Gabriel.API/Identity/AuthCookies.cs.md)) to set or clear access and refresh tokens in HTTP responses. Think of this file as the routing and policy layer that translates HTTP verbs and JSON payloads into calls into the token lifecycle and identity persistence components.

## AuthCookies.cs
Manages issuing and reading authentication cookies used by the API.

This helper encapsulates cookie semantics for the API (setting, clearing, and reading access and refresh tokens) so controllers like [AuthController.cs](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) don't need to reimplement cookie handling. When the token service issues or rotates tokens ([JwtTokenService.cs](Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md)), the controller uses this helper to persist those tokens to the client as cookies or to remove them on logout/revoke.

## JwtTokenService.cs
Implements token minting and rotation logic for access/refresh tokens.

This implementation encapsulates the core JWT lifecycle: minting short-lived access tokens, creating and persisting server-backed refresh tokens, rotating refresh tokens on use, and detecting reuse/theft. It is the concrete behind the [IJwtTokenService.cs](Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md) abstraction used by the API surface. The service also interacts with persisted user data to store refresh state (see [ApplicationUser.cs](Code/src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs.md)) so refresh tokens can be revoked or rotated safely.

## IJwtTokenService.cs
Defines the interface for token generation and validation used by the auth surface.

This interface expresses the operations the HTTP surface needs (issue access tokens, create/validate/rotate refresh tokens, revoke, and detect token reuse). [AuthController.cs](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) depends on this abstraction so the controller remains implementation-agnostic; the concrete behavior is provided by [JwtTokenService.cs](Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md).

## ApplicationUser.cs
Represents the application user persisted by ASP.NET Identity.

This type models the persisted user record (Guid primary key) and carries any per-user state the token service needs to track (for example, refresh token metadata and user preferences). The token implementation ([JwtTokenService.cs](Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md)) reads and updates this record to persist refresh tokens, detect reuse, and perform revocation so that tokens remain rotatable and revocable across sessions.

How the pieces fit

The runtime flow is: the HTTP client calls endpoints on [AuthController.cs](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md), which relies on [IJwtTokenService.cs](Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md) (implemented by [JwtTokenService.cs](Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md)) to mint or rotate tokens; the controller then uses [AuthCookies.cs](Code/src/api/Gabriel.API/Identity/AuthCookies.cs.md) to write the access and refresh tokens into cookies for the client. The token service persists refresh-state to the user record type defined in [ApplicationUser.cs](Code/src/api/Gabriel.Infrastructure/Identity/ApplicationUser.cs.md), enabling server-side revocation and detection of token reuse.

---
*Synthesised by Aurion on 2026-06-08 22:34:22 UTC*
