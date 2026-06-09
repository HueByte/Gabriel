# Program

> **File:** `src/api/Gabriel.API/Program.cs`  
> **Kind:** file

Sets up the ASP.NET Core application's host pipeline and service registrations: initializes a bootstrap Serilog logger for early-stage diagnostics, configures the application's final Serilog pipeline (including a programmatic Map → File sink assembled from the "FileLog" configuration), pulls secrets from Infisical before registering services so configuration-bound options see live secrets, and registers common middleware and infrastructure such as controllers (auto-prefixed with /api), Swagger, ProblemDetails and a global exception handler, and CORS.

## Remarks
This file is the single place to change high-level startup behavior and diagnostics wiring for the API. The bootstrap logger captures errors that happen before the full logging configuration is available (configuration loading, secret fetches, container startup failures). The Serilog File sink is composed in code rather than purely in appsettings because the Map sink requires a non-JSON-bindable callback; the code reads FileLog settings (path template, output template, size limit) and assembles the sink at startup. Infisical secrets are fetched and bound before service registration so IOptions-bound services (for example, provider API keys or JWT signing keys) will be populated with the live secret values when the DI container is configured. The global route prefix removes the need to add an "api" prefix on every controller route attribute.

## Notes
- The bootstrap Serilog logger is temporary: once the host finishes building and the configured logger is created from configuration, it replaces the bootstrap logger. Rely on the final configured sinks for long-term log retention.
- The Map → File sink uses a key property named "LogDate" (with a provided default key). If log events do not include that property the default key will be used and file routing may not behave as expected; the file path template expects a single format placeholder (the code calls string.Format(path, date)).
- CORS origins are read from configuration at "Cors:AllowedOrigins" and default to an empty array in code; an empty list will effectively block cross-origin requests unless populated in configuration.