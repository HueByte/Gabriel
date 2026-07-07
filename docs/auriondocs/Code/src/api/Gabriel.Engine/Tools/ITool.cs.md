# ITool

> **File:** `src/api/Gabriel.Engine/Tools/ITool.cs`  
> **Kind:** interface

```csharp
public interface ITool
```


ITool defines the contract for pluggable tooling within Gabriel.Engine. Implementations register via dependency injection and are discovered by IToolRegistry, enabling dynamic extension of capabilities. Each tool exposes a Name and Description, a ParametersJsonSchema describing the JSON arguments accepted by ExecuteAsync, and an asynchronous ExecuteAsync(string argumentsJson, CancellationToken ct) that runs the operation and returns a string result or error.

## Remarks
ITool serves as a thin abstraction that decouples tool implementation from how tools are invoked, enabling a plugin-like architecture. The ParametersJsonSchema enables dynamic validation and tooling documentation, allowing callers to construct valid argument payloads without embedding argument shapes in the caller code.

## Notes
- Tool implementations should be thread-safe if the engine may invoke them concurrently.
- The ParametersJsonSchema must be valid JSON Schema and accurately describe the accepted arguments to avoid runtime errors.
- ExecuteAsync should respect the CancellationToken to support cooperative cancellation.