# Program

> **File:** `src/api/Gabriel.API/Program.cs`  
> **Kind:** file

Program is the application entry point and host bootstrap for the Gabriel.API web application. It builds and configures the WebApplication host: sets up logging (including a lightweight bootstrap logger and a full Serilog pipeline using the "Serilog" configuration section), pulls secrets from Infisical before DI and options binding, registers framework services (controllers, Swagger, ProblemDetails, CORS), and wires application-wide middleware such as a global exception handler and a global route prefix.

## Remarks
This file centralizes cross-cutting startup concerns so the rest of the codebase can depend on configured services and logging being available at runtime. The bootstrap logger captures errors that occur before the configured Serilog pipeline is built (configuration loading, secret retrieval, container startup), while the full Serilog configuration (read from appsettings) controls runtime sinks, levels, and enrichers. Infisical secrets are retrieved early so configuration-bound options and service registrations see live secret values. The Program setup also enforces application conventions (for example, auto-prefixing controller routes with /api) so controllers remain simple and consistent.

## Notes
- The bootstrap Serilog logger is console-only and intended only to capture startup-time events; once the host builds, the configured Serilog pipeline (from the "Serilog" appsettings section) replaces it. Ensure your deployment includes a valid Serilog configuration to get the intended sinks and level overrides.
- Infisical secrets are pulled before service registration and IOptions bindings. Moving that code later will cause configuration-bound values to miss secrets and fall back to unset or default values.
- The GlobalRoutePrefixConvention automatically prefixes all controller routes with /api. Avoid duplicating the "api" segment in individual controller Route attributes to prevent routes like /api/api/… from being created.