# ITool

> **File:** `src/api/Gabriel.Engine/Tools/ITool.cs`  
> **Kind:** interface

```csharp
public interface ITool
```


ITool defines the pluggable tool contract used by Gabriel.Engine. Each tool exposes a Name and a human-readable Description, and provides a JSON schema describing the Arguments it accepts via ParametersJsonSchema. Implementations are registered via dependency injection and discovered by IToolRegistry, enabling the engine to enumerate and invoke tools by name. At runtime, ExecuteAsync executes the tool's logic asynchronously, consuming a JSON-encoded payload and returning a string observation (or error) for the caller to interpret.

## Remarks
This interface serves as a clean boundary between the host and runtime tools. By requiring a JSON schema for arguments, it enables dynamic validation and UI generation without coupling to concrete payload types. Tools can be added, replaced, or reconfigured at runtime through the DI container and registry, supporting extensibility and testability.

## Notes
- Tools are discovered via IToolRegistry, so adding new tools typically requires only a new ITool implementation and registration in DI, without changing the engine core.
- ExecuteAsync must honor the provided CancellationToken and should robustly handle malformed or missing arguments by relying on the declared ParametersJsonSchema.
