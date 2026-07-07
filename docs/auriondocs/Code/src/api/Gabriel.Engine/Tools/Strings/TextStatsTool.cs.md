# TextStatsTool

> **File:** `src/api/Gabriel.Engine/Tools/Strings/TextStatsTool.cs`  
> **Kind:** class

```csharp
public sealed partial class TextStatsTool : ITool
```


TextStatsTool measures a given block of text, returning counts for characters, words, lines, sentences, and paragraphs, plus an estimated reading time and a rough token count. Use it when you need a precise, reproducible answer to how long a text is, rather than guessing; it is a pure function of the input text with no I/O or external dependencies.

## Remarks
TextStatsTool provides a centralized, deterministic analytics primitive used by editors, dashboards, or consoles to surface text length metrics consistently across the codebase. It counts characters as Unicode code points (so emoji and CJK characters count as one each) and uses simple heuristics to estimate sentences and paragraphs, ensuring behavior is predictable but not perfect for all languages. Because it is implemented as a small, self-contained tool implementing ITool, it can be composed with other tools in the Gabriel.Engine toolchain to create data-driven workflows.

## Example
```csharp
// Common usage: measure a short paragraph
var tool = new TextStatsTool();
var json = "{\"text\": \"Hello world. This is a sample sentence.\"}";
var result = await tool.ExecuteAsync(json, CancellationToken.None);
```

## Notes
- The token estimate is a rough heuristic and should not be used for precise tokenization or cryptographic purposes.
- The reading time uses a default 200 words-per-minute baseline and may vary by language, content, and reader.