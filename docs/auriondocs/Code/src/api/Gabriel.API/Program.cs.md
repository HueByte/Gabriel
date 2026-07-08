# Program

> **File:** `src/api/Gabriel.API/Program.cs`  
> **Kind:** file


Program is the application's entry point that bootstraps and runs the ASP.NET Core host. It centralizes startup concerns—logging, configuration (including Infisical-sourced secrets), service registration, and middleware wiring—so developers modify startup behavior in one place rather than scattering it across the app.

## Remarks
This symbol acts as the bootstrapping nucleus, ensuring the host is properly configured before any application code runs. By loading Infisical secrets early, it guarantees configuration bindings have live values when services are constructed, and by wiring Serilog early, it ensures startup logs are captured through the full pipeline. It also centralizes the ordering of infrastructure concerns (controllers, OpenAPI/Swagger, problem details, exception handling, and CORS), keeping startup logic isolated from business logic.

## Notes
- The bootstrap logger runs before the host's logging pipeline is built; environments without a console or proper stdout redirection may not capture these logs.
- Secrets loaded from Infisical at startup mean a misconfiguration or network issue can prevent the app from starting or supply incorrect values.
- Startup changes require restarting the application; this file is the single point to adjust how the host is configured and started.