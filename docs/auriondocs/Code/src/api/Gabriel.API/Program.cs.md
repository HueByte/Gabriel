# Program

> **File:** `src/api/Gabriel.API/Program.cs`  
> **Kind:** file


Program is the application’s entry point for the Gabriel API. It orchestrates the web host startup by configuring Serilog as the primary logging pipeline, loading configuration and secrets at bootstrap time, and wiring essential services and middleware. From loading Infisical secrets early to bind InfisicalOptions and AuthOptions, to applying a global API prefix and Swagger, this file establishes the cohesive startup sequence that all HTTP requests flow through before the app begins handling traffic.

## Remarks
Centralizes startup choreography: logging bootstrap, secret provisioning, config binding, and middleware registration all in one place. The early Infisical load ensures subsequent services see real-time config values, while the global API prefix enforces consistent routing across controllers. Swapping the logging pipeline to Serilog at startup decouples early startup diagnostics from the rest of the app's logging, improving observability during deploys.

## Notes
- The bootstrap logger runs before the host's logging pipeline is built, so startup errors are captured in the console regardless of later configuration.
- The FileLog settings are optional; if the FileLog section is missing, default values are used for path, template, and file size limits.
- Global route prefixing ("api") changes all controller routes; to opt-out, remove the GlobalRoutePrefixConvention usage.
