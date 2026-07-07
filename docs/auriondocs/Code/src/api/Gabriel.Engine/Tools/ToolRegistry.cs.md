# ToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/ToolRegistry.cs`  
> **Kind:** class

```csharp
public class ToolRegistry : IToolRegistry
```


ToolRegistry is an in-memory implementation of IToolRegistry that aggregates a set of ITool instances, exposes them as a read-only collection, supports fast, case-insensitive lookup by tool name, and can produce ToolDescriptor metadata for all registered tools. It builds an internal dictionary keyed by tool.Name using a case-insensitive comparer and stores the tools in All for enumeration.

## Remarks
ToolRegistry centralizes access to tooling components without requiring callers to manage the lifetime or storage of ITool instances. It keeps the registry lightweight by materializing the input sequence into a list and deriving the lookup map from that snapshot. The AsDescriptors method provides a ready-made set of ToolDescriptor entries (name, description, and parameter schema) that editors or UIs can consume to present available tools to users.

## Example
```csharp
// Typical usage
IEnumerable<ITool> tools = LoadTools();
var registry = new ToolRegistry(tools);

var tool = registry.Find("deploy"); // case-insensitive
if (tool != null)
{
    // use tool
}

IReadOnlyList<ToolDescriptor> descriptors = registry.AsDescriptors();
```

## Notes
- Duplicate tool names differing only by case will cause an exception during construction when building _byName via ToDictionary with OrdinalIgnoreCase.
- Find returns null if no matching tool exists; callers should guard accordingly.