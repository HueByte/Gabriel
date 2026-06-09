# Authentication and Authorization

> How the system authenticates users, issues tokens, and protects API and UI surfaces across services.

This guide explains how the application authenticates users, issues and rotates tokens, and protects both API and web UI surfaces. It maps the HTTP surface and the supporting identity services so you can find where to change login, token handling, and per-request identity checks. Read the referenced files to see implementation details and the public contracts you should depend on.

## AuthController.cs
Exposes the central HTTP endpoints for authentication flows (register, login, refresh, etc.).

[AuthController.cs](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) is the HTTP entry point for all auth-related user interactions: register, login, token refresh, logout, revoke, revoke-all, and the /me endpoint. It translates HTTP requests into identity operations, delegates token issuance and rotation to the token service, and coordinates cookie handling for browser flows. When integrating auth into new UI routes or APIs, start at this controller to see how endpoints call into the token service and cookie helper and how responses are shaped for clients.

## AuthCookies.cs
Manages authentication cookies for access and refresh tokens on the web surface.

[AuthCookies.cs](Code/src/api/Gabriel.API/Identity/AuthCookies.cs.md) centralizes the logic for setting and clearing the access and refresh cookies used by web clients. It provides the concrete mechanics of cookie flags, expiration, and a small accessor for reading the refresh cookie, so web-oriented endpoints in the controller rely on it rather than duplicating cookie logic. Use this helper when implementing or changing how tokens are delivered to browsers (secure-only, SameSite, lifetimes, etc.).

## HttpContextCurrentUser.cs
Provides per-request identity information via HttpContext for authorization decisions.

[HttpContextCurrentUser.cs](Code/src/api/Gabriel.API/Identity/HttpContextCurrentUser.cs.md) exposes a testable ICurrentUser façade that reads the authenticated principal from ASP.NET Core's HttpContext via an IHttpContextAccessor. Controllers and authorization checks use this façade to obtain the current UserId, IsAuthenticated, and Email without coupling business logic to HttpContext details. Turn here when you need to implement or mock per-request identity for authorization logic or policy evaluation.

## JwtTokenService.cs
Implements token issuance/rotation using JWT for API authentication.

[JwtTokenService.cs](Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md) provides the concrete implementation that mints short-lived JWT access tokens, manages server-side refresh tokens, and performs rotation/revocation behaviors. It implements the token lifecycle called by the HTTP controller: creating access tokens for API calls, issuing and persisting refresh tokens, and rotating or revoking refresh tokens on refresh or logout operations. If you need to change token formats, signing keys, or refresh policies, this is the implementation to modify.

## IJwtTokenService.cs
Defines the contract for issuing and managing JWTs and refresh tokens.

[IJwtTokenService.cs](Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md) is the abstraction consumers depend on for token operations. It specifies the surface for issuing access tokens, generating/rotating refresh tokens, and validating or revoking tokens. Other parts of the system should depend on this interface so implementations like JwtTokenService can be swapped, mocked, or decorated without touching controllers or cookie helpers.

How the pieces fit

The AuthController is the HTTP gateway that orchestrates authentication flows: it calls the IJwtTokenService implementation (JwtTokenService) to mint and rotate tokens and uses AuthCookies to deliver or clear tokens for web clients. Per-request authorization reads identity through HttpContextCurrentUser, which surfaces the authenticated principal for business logic and policy checks. The interface/implementation split (IJwtTokenService -> JwtTokenService) keeps token lifecycle concerns encapsulated and testable while the controller and cookie helper focus on HTTP and browser interactions.

---
*Synthesised by Aurion on 2026-06-09 03:21:57 UTC*
