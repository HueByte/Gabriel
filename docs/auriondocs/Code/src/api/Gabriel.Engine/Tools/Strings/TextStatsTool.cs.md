# TextStatsTool

> **File:** `src/api/Gabriel.Engine/Tools/Strings/TextStatsTool.cs`  
> **Kind:** class

```csharp
public sealed partial class TextStatsTool : ITool
```


TextStatsTool is a deterministic utility that measures a block of text and reports basic statistics: characters, words, lines, sentences, and paragraphs, plus an estimated reading time and a rough token count. Use it when you need an exact, reproducible sense of how long a piece of text is, rather than relying on eyeballing or ad-hoc estimates. The tool treats Unicode code points as characters, so emoji and CJK characters are counted correctly. It is not designed for hashing or arithmetic operations.

## Remarks
TextStatsTool encapsulates text-metric logic behind an ITool interface, making it easy to reuse in UI diagnostics, content editors, or tooling that needs quick, offline text measurements. It operates purely on the input text and simple heuristics, with no external I/O, which helps ensure consistent results across call sites. The presence of the JSON-based input schema and the internal validation means callers can rely on predictable error messages when input is invalid or exceeds the allowed size.

## Example
```csharp
using System.Threading;

// Example usage
var tool = new TextStatsTool();
var json = "{\"text\":\"Hello world! This is a sample text to illustrate counting. It has multiple sentences.\"}";
var result = await tool.ExecuteAsync(json, CancellationToken.None);
Console.WriteLine(result);
```

## Notes
- The tool expects a JSON object with a single string property named "text". Missing, non-string, empty, or oversized input yields an error string (e.g., "arguments were not valid JSON." or "'text' cannot be empty.").
- Sentence and paragraph counts are heuristic; if no sentences are detected but words exist, it reports one sentence. If no paragraphs are detected but words exist, it reports one paragraph. Reading time uses a fixed WordsPerMinute baseline (200 WPM) to produce a rough estimate. 
- The estimated token count is a naive ceil(chars / 4) approximation and is not intended for cryptographic or exact-token purposes.