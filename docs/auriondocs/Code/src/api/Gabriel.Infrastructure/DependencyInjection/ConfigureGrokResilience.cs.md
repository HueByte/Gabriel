Configures the resilience options for the Grok HTTP-based streaming pipeline by tying both the total request timeout and the per-attempt timeout to a shared totalTimeout value, and by widening the circuit-breaker sampling window accordingly. This aligns with streaming SSE chat workloads where a long-running generation would be prematurely terminated by default timeouts; retries are still restricted to the phase before the response stream starts so transient network or DNS issues can be recovered before tokens begin flowing.

## Remarks
By centralizing these settings in this helper, the DI configuration can adjust streaming allowances in one place without modifying each call site. It also clarifies the intended policy: allow longer streaming to complete within totalTimeout while granting early protection via retries and circuit-breaker checks.

## Notes
- TotalTimeout governs both overall and per-attempt windows; changing totalTimeout affects how quickly failures are surfaced.
- This method is private; changes require updating the DI configuration context rather than external usage.