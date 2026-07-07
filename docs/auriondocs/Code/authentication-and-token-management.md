# Authentication and token management

> Authentication endpoints and payloads for login, token issuance, and token refresh.

*Figure: How Authentication and token management works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
sequenceDiagram
participant Client
participant AuthController
participant LoginRequest
participant JwtResponse
participant RefreshTokenRequest

Client->>AuthController: POST /login with LoginRequest(Email, Password)
AuthController->>LoginRequest: Deserialize and validate LoginRequest
AuthController->>JwtResponse: Construct JwtResponse(accessToken, refreshToken)
AuthController-->>Client: 200 OK JwtResponse

Client->>AuthController: POST /refresh with RefreshTokenRequest(RefreshToken)
AuthController->>RefreshTokenRequest: Deserialize and validate RefreshTokenRequest
AuthController->>JwtResponse: Construct new JwtResponse(accessToken, refreshToken)
AuthController-->>Client: 200 OK JwtResponse
```

Authentication and token management

This topic covers the HTTP surface and small data contracts used to authenticate users and manage JWT / refresh-token lifecycles. You will see the controller that exposes login, refresh, revoke and related endpoints, and the immutable request/response records used as the request and response payloads; together they support both browser-based (HttpOnly cookie) and external-client (token-in-body) flows.

## AuthController.cs
Exposes authentication endpoints (login, token refresh, etc.).

The [AuthController](../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) class is the API surface for registration, login, refresh, logout, revoke (single token), revoke-all and the "me" endpoint. It centralizes JWT issuance and refresh-token lifecycle operations by calling into an IJwtTokenService to mint, rotate and revoke refresh tokens while delegating user persistence and credential verification to ASP.NET Identity (UserManager and SignInManager). The controller intentionally both returns a [JwtResponse](../Code/src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs.md) in the response body and sets/clears HttpOnly cookies so the same endpoints serve browser SPAs (which use cookies) and external clients (which read tokens from the body). Registration can be toggled at runtime via AuthOptions read through IOptionsMonitor; when disabled the controller returns a 403.

## LoginRequest.cs
Represents login request payload (email and password).

[LoginRequest](../Code/src/api/Gabriel.API/Contracts/Auth/LoginRequest.cs.md) is a positional, immutable record that carries the Email and Password fields for the login endpoint. The controller consumes this DTO on login requests (POST /api/auth/login) to obtain credentials that Identity will validate; because it contains a password it should be treated as sensitive (redact in logs and send only over TLS).

## JwtResponse.cs
Represents issued JWT token response.

[JwtResponse](../Code/src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs.md) is the immutable response contract returned by endpoints that issue tokens (for example POST /api/auth/jwt and POST /api/auth/jwt/refresh). It contains the short-lived signed access token (AccessToken) and its expiry (AccessExpiresAt), plus an opaque RefreshToken and its expiry (RefreshExpiresAt). The controller produces this record for external clients to consume while also writing the refresh token into an HttpOnly cookie for browser clients; the contract notes that refresh tokens are rotated on use and both tokens must be treated as sensitive.

## RefreshTokenRequest.cs
Represents a refresh token submission for renewal.

[RefreshTokenRequest](../Code/src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs.md) is a single-field positional record that carries a RefreshToken string when an external client submits a refresh request in the request body. The controller accepts this DTO on refresh calls (POST /api/auth/refresh for external clients) and forwards the token to the IJwtTokenService/validation logic; clients must transmit this record over HTTPS and replace stored tokens when the server returns a rotated refresh token.

How the pieces fit

The [AuthController](../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) depends on the three small contracts: it accepts [LoginRequest](../Code/src/api/Gabriel.API/Contracts/Auth/LoginRequest.cs.md) to authenticate credentials, accepts [RefreshTokenRequest](../Code/src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs.md) when external clients present a refresh token, and issues [JwtResponse](../Code/src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs.md) containing the access and refresh tokens. Responsibility is split so the controller orchestrates HTTP behavior (cookies vs. body, registration toggle via AuthOptions) and delegates credential checks to Identity and token operations to the IJwtTokenService; this enables a single set of endpoints to serve both browser-first and external-client authentication flows while keeping token contracts and security guidance explicit.

---
*Synthesised by Aurion on 2026-07-07 18:10:39 UTC*
