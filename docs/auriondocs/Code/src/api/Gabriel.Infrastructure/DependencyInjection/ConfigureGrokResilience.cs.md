Configures the HTTP resilience options for the Grok streaming path, aligning total and per-attempt timeouts to a single totalTimeout value. It sets TotalRequestTimeout.Timeout and AttemptTimeout.Timeout to totalTimeout, and expands CircuitBreaker.SamplingDuration to twice the total timeout. This prevents long-running SSE-like generation from being cut off mid-stream and ensures the circuit-breaker window covers the entire attempt.

## Remarks
Acts as a domain-specific adapter that centralizes Grok's resilience policy. By encapsulating the timeout semantics in one place, it reduces duplication and ensures consistency with the Providers:Grok:TimeoutSeconds setting. It complements the streaming nature of the Grok path, where tokens flow after the initial request, making mid-stream timeouts particularly disruptive.

## Notes
- This method mutates the provided HttpStandardResilienceOptions; call it during application startup to configure a shared policy, not per-request.