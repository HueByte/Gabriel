# TextStatsTool

> **File:** `src/api/Gabriel.Engine/Tools/Strings/TextStatsTool.cs`  
> **Kind:** class

```csharp
public sealed partial class TextStatsTool : ITool
```


TextStatsTool exposes a compact, deterministic way to measure a block of text by returning a human-readable summary of characters, words, lines, sentences, and paragraphs, plus an estimated reading time and token rough count. Use it when you need a quick, Unicode-aware text size metric (for previews, editors, or content tooling) instead of counting manually, and note that sentence/paragraph counts are heuristic.

## Remarks
TextStatsTool is a pure utility that computes its metrics via a straightforward, heuristic approach and delegates linguistic splits to underlying helpers (Words, Sentences, CountParagraphs). It validates input and enforces a maximum length (1,000,000 characters) to avoid unbounded work, returning an error string when input is invalid rather than throwing. The output is a single, human-readable summary string suitable for display in dashboards or quick QA checks, and the description explicitly notes that it is not intended for hashing or arithmetic.

## Notes
- Characters are counted as Unicode code points; emoji and CJK characters contribute a single unit per code point, which may differ from user-perceived grapheme clusters in edge cases.
- Sentence and paragraph counts are heuristic; use this tool for rough sizing rather than exact linguistic analysis.
