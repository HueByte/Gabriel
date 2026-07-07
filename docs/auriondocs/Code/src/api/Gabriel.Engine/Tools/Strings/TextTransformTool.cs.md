# TextTransformTool

> **File:** `src/api/Gabriel.Engine/Tools/Strings/TextTransformTool.cs`  
> **Kind:** class

```csharp
public sealed partial class TextTransformTool : ITool
```


Transform the shape or case of a string into a different convention (upper, lower, title, sentence, snake_case, camelCase, PascalCase, kebab-case, URL-safe slug, or trimmed/collapsed whitespace). Use this tool when you need a deterministic, word-aware reformatting of an identifier or label instead of editing characters by hand; it is a pure, side-effect-free transformer.

## Remarks
Implements ITool as a stateless, pure transformer: no I/O and no hidden dependencies. Word boundaries are detected consistently across input forms — spaces, underscores, hyphens and camelCase humps all produce the same token stream — so any input form can be converted to any supported output form. Casing operations use the invariant culture; helper routines handle tokenization, capitalization, camel/pascal joining, whitespace collapsing, and ASCII slugification.

## Notes
- The tool expects JSON with a non-empty string "text" and an "op" from the supported set; invalid JSON or missing/invalid fields produce an error string. 
- Input length is limited to 100,000 characters; longer inputs result in an error. 
- ExecuteAsync catches argument/validation failures and returns an error message string beginning with "Error: "; callers should inspect the returned value rather than relying on exceptions. 
- Casing uses CultureInfo.InvariantCulture (culture-invariant behavior). 
- The "slug" operation is ASCII-only: non-ASCII characters are dropped and non-slug runs are replaced with hyphens. Other operations preserve Unicode letters.
- "trim" trims leading/trailing whitespace and collapses internal runs of whitespace to a single space.