# IToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/IToolRegistry.cs`  
> **Kind:** interface

```csharp
public interface IToolRegistry
```


IToolRegistry is a lightweight registry abstraction that enumerates and exposes available tools. Consumers can enumerate All to see every ITool, use Find to retrieve a tool by its name, or call AsDescriptors to translate the registry into ToolDescriptor objects that are sent to the LLM as the tools parameter. This abstraction decouples tool registration from tool consumption, enabling dynamic tool sets and a stable descriptor surface for provider integrations.

## Remarks
IToolRegistry serves as the discovery boundary between concrete tool implementations and the provider/LLM pipeline. It aggregates ITool instances and exposes a stable, serializable surface (ToolDescriptor) via AsDescriptors, which the provider uses to populate the LLM's tools payload. By offering both a full enumeration (All) and a name-based lookup (Find), it supports dynamic tool composition while keeping downstream consumers insulated from the concrete tool types.

## Notes
- Find(string) may return null if no tool with the given name exists; callers must handle potential absence.
- AsDescriptors reflects the registry's current state; call it when you need up-to-date tool metadata for the LLM.