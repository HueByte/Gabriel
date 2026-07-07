# IToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/IToolRegistry.cs`  
> **Kind:** interface

```csharp
public interface IToolRegistry
```


IToolRegistry defines a centralized registry of tools available to the engine. It exposes a read-only collection of all registered tools, a name-based lookup, and a projection into provider-facing descriptors. Use it when you need to enumerate tools, retrieve a specific tool by name, or convert the registry into a list of ToolDescriptor objects to send to a consumer such as an LLM.

## Remarks
IToolRegistry acts as a façade between internal ITool implementations and the provider-facing consumer. By exposing AsDescriptors, it offers a lightweight ToolDescriptor view that external components can rely on without coupling to concrete ITool details; this separation lets the engine evolve its internal tool model while preserving a stable external contract.

## Example
```csharp
// Most common usage: present descriptor metadata to the provider
IReadOnlyList<ToolDescriptor> descriptors = toolRegistry.AsDescriptors();

// Optionally look up a specific tool by name
ITool? tool = toolRegistry.Find("SpellCheck");
```

## Notes
- Find returns null if no tool with the given name exists; callers must null-check.
- AsDescriptors reflects the registry state at invocation time; if tools are added or removed, refresh by calling again.