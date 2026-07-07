# TextTransformTool

> **File:** `src/api/Gabriel.Engine/Tools/Strings/TextTransformTool.cs`  
> **Kind:** class

```csharp
public sealed partial class TextTransformTool : ITool
```


Transforms the case or shape of a piece of text according to a small, fixed set of operations (upper, lower, title, sentence, snake, camel, pascal, kebab, slug, trim). Use this tool when you need to convert an identifier or label between naming conventions or normalize user-provided text exactly and reproducibly instead of doing ad-hoc string edits.

## Remarks
This class is a pure, side-effect-free transformer: it accepts a JSON argument object with "text" and "op" properties and returns the transformed string. It is word-boundary aware — spaces, underscores, hyphens and camelCase transitions are treated as word boundaries so any input form can be converted to any output form (e.g., snake_case ↔ camelCase ↔ kebab-case). The "slug" operation produces an ASCII-safe, URL-friendly form; other ops retain Unicode letters. The tool enforces a maximum input length (100,000 characters) and validates the JSON arguments before transforming.

## Example
```csharp
// Call ExecuteAsync with a small JSON payload. The tool returns the transformed text (or an "Error: ..." string on validation failure).
var tool = new TextTransformTool();

string args = "{ \"text\": \"ExampleInputValue\", \"op\": \"snake\" }";
string result = await tool.ExecuteAsync(args, CancellationToken.None);
// result => "example_input_value"

args = "{ \"text\": \"  hello   WORLD  \", \"op\": \"trim\" }";
result = await tool.ExecuteAsync(args, CancellationToken.None);
// result => "hello WORLD"

args = "{ \"text\": \"Hello world. this is fun.\", \"op\": \"sentence\" }";
result = await tool.ExecuteAsync(args, CancellationToken.None);
// result => sentence-casing behavior (capitalizes sentences) as implemented by the tool
```

## Notes
- The tool expects a JSON object with string properties "text" and "op"; missing or invalid types are reported as an error string (ExecuteAsync returns "Error: <message>").
- Input is limited to 100,000 characters; longer inputs produce a validation error.
- "slug" is ASCII-focused and intended for URL slugs (non-ASCII characters are removed or normalized by the slug logic). The other operations preserve Unicode letters.
- "title" uses the invariant-culture TextInfo.ToTitleCase applied to a lower-cased input; results follow .NET's TitleCase behavior and may not be language-specific.
- Word tokenization treats spaces, underscores, hyphens and camelCase boundaries as split points; this is why conversions between programmer cases and natural-case forms are reliable.
