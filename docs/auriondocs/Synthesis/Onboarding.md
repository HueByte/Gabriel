# Onboarding — HueByte/Gabriel

> *A curated reading path through this codebase for new contributors. Work through the stops in order.*

This reading path gets a new contributor from zero to a first change by walking the smallest set of sources that reveal how Gabriel boots, accepts requests, does its work, and persists data. Read the architecture overview first to form a mental model, then follow the entry points into controllers and services so you can run the app and make a small, safe change.

## What this project is

Skim the auto-generated system map to pick up Gabriel's collaboration pattern, its main components (API, Engine, Core, Infrastructure, and Webapp), and where persistent state and contracts live. Start with the [Architecture.md](Architecture.md) overview to see the high-level boundaries and the primary data flow that the rest of this path explores in detail.

## Where execution starts

Learn how the backend and frontend are wired at startup by reading the API and webapp entry points. Inspect [Program.cs](../Code/src/api/Gabriel.API/Program.cs.md) to see how the Gabriel API configures Serilog, loads configuration and secrets, and wires core services at host bootstrap; then open the UI entry points [App.tsx](../Code/src/webapp/src/App.tsx.md) and [main.tsx](../Code/src/webapp/src/main.tsx.md) to see how the frontend installs the one-time authentication interceptor, loads global styles, and mounts the React application.

## Where requests come in

Trace an inbound request through the controller and top-level middleware so you know how exceptions, auth, and endpoints are surfaced. Read the API surface of [AuthController.cs](../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) (an [ApiController]) to see the available auth endpoints, then review [GlobalExceptionHandler.cs](../Code/src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs.md) to understand how unhandled exceptions are caught and translated into responses.

## Where the business logic lives

Meet the services and interfaces that do the actual work behind controllers and UI code. The authentication token contract is defined by the [IJwtTokenService](../Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md) interface with a concrete implementation in [JwtTokenService](../Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md); chat and agent behavior surface through the [IChatProvider](../Code/src/api/Gabriel.Engine/Providers/IChatProvider.cs.md) interface and the [AgentService](../Code/src/api/Gabriel.Engine/Services/AgentService.cs.md) concrete class. On the frontend, the React auth state shape is exposed by [AuthContext.tsx](../Code/src/webapp/src/auth/AuthContext.tsx.md) so you can follow auth data from UI to API.

## Where state lives

Understand the wire and persistence shapes used for auth-related flows by reading the API contract records. The JWT payload returned to clients is in [JwtResponse.cs](../Code/src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs.md), the login input in [LoginRequest.cs](../Code/src/api/Gabriel.API/Contracts/Auth/LoginRequest.cs.md), the authenticated user view in [MeResponse.cs](../Code/src/api/Gabriel.API/Contracts/Auth/MeResponse.cs.md), and refresh token usage in [RefreshTokenRequest.cs](../Code/src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs.md). These contracts show what the API expects and returns for auth flows you’ll exercise when running or testing the app.

## Where to put new code

Follow the repository conventions illustrated by existing controllers, services, repositories, and tools when adding features. Controller endpoints belong under API controllers like [AuthController.cs](../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md); core service implementations belong under Core (for example [ChatService.cs](../Code/src/api/Gabriel.Core/Services/ChatService.cs.md) implements IChatService) and repository interfaces such as [IConversationRepository.cs](../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md) define persistence contracts. Utility/runtime tools live in Engine tools (for example [FileInfoTool.cs](../Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md) is a sealed ITool implementation).

## Next steps

Begin by reading the [Architecture.md](Architecture.md) overview, then start both the API and webapp dev servers and exercise the auth endpoints through the UI (login, refresh) so you can observe Program.cs, AuthController.cs, JwtTokenService, and the frontend auth flow in action.

---
*Synthesised by Aurion on 2026-07-07 21:08:12 UTC*
