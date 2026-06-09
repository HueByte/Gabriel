Sets up conservative resilience settings for Server-Sent Events (SSE) chat streams by aligning per-attempt and total request timeouts and widening the circuit-breaker sampling window. Use this when configuring an HTTP resilience pipeline for long-running SSE/chat responses so the pipeline does not cut off an in-progress generation.

## Remarks
This method intentionally makes the per-attempt timeout equal to the overall request timeout to avoid terminating an active streaming response mid-generation. It leaves retry behavior unchanged (retries occur only before the response stream begins) and expands the circuit-breaker sampling duration to at least twice the attempt timeout to satisfy validation rules and to give the circuit-breaker enough sample time for long attempts.

## Example
```csharp
// Configure timeouts using a configured timeout value (e.g. Providers:Grok:TimeoutSeconds)
var total = TimeSpan.FromSeconds(ProvidersGrokTimeoutSeconds);
ConfigureGrokResilience(opts, total);
```

## Notes
- Setting AttemptTimeout equal to totalTimeout means there is no shorter per-attempt timeout; if you want a shorter attempt-level timeout, adjust AttemptTimeout after this call.
- Retries will not interrupt an in-progress stream — they only apply before the response stream starts.
- The circuit-breaker sampling duration is calculated as totalTimeout * 2 via ticks multiplication; avoid extremely large totalTimeout values to prevent potential overflow when doubling ticks.
