# Base64Tool

> **File:** `src/api/Gabriel.Engine/Tools/Codecs/Base64Tool.cs`  
> **Kind:** class

```csharp
public sealed class Base64Tool : ITool
```


Base64Tool is a pure function that encodes text to Base64 or decodes a Base64 string back to text, operating strictly on its arguments with no I/O or external dependencies. It treats input as UTF-8, enforces a maximum length, and supports URL-safe encoding when requested while decoding accepts both standard and URL-safe inputs.

## Remarks
Base64Tool serves as a focused, dependency-free utility that encapsulates the common Base64 encode/decode workflow used by higher-level tooling. It provides consistent validation and error reporting through Base64Exception and a uniform ExecuteAsync entry, enabling predictable composition with other tools. This isolation prevents ad-hoc encoding logic scattered across the codebase and makes behavior consistent for both UTF-8 text and URL-safe variants.

## Example
```csharp
var tool = new Base64Tool();
var json = "{\"text\":\"sample text\",\"op\":\"encode\"}";
var result = await tool.ExecuteAsync(json, CancellationToken.None);
// result starts with "Encoded: "
```

## Notes
- 'text' cannot be empty and is limited to a maximum of 100000 characters; violations throw Base64Exception. 
- Decoding accepts both standard and URL-safe input; encoding can emit the URL-safe alphabet when url_safe is true. 
- Results are returned as a string prefixed with either "Encoded: " or "Decoded: " to indicate the operation performed.