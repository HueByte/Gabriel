Configures a standard resilience policy tuned for Grok's server-sent-event (SSE) chat streams. Use this when wiring HttpStandardResilienceOptions for the Grok provider so streaming responses are not cut off by the default short per-attempt timeout; the caller should supply a total timeout derived from Providers:Grok:TimeoutSeconds.

## Remarks
This method aligns the attempt timeout with the overall request timeout (so a single attempt can run for the configured total duration) and expands the circuit-breaker sampling window to satisfy the framework validation (SamplingDuration >= 2 * AttemptTimeout) and to reduce aggressive tripping during long-running streams. Retries are intentionally left at their default behavior because retries only apply before the response stream starts — once tokens begin flowing the pipeline allows the active attempt to continue for the full timeout.

## Example
```csharp
// Example during DI setup: read the configured timeout and apply the resilience tuning
var grokTimeoutSeconds = configuration.GetValue<int>("Providers:Grok:TimeoutSeconds");
var totalTimeout = TimeSpan.FromSeconds(grokTimeoutSeconds);
ConfigureGrokResilience(httpStandardResilienceOptions, totalTimeout);
```

## Notes
- Setting AttemptTimeout equal to the total timeout prevents per-attempt preemption of long streaming responses but means a single attempt may run for the entire duration.
- The circuit-breaker SamplingDuration is doubled relative to the timeout; this makes the breaker less sensitive (takes longer to accumulate failure samples), which is desirable for long-lived streams but delays detection of repeated quick failures.
- Ensure the supplied totalTimeout comes from the provider configuration (Providers:Grok:TimeoutSeconds); an undersized value will still terminate lengthy generations mid-stream.