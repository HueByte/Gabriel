Registers chat providers into the application's dependency-injection container and configures the Grok provider when a Providers:Grok configuration section with at least one model is present. Always adds a MockChatProvider as a singleton (acts as a safety-net and a UI fallback). Use this during application startup to centralize provider wiring, validation, HTTP client setup, and authentication handler registration.

## Remarks
This method ensures the application always has a working provider (the Mock provider) and conditionally enables the Grok provider when configuration indicates it should be used. GrokOptions are bound via a named configuration section and are validated with several fail-fast checks (API key presence, valid BaseUrl, positive timeout, model shape and single default-model constraint). Validation is run at host build time via ValidateOnStart except when SKIP_DB_INIT is set to "true" (used to avoid failing environments such as code generation where secrets are not available). The method also captures the Grok timeout once at startup for use by the resilience pipeline, registers a transient GrokAuthHandler to apply bearer tokens, and registers a named HttpClient consumed by the GrokChatProvider; the bearer header is applied by the handler so key rotation does not require recycling the HttpClient.

## Example
```csharp
// inside Startup.ConfigureServices or equivalent host builder code
public void ConfigureServices(IServiceCollection services)
{
    // IConfiguration configuration is available from the host
    AddChatProvider(services, Configuration);
}
```

## Notes
- SKIP_DB_INIT = "true" skips ValidateOnStart so the host build won't fail when secrets (e.g., API keys) are not present (used by codegen/migrations).
- The Grok provider expects its ApiKey to be populated (e.g. env var PROVIDERS__GROK__APIKEY or via Infisical) — validation will fail host startup if missing unless SKIP_DB_INIT is set.
- Delegating handlers (GrokAuthHandler) must be registered transient when used with AddHttpMessageHandler; HttpClient.Timeout is intentionally left infinite because the resilience pipeline enforces timeouts.