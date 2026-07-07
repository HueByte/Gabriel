# Program

> **File:** `src/api/Gabriel.API/Program.cs`  
> **Kind:** file


Program.cs is the application entry point for Gabriel.API. It bootstraps Serilog early to capture startup events, constructs the ASP.NET Core WebApplication, and wires the hosting environment, DI services, and middleware that shape the API. The file loads secrets from Infisical before service registration, binds InfisicalOptions, and configures AuthOptions so those values are available to the rest of the app. It sets up a global API prefix for controllers, enables Swagger for API exploration, adds ProblemDetails and a custom GlobalExceptionHandler to unify error responses, and configures Cross-Origin Resource Sharing based on configuration. It also defines a per-day log file sink using Serilog's Map sink, ensuring log rotation and shared file access during runtime. In short, this symbol is the centralized bootstrap that determines how the application starts, how secrets are resolved, how logging is configured, and how global API conventions are applied.