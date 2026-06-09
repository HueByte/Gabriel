Registers chat providers into the DI container based on the application's IConfiguration. Always adds a singleton MockChatProvider as a safety-net/fallback; if a Providers:Grok section exists and contains at least one model the method configures and validates GrokOptions, optionally enables ValidateOnStart, registers the GrokAuthHandler, captures a startup timeout value, and configures a named HttpClient and resilience pipeline to be consumed by GrokChatProvider.

## Remarks
This method centralizes provider registration and guard-rails for third-party LLM/chat providers. The mock provider ensures the application never runs without a provider available (preventing runtime failures and providing a simple dev fallback). Provider-specific sections (like Grok) are bound via the standard Options pipeline and validated early so misconfiguration is discovered at host build time. ValidateOnStart is intentionally conditional to accommodate out-of-process codegen/migration runs that do not have secrets available.

## Example
```csharp
// Typical call from Startup/Program when wiring services:
AddChatProvider(services, Configuration);
```

## Notes
- ValidateOnStart is skipped when the SKIP_DB_INIT environment variable equals "true"; this prevents host-start validation from failing in environments (like codegen) where secrets aren't available.
- Grok options include several runtime validations (ApiKey non-empty, BaseUrl a valid absolute URL, TimeoutSeconds > 0, at most one model marked IsActive, and each model having a Name and ContextWindowTokens > 0). The ApiKey is expected to come from environment variables (e.g. PROVIDERS__GROK__APIKEY) or a secrets provider.
- Delegating handlers must be registered with transient lifetime; the method registers GrokAuthHandler as transient so AddHttpMessageHandler can resolve a fresh handler per HttpClient creation. The code leaves HttpClient.Timeout infinite and reads a provider TimeoutSeconds at startup (fallback 900s) so the configured resilience pipeline is the authoritative timeout mechanism.