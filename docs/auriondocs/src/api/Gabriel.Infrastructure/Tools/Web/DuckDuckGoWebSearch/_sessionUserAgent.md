Stores the session-scoped User-Agent string for this DuckDuckGoWebSearch instance. The field is nullable and private; when null it indicates no explicit user-agent has been recorded for the current session.

## Remarks
This private field acts as the single location inside the class where a per-instance user-agent value can be cached and reused for outgoing requests. External code cannot access it directly — any interaction with the session user-agent should go through the class's public API.

## Notes
- The field is nullable: callers within the class must check for null before using the value.
- Access is not synchronized by the field itself; if the containing class is used concurrently, reads/writes should be protected by the class's synchronization strategy.
- Because it is a private mutable field, prefer using the class's public methods or properties to modify the session user-agent rather than altering it directly.