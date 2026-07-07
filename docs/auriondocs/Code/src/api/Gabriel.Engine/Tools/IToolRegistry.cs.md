# IToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/IToolRegistry.cs`  
> **Kind:** interface

```csharp
public interface IToolRegistry
```


IToolRegistry is an abstraction that collects and exposes the available tools to a consuming provider. It exposes All as a read-only list of ITool, enabling inspection and enumeration of capabilities; Find allows optional lookup by tool name, returning null if the tool is not present; AsDescriptors projects the registry into a provider-facing list of ToolDescriptor values, which is the shape sent to the language model via the tools parameter.

## Remarks
IToolRegistry serves as a boundary between the runtime implementations of tools and the consumer that presents them to the language model. The distinction between ITool (dynamic, executable tools with metadata) and ToolDescriptor (a lightweight descriptor) allows swapping the underlying registry, caching results, or filtering tools without affecting callers.

## Notes
- Find may return null when a tool with the given name is not registered; callers should handle the absence gracefully.