Encode or decode text using standard or URL/filename-safe Base64 (text is treated as UTF-8). Reach for this ITool when you need a reliable, self-contained Base64 converter (for example to inspect an encoded token or to prepare text for transport) instead of implementing Base64 logic by hand. Use url_safe=true when you need the URL/filename-safe alphabet and no padding.

## Remarks
This class implements a pure, argument-driven tool: it accepts a JSON payload describing the operation and returns a string result (no I/O or external dependencies). It validates input against the expected schema (required string properties "text" and "op"; optional boolean "url_safe"), enforces a maximum text length (100,000 characters) and a non-empty text value, and maps any parsing/validation problems to a user-facing error message. The url_safe flag only affects encoding; decoding accepts both standard and URL-safe alphabets.

## Example
```csharp
// Encode text to URL-safe Base64
var tool = new Base64Tool();
var args = "{ \"text\": \"hello world\", \"op\": \"encode\", \"url_safe\": true }";
var encoded = await tool.ExecuteAsync(args, CancellationToken.None);
Console.WriteLine(encoded); // e.g. "Encoded: aGVsbG8gd29ybGQ" (no padding)

// Decode a Base64 string back to text
var args2 = "{ \"text\": \"aGVsbG8gd29ybGQ=\", \"op\": \"decode\" }";
var decoded = await tool.ExecuteAsync(args2, CancellationToken.None);
Console.WriteLine(decoded); // "Decoded: hello world"
```

## Notes
- ExecuteAsync returns a human-readable string prefixed with "Encoded: ", "Decoded: ", or "Error: "; callers that need the raw value should parse the prefix out.
- When encoding with url_safe=true the implementation emits the URL/filename-safe alphabet ('-' and '_') and omits '=' padding. Decoding accepts both alphabets and handles padded or unpadded input.
- Input JSON must include a non-empty "text" string and an "op" of either "encode" or "decode"; if present, "url_safe" must be a boolean.
- The tool enforces a maximum input text length of 100,000 characters and will return an error message if exceeded or if JSON is invalid.