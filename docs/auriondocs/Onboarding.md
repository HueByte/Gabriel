# Onboarding — HueByte/Gabriel

> *A curated reading path through this codebase for new contributors. Work through the stops in order.*

This reading path gets a new engineer from zero to being able to contribute to Gabriel by guiding a focused, ordered skim of the high-level architecture, the runtime entry points, an example request flow, the core business services, and the conventional places to add new code. Work through the stops in order and open the linked file docs to see the actual code and signatures referenced below.

## Stop 1: What this project is
Skim the auto-generated architecture overview to pick up the system's collaboration pattern, the main components, and where state lives. Read [Architecture.md](Synthesis/Architecture.md) to pick up the big-picture boundaries between the API, Core, Engine, Infrastructure, and the webapp, and to understand where central state and responsibilities are placed so later code-level reading has context.

## Stop 2: Where execution starts
Read the entry-point file(s) to see how the app is wired together at startup — DI registrations, hosted services, middleware pipeline. The reading path below was derived by walking outward from here. Start by opening [Code/src/api/Gabriel.API/Program.cs.md](Code/src/api/Gabriel.API/Program.cs.md) to see how the backend host is configured and which services are registered, then inspect the frontend boot in [Code/src/webapp/src/main.tsx.md](Code/src/webapp/src/main.tsx.md) and the top-level React composition in [Code/src/webapp/src/App.tsx.md](Code/src/webapp/src/App.tsx.md) to understand how the webapp is initialized and how it connects to the API surface.

## Stop 3: Where requests come in
Trace one end-to-end request through a controller or realtime hub reachable from the entry points above. The controller in [Code/src/api/Gabriel.API/Controllers/AuthController.cs.md](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) provides a compact example of how HTTP requests are accepted, validated, and forwarded to application services, so follow its methods to see routing, model binding, and the first hops into the Core/Infrastructure layers.

## Stop 4: Where the business logic lives
These workhorses are reached transitively from the ingress layer — services, agents, and pipeline steps doing the actual work behind the API surface. Compare the service interface [Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md](Code/src/api/Gabriel.Core/Identity/IJwtTokenService.cs.md) with its concrete implementation [Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md](Code/src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs.md) to see the Core/Infrastructure separation for identity concerns, and read [Code/src/api/Gabriel.Engine/Services/AgentService.cs.md](Code/src/api/Gabriel.Engine/Services/AgentService.cs.md) to observe how engine-level services orchestrate work and call into lower-level utilities.

## Stop 5: Where to put new code
Conventional homes for each kind of contribution, computed from the modal folder per role across the whole codebase (not just the reachable subgraph). Use [Code/src/api/Gabriel.API/Controllers/AuthController.cs.md](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) as the canonical place for HTTP surface changes, put domain orchestration and cross-cutting logic into services such as [Code/src/api/Gabriel.Core/Services/ChatService.cs.md](Code/src/api/Gabriel.Core/Services/ChatService.cs.md), keep persistence abstractions in repository interfaces like [Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md](Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md), and add engine tools or utilities under the Engine tool folders exemplified by [Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md](Code/src/api/Gabriel.Engine/Tools/Files/FileInfoTool.cs.md).

The pieces fit together in a layered collaboration: the architecture overview shows the system boundaries and where state belongs, the API and webapp entry points wire up DI and middleware and start the hosts, controllers like [Code/src/api/Gabriel.API/Controllers/AuthController.cs.md](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md) accept requests and delegate to Core/Engine services, and those services (for example the token interfaces and implementations and the Engine's AgentService) contain the business logic. New contributions should follow the convention mapping above so wiring (Program/App), surface (Controllers), domain (Core services/interfaces), and implementations (Infrastructure/Engine) remain cleanly separated.

## Next steps
Run the development servers locally by starting the API (see [Code/src/api/Gabriel.API/Program.cs.md](Code/src/api/Gabriel.API/Program.cs.md)) and the webapp (see [Code/src/webapp/src/main.tsx.md](Code/src/webapp/src/main.tsx.md)) so you can exercise the system while you read the linked files.

---
*Synthesised by Aurion on 2026-06-09 03:24:14 UTC*
