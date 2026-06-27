Registers chat providers into the application's dependency-injection container based on the provided configuration. This method always adds a MockChatProvider as a safe default and conditionally configures the Grok provider (options, validation, auth handler and a named HttpClient) when a Providers:Grok section exists and contains at least one model. Use this during host startup when composing IServiceCollection from IConfiguration so chat backends are discovered and validated at startup.

## Remarks
This method enforces a few architectural rules: the Mock provider is always present to act as a safety-net (prevents "no provider available" failures and provides a dev fallback), while each real provider is driven entirely from a named configuration section (so secrets and per-provider settings can be supplied via environment variables or a secrets store independently). For Grok specifically it binds strongly-typed options, applies multiple validators (API key presence, BaseUrl format, positive timeout, model shape and at-most-one active default), and wires an authentication DelegatingHandler plus a named HttpClient consumed by GrokChatProvider. ValidateOnStart is used to catch misconfiguration early, but it's skipped when SKIP_DB_INIT=="true" to avoid failing codegen or other non-runtime host builds that don't have secrets available.

## Example
```csharp
// In Program.cs or Startup when building the host
var builder = WebApplication.CreateBuilder(args);
AddChatProvider(builder.Services, builder.Configuration);

// Example environment variable for the Grok API key (provider sections use __ for nesting):
// PROVIDERS__GROK__APIKEY=sk-... (or use your secrets manager such as Infisical)
```

## Notes
- SKIP_DB_INIT environment variable: when set to "true" this method will avoid ValidateOnStart for the Grok options. This is intentional to allow build-time operations (e.g., swagger/codegen) that lack runtime secrets to complete.
- At-most-one default model: Grok options validation enforces that the Models collection contains at most one entry with IsActive=true. Zero active defaults is allowed and handled by the broader model catalog fallback.
- DelegatingHandler lifetime: the Grok authentication handler is registered transient because handlers added via AddHttpMessageHandler must be transient and are recreated per HttpClient.
- HttpClient timeout: the configured HttpClient intentionally leaves HttpClient.Timeout infinite; the code relies on a captured TimeoutSeconds value and a resilience pipeline (e.g., Polly) to enforce request timeouts instead of HttpClient.Timeout.
- Validation checks performed at startup include: ApiKey presence, BaseUrl as an absolute URL, TimeoutSeconds > 0, non-empty model Name and ContextWindowTokens > 0. These validations surface configuration mistakes early (unless skipped by SKIP_DB_INIT).