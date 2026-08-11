# TextTransformTool

> **File:** `src/api/Gabriel.Engine/Tools/Strings/TextTransformTool.cs`  
> **Kind:** class

```csharp
public sealed partial class TextTransformTool : ITool
```


Transform the case or general shape of a string value according to a small set of well-known operations (upper, lower, title, sentence, snake, camel, pascal, kebab, slug, trim). Use this tool when you need to convert an identifier or label between conventions (for example, snake_case -> camelCase, or free-form text -> a URL-friendly slug) rather than performing character-level edits manually.

## Remarks
This tool is a pure, self-contained transformer: it validates its JSON arguments, enforces a maximum input length (100,000 characters), and performs no I/O. Word boundary detection is aware of spaces, underscores, hyphens and camelCase humps so mixed-format inputs convert cleanly between programmer-case forms and natural-language forms. Most operations preserve Unicode letters; the "slug" operation produces an ASCII-only form. When argument validation fails the implementation throws TextTransformException (ExecuteAsync catches that and returns an error string).

## Example
```csharp
var tool = new TextTransformTool();
// async/await style
var resultTask = tool.ExecuteAsync("{\"text\": \"example_text_here\", \"op\": \"camel\"}", CancellationToken.None);
string transformed = await resultTask; // "exampleTextHere"

// or synchronous blocking (not recommended in UI code):
string transformedSync = tool.ExecuteAsync("{\"text\": \"Hello World!\", \"op\": \"kebab\"}", CancellationToken.None).Result; // "hello-world"
```

## Notes
- The tool requires a JSON object with string properties "text" and "op"; missing/invalid JSON or a missing/empty "text" will cause a TextTransformException which ExecuteAsync converts into a returned string prefixed with "Error:".
- Input length is capped at 100,000 characters; longer inputs are rejected with an error.
- "title" uses CultureInfo.InvariantCulture.TextInfo.ToTitleCase (after lowercasing), so casing is invariant-culture based rather than locale-specific. "slug" intentionally drops non-ASCII characters to produce a URL-friendly token.