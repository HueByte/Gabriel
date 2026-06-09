# ToolRegistry

> **File:** `src/api/Gabriel.Engine/Tools/ToolRegistry.cs`  
> **Kind:** class

Stores a collection of ITool instances and provides fast, case-insensitive lookup by tool name plus a way to project the registry into lightweight ToolDescriptor objects. Reach for ToolRegistry when you need a central, read-only registry of available tools (for example at application startup) instead of manually searching or re-filtering the tool collection each time.

## Remarks
ToolRegistry takes an `IEnumerable<ITool>` at construction, materializes it into a list and builds a Dictionary keyed by Name using StringComparer.OrdinalIgnoreCase for name lookups. It is intended as a small, immutable-at-runtime registry: once constructed the set of tools and their lookup mapping do not change, making it suitable for reuse throughout the application (for lookups, showing available tools, or exposing metadata).

## Example
```csharp
// create tools (ITool implementations assumed)
var tools = new ITool[] { new EchoTool(), new SummationTool() };

var registry = new ToolRegistry(tools);

// find a tool by name (case-insensitive)
var echo = registry.Find("echo");

// get descriptors for UI/metadata
var descriptors = registry.AsDescriptors();
```

## Notes
- Passing duplicate tool names to the constructor will throw when ToDictionary is called (name collisions). Ensure tool names are unique.
- If the `tools` argument is null the constructor will throw (it calls ToList() without a null check).
- The All property is exposed as `IReadOnlyList<ITool>`, but the registry internally holds a `List<T>` from ToList(); callers should not rely on runtime immutability beyond the public read-only surface (avoid casting to `List<T>` and mutating).
- After construction the registry is safe for concurrent reads; it does not provide any API to modify the collection at runtime.
