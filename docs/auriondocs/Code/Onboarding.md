# Onboarding — HueByte/Gabriel

> *A curated reading path through this codebase for new contributors. Work through the stops in order.*

This onboarding reading path gets a new contributor from zero to being able to make small, safe changes. Read the stops in order: you start with a high-level map of the system, then inspect how the app is wired at startup, trace an incoming request through the API, read the core business services and identity pieces, inspect the contracts that travel over the wire, and finally learn the conventional folders where you should add new code.

## What this project is
Read the auto-generated architecture overview to pick up the system's collaboration pattern, the main components, and where state lives in the repository by opening the [Architecture.md](Architecture.md). This file summarizes the system-level boundaries and the dominant runtime patterns that you'll see again in the code (for example, API vs webapp responsibilities and where data persists).

## Where execution starts
At this stop you'll see how the API and web UI bootstrap and what pieces are wired at startup. Open [Program.cs](../Code/src/api/Gabriel.API/Program.cs.md) to see how Gabriel.API bootstraps Serilog early, constructs the ASP.NET Core WebApplication, and wires hosting, dependency injection, and the middleware pipeline that shapes request handling. For the frontend, inspect [main.tsx](../Code/src/webapp/src/main.tsx.md) to learn how the React app is initialized (it installs a one-time authentication interceptor for the API client, imports global styles, and renders the application) and [App.tsx](../Code/src/webapp/src/App.tsx.md) to see the top-level React component that composes routes and UI shells.

## Where requests come in
Trace an incoming request through the HTTP ingress and the error path. The [AuthController.cs](../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) is an [ApiController] that exposes the authentication endpoints the frontend calls during sign-in and token refresh. Read [GlobalExceptionHandler.cs](../Code/src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs.md) to understand how uncaught exceptions are translated into API responses and where global error handling is centralized in the pipeline wired by Program.cs.

## Where the business logic lives
Read the core service and provider contracts and their primary implementations that the controllers call into. The [IJwtTokenService](../Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md) defines the token operations used by authentication flows, and the concrete [JwtTokenService](../Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md) provides that implementation inside the Infrastructure layer. Conversation and assistant behavior are mediated through the engine: examine [IChatProvider](../Code/src/api/Gabriel.Engine/Providers/IChatProvider.cs.md) for the chat-provider contract and [AgentService](../Code/src/api/Gabriel.Engine/Services/AgentService.cs.md) for the engine-level service that orchestrates agent logic and implements IAgentService. On the frontend, [AuthContext.tsx](../Code/src/webapp/src/auth/AuthContext.tsx.md) defines the React-side authentication state shape (the exported AuthState) used by components to know the current user and tokens.

## Where state lives
Open the API contract records to see what the auth endpoints send and receive. The API uses small DTO records: [LoginRequest](../Code/src/api/Gabriel.API/Contracts/Auth/LoginRequest.cs.md) carries Email and Password for sign-in, [RefreshTokenRequest](../Code/src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs.md) carries a RefreshToken for renewals, [JwtResponse](../Code/src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs.md) represents the token payload returned on successful auth, and [MeResponse](../Code/src/api/Gabriel.API/Contracts/Auth/MeResponse.cs.md) is the shape returned for the current user's identity (Id and Email). These contracts define the wire shapes between frontend and API and are the place to update when you change auth surface area.

## Where to put new code
This stop explains the conventional homes for new contributions so pull requests land in the expected folders. Add HTTP endpoints and API surface changes under API controllers like [AuthController.cs](../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md); put domain service implementations in Core (for example [ChatService](../Code/src/api/Gabriel.Core/Services/ChatService.cs.md) implements chat-related logic), declare repository interfaces in Core (see [IConversationRepository](../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md) for persistence contracts), and add engine tools under the Engine area (for example [FileInfoTool](../Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md) is a sealed ITool implementation). Follow these folders to keep responsibilities and DI registrations clear.

## Next steps
First contribution suggestion: read the [Architecture.md](Architecture.md) overview, then start the application locally (run the API and the webapp dev servers) and exercise the auth flow (login → refresh → me) so you can observe Program.cs, GlobalExceptionHandler.cs, AuthController.cs, and the token services in action. This gives a short, executable path that touches startup, ingress, business logic, and the contracts you just reviewed.

---
*Synthesised by Aurion on 2026-07-07 18:12:55 UTC*
