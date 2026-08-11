# Onboarding — HueByte/Gabriel

> *A curated reading path through this codebase for new contributors. Work through the stops in order.*

This reading path gets a new contributor from zero to being able to make small, safe changes in Gabriel. Read it in order: the early stops show the system shape and where execution begins, the middle stops trace a request and the business logic it touches, and the later stops show the data contracts and the conventional places to add new code.

## What this project is
Read the auto-generated architecture overview at [Architecture.md](Architecture.md) to pick up the system’s collaboration pattern, the main components (API, Engine, Infrastructure, and webapp), and where state is owned. This document highlights the boundaries and the primary data flows so you know which subsystems to look into next.

## Where execution starts
Open [Program.cs](../Code/src/api/Gabriel.API/Program.cs.md) to see how the ASP.NET Core host is bootstrapped: logging, configuration (including Infisical-sourced secrets), service registration, hosted services, and middleware wiring are centralized there. For the front end, inspect [App.tsx](../Code/src/webapp/src/App.tsx.md) and [main.tsx](../Code/src/webapp/src/main.tsx.md) to see how the React application is mounted, where the one-time authentication HTTP interceptor is installed, global styles are loaded, and App is rendered inside StrictMode.

## Where requests come in
Trace inbound requests through the API surface beginning with the [AuthController.cs](../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) controller, which exposes the authentication endpoints clients hit (login, refresh, me, etc.). Also read the [GlobalExceptionHandler.cs](../Code/src/api/Gabriel.API/Middleware/GlobalExceptionHandler.cs.md) middleware to understand how uncaught exceptions are translated into API responses and how errors are centralized for logging and client-friendly messages.

## Where the business logic lives
Look at the core and engine abstractions and implementations: the [IJwtTokenService](../Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md) interface defines token operations and is implemented by [JwtTokenService](../Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md) in Infrastructure; chat behavior is exposed by the [IChatProvider](../Code/src/api/Gabriel.Engine/Providers/IChatProvider.cs.md) contract and agent orchestration is implemented by [AgentService.cs](../Code/src/api/Gabriel.Engine/Services/AgentService.cs.md) (the concrete class implementing IAgentService). On the frontend, inspect [AuthContext.tsx](../Code/src/webapp/src/auth/AuthContext.tsx.md) to see the in-browser AuthState and how UI code consumes authentication state.

## Where state lives
Examine the API contracts that cross the HTTP boundary to understand persisted and transmitted shapes: [JwtResponse.cs](../Code/src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs.md) defines the token response, [LoginRequest.cs](../Code/src/api/Gabriel.API/Contracts/Auth/LoginRequest.cs.md) the login payload, [MeResponse.cs](../Code/src/api/Gabriel.API/Contracts/Auth/MeResponse.cs.md) the authenticated user shape, and [RefreshTokenRequest.cs](../Code/src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs.md) the refresh-token payload. These records show what the API stores or issues and which boundary (API contract layer) owns those shapes.

## Where to put new code
Follow the project’s conventions when adding features: API endpoints belong in controllers such as [AuthController.cs](../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md), domain services live in Core (for example [ChatService.cs](../Code/src/api/Gabriel.Core/Services/ChatService.cs.md) implements chat use cases), repository interfaces like [IConversationRepository](../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md) declare persistence contracts, and Engine tools such as [FileInfoTool.cs](../Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md) implement tool interfaces for agent workflows. Use these files as canonical examples for placement and naming when you add controllers, services, repositories, or engine tools.

## Next steps
Start by reading [Architecture.md](Architecture.md) to confirm the high-level boundaries, then open [Program.cs](../Code/src/api/Gabriel.API/Program.cs.md) and the frontend [main.tsx](../Code/src/webapp/src/main.tsx.md) to run and observe the dev servers so you can hit the [AuthController.cs](../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) endpoints and follow a request through the stack.

---
*Synthesised by Aurion on 2026-07-08 05:46:46 UTC*
