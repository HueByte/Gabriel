# Onboarding — HueByte/Gabriel

> *A curated reading path through this codebase for new contributors. Work through the stops in order.*

This onboarding reading path gets a new contributor from zero to productive in the Gabriel codebase by steering them through a small, high-value subset of documentation and source files. It highlights where the system starts, how requests flow in, where the core business logic lives, and the conventional locations for new code. Work through the stops in order to build a mental model you can use when picking up your first issue.

## Stop 1: What this project is
Purpose: Skim the auto-generated architecture overview to pick up the system's collaboration pattern, the main components, and where state lives. Read the architecture overview to get the big-picture collaboration pattern (how components depend on each other, the primary responsibilities of API vs Engine vs Core vs Infrastructure, and where long-lived state lives). Start with [Architecture.md](Synthesis/Architecture.md) to capture terminology, high-level data flows, and the major subsystems you will encounter in later stops.

## Stop 2: Where execution starts
Purpose: Read the entry-point file(s) to see how the app is wired together at startup — DI registrations, hosted services, middleware pipeline. The reading path below was derived by walking outward from here. Inspect the API and webapp entry points to understand how hosting, dependency injection, and client bootstrapping are configured. Look at the API bootstrap in [Program.cs](Code/src/api/Gabriel.API/Program.cs.md) to see registration of services and middleware, then open the React app entry files [App.tsx](Code/src/webapp/src/App.tsx.md) and [main.tsx](Code/src/webapp/src/main.tsx.md) to see how the frontend mounts routes, providers, and connects to the backend.

## Stop 3: Where requests come in
Purpose: Trace one end-to-end request through a controller or realtime hub reachable from the entry points above. Follow an incoming request through an API surface by reading the authentication controller to see how HTTP routes map to application actions and which services are invoked. The controller in [AuthController.cs](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) is a good single-path example: it shows request/response shapes, authorization attributes, and the service calls that implement the auth flow.

## Stop 4: Where the business logic lives
Purpose: These workhorses are reached transitively from the ingress layer — services, agents, and pipeline steps doing the actual work behind the API surface. After seeing the controller, dive into the services it relies on and the concrete implementations that perform domain work. Read the interface [IJwtTokenService.cs](Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md) to learn the contract for token operations, then inspect the concrete provider [JwtTokenService.cs](Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md) to see how tokens are actually created and validated. Also review [AgentService.cs](Code/src/api/Gabriel.Engine/Services/AgentService.cs.md) to understand a core engine service that encapsulates agent orchestration and how higher-level operations are implemented.

## Stop 5: Where to put new code
Purpose: Conventional homes for each kind of contribution, computed from the modal folder per role across the whole codebase (not just the reachable subgraph). Learn the repository's conventions for placing new controllers, services, repositories, and tools by reading representative files in each layer. Revisit [AuthController.cs](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) for an example API controller pattern, inspect [ChatService.cs](Code/src/api/Gabriel.Core/Services/ChatService.cs.md) to see a Core service shape and dependency boundaries, open [IConversationRepository.cs](Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md) to learn repository interface conventions, and read [FileInfoTool.cs](Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md) to understand how Engine tools are organized and invoked.

How the pieces fit: the entry points wire hosting and DI (API Program.cs and webapp main/App files) and expose HTTP and browser surfaces. The API controllers (for example AuthController) accept requests and delegate to Core interfaces and Engine services; Core defines the service and repository contracts (IJwtTokenService, IConversationRepository, ChatService) while Infrastructure and Engine provide concrete implementations (JwtTokenService, AgentService, FileInfoTool). This separation keeps wiring and surface concerns in the API/webapp, domain contracts in Core, and execution details in Engine/Infrastructure.

## Next steps
Boot the development environment and exercise an end-to-end auth request: start the API and the webapp locally, open the app in your browser, and perform the login/auth flow so you can observe the Program.cs wiring, the AuthController handling, and the token creation in JwtTokenService working together. This will validate your environment and give you a concrete trace to reference when opening your first issue or PR.

---
*Synthesised by Aurion on 2026-06-08 22:36:04 UTC*
