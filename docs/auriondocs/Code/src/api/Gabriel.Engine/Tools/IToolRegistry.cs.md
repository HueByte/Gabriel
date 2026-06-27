# IToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/IToolRegistry.cs`  
> **Kind:** interface

Represents a centralized registry of available ITool instances and provides ways to query them and produce provider-facing descriptors. Use this abstraction when you need a single source of truth for available tools or when preparing the list of tools to send to an LLM provider (via AsDescriptors).

## Remarks
The registry decouples tool implementations from the transport/serialization format required by a provider: callers interact with ITool instances for execution and use AsDescriptors to obtain a provider-friendly representation (ToolDescriptor) that is safe to serialize and send as the `tools` parameter. Implementations may back the registry with in-memory collections, dynamic discovery, or plugin loading.

## Example
```csharp
// Lookup and use a tool by name
var tool = registry.Find("web-search");
if (tool != null)
{
    var result = tool.Execute("latest news about X");
    Console.WriteLine(result);
}

// Prepare descriptors to include in a provider request
var descriptors = registry.AsDescriptors();
// `descriptors` can now be serialized and sent to an LLM as the `tools` parameter
```

## Notes
- All exposes an IReadOnlyList; callers should not assume mutability or ordering.  
- Find returns null when no tool matches the provided name; handle nulls accordingly.  
- AsDescriptors produces a provider-facing projection of the current registry state; if the registry is mutable, callers may need to refresh descriptors before sending them to the provider.  
- Thread-safety and name-matching semantics (case sensitivity, normalization) are implementation-defined—do not rely on specific behavior unless documented by the concrete implementation.