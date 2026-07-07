# Base64Tool

> **File:** `src/api/Gabriel.Engine/Tools/Codecs/Base64Tool.cs`  
> **Kind:** class

```csharp
public sealed class Base64Tool : ITool
```


Base64Tool is a sealed, stateless utility that encodes text to Base64 or decodes Base64 back to text. It is a pure function of its inputs with no I/O, designed to be used when you need reliable Base64 operations on UTF-8 data (for transport or inspection) rather than hand-rolling the encoding.

## Remarks
Base64Tool centralizes Base64 behavior behind a small, well-defined surface. By enforcing input validation (non-empty text and a configurable maximum length) and consistent error signaling through Base64Exception, it remains easy to test and reason about, while remaining completely agnostic to I/O concerns. It fits alongside other codec-like tools in the toolbox, enabling reuse and predictable behavior across the codebase.

## Example
```csharp
// Encode text to Base64
var tool = new Base64Tool();
var json = "{\"text\": \"Hello, world!\", \"op\": \"encode\"}";
var result = await tool.ExecuteAsync(json, CancellationToken.None);
// result starts with: "Encoded: "
```

## Notes
- 'text' cannot be empty and must be at most 100000 characters.
- When encoding, set url_safe = true to emit URL/filename-safe Base64 without padding.
- Decoding accepts both standard and URL-safe Base64 inputs.
- The wrapper returns human-friendly strings like "Encoded: ..." or "Decoded: ..."; parse the payload accordingly rather than relying on exact textual formatting.