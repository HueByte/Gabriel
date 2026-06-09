# DocsListTool

> **File:** `src/api/Gabriel.Engine/Tools/Docs/DocsListTool.cs`  
> **Kind:** class

```csharp
public sealed class DocsListTool : ITool
```


Lists the OFFICIAL Gabriel documentation pages and returns a human-readable text block an LLM or calling agent can present. Reach for this tool when you need to discover what documentation pages exist (it does not return page contents); after finding a path here, call the docs_read tool with the chosen path to fetch the page.

## Remarks
DocsListTool queries an IDocsLookup implementation to retrieve all available DocsEntry items, and formats them into a compact, LLM-friendly summary. Entries are grouped by their source and ordered using a built-in source priority so that LLM-native pages (DocsSources.LocalLlmNative) appear first and GitHub (DocsSources.GitHub) pages appear as a human-prose fallback. The tool never throws on lookup failures; it returns an error string instead, and returns a special message if no pages are available to encourage falling back to other search mechanisms.

## Example
```csharp
// Assume `docsLookup` implements IDocsLookup and is already configured
var tool = new DocsListTool(docsLookup);
string result = await tool.ExecuteAsync("{}", CancellationToken.None);
Console.WriteLine(result);
```

## Notes
- ParametersJsonSchema is an empty object: the tool expects no parameters (pass an empty JSON object).
- The returned value is a formatted plain-text block (not structured JSON); callers should present it directly to users or an LLM and then call docs_read with a selected `path` to fetch content.
- If the docs source is unreachable or an exception occurs during listing, the tool returns a prefixed error message rather than throwing.
- Unknown/third-party sources are placed last by priority (SourcePriority returns 99 for unrecognized sources).