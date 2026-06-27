# ToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/ToolRegistry.cs`  
> **Kind:** class

A simple, in-memory registry of ITool instances that provides quick lookup by tool name and exposes a snapshot of all registered tools and their public descriptors. Use this when you need a read-oriented collection of tools built once (at construction) to perform case-insensitive name lookups or to enumerate tool metadata.

## Remarks
ToolRegistry takes an enumerable of ITool and captures it as a list at construction time, building an internal, case-insensitive dictionary keyed by each tool's Name. It purposefully provides a lightweight, read-focused abstraction: fast lookup via Find(name) and a way to produce ToolDescriptor objects for UI, discovery or API surface purposes via AsDescriptors(). This class is intended for scenarios where the set of tools is known at startup and does not need dynamic additions or removals.

## Example
```csharp
var tools = new ITool[] { new EchoTool(), new SummarizeTool() };
var registry = new ToolRegistry(tools);

// Find by name (case-insensitive)
var echo = registry.Find("echo");
if (echo != null)
{
    // use echo
}

// Get descriptors for discovery/UI
var descriptors = registry.AsDescriptors();
```

## Notes
- Duplicate or null tool names in the provided sequence will cause ToDictionary to throw during construction; ensure tool.Name is non-null and unique (ignoring case).
- Find performs a case-insensitive lookup (StringComparer.OrdinalIgnoreCase) so callers don't need to normalize name casing.
- All is a snapshot taken at construction (All is an IReadOnlyList backed by the internal list); mutating the original enumerable after construction does not affect the registry.