# Markdown.tsx

> **Source:** `src/webapp/src/components/Markdown.tsx`

## Contents

- [Props](#props)
- [Inline](#inline)
- [MarkdownImpl](#markdownimpl)
- [buildComponents](#buildcomponents)
- [expandText](#expandtext)
- [languageOf](#languageof)
- [remarkInlineEnrichments](#remarkinlineenrichments)
- [remarkUnliftEmptyOrderedList](#remarkunliftemptyorderedlist)
- [toAstNode](#toastnode)
- [toGalactic](#togalactic)

---

## Props
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** interface

```typescript
interface Props
```


Props is the interface that defines the shape of the properties passed to the Markdown rendering component. It carries the text to render and an optional streaming flag that indicates whether the parent typewriter is actively revealing characters. When streaming is true, the renderer disables expensive post-processing steps (syntax highlighting, KaTeX math rendering, and the mermaid diagram renderer) to avoid costly updates during per-tick mutations, snapping back to full rendering once streaming ends. If you render static history messages or other non-streaming content, you can omit the streaming flag.

## Remarks

The streaming flag encapsulates a performance-oriented concern: it communicates to the Markdown renderer when it should trade richer visual processing for streaming responsiveness. By centralizing this control in Props, the component remains agnostic about how the content is produced, while callers that stream text can rely on the renderer to defer heavy work until the stream completes. This separation helps keep interactive typing smooth without intertwining rendering logic with content generation.

## Example

```ts
const sample: Props = { text: "Loading…", streaming: true };
```

## Notes

- If streaming is omitted, the component follows a non-streaming rendering path where all features may render upfront.
- This flag is intended for live-typing scenarios; passing an inappropriate value may cause unnecessary postponement or premature rendering of features like rich plugins.

---

## Inline
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** type alias

```typescript
type Inline =
  | { type: 'text'; value: string }
  | { type: 'galactic'; value: string }
  | { type: 'hexChip'; value: string };
```


Inline defines the set of inline fragments used by the Markdown renderer in Markdown.tsx. It is a discriminated union with three variants: text, galactic, and hexChip. Each variant carries a string value that holds the content to be rendered.

## Remarks
Inline provides a type-safe way to represent mixed inline content. The discriminated union makes rendering exhaustive and extensible: you can handle each kind explicitly and add new variants in the future without changing the surrounding code path.

## Notes
- Adding a new variant requires updating all rendering branches and any serialization logic that consumes Inline[].
- Keep the meaning of each variant distinct (e.g., treat 'text', 'galactic', and 'hexChip' differently in rendering) to avoid ambiguous output.

---

## MarkdownImpl
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function MarkdownImpl(
```


MarkdownImpl is a React functional component that renders Markdown content provided via its text prop. The optional streaming flag, when true, enables progressive rendering as content becomes available; when false or omitted, the entire Markdown input is rendered in a single pass.

## Remarks
MarkdownImpl encapsulates the Markdown rendering behavior behind a simple, predictable API. It separates concerns by letting callers supply text content without worrying about the parsing or rendering details. The streaming option offers a way to improve perceived latency in scenarios where Markdown content is generated or loaded incrementally.

---

## buildComponents
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function buildComponents(streaming: boolean): Components
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `streaming` | `boolean` | — |

**Returns:** `Components`


Factory function buildComponents(streaming) returns a Components object that customizes how Markdown is converted to React elements for this app. It exposes renderers for anchors, horizontal rules, and code blocks, wiring in mermaid rendering, inline hex chips, and standard syntax-highlighted code, all controlled by the streaming flag to balance responsiveness with dynamic rendering.

## Remarks

By centralizing these renderers in one place, this abstraction isolates presentation concerns from content. Mermaid diagrams are only rendered when streaming has stopped to avoid heavy, incremental rendering during live reveal, and inline hex chips provide a compact visual cue for color values without altering the underlying text. The approach complements the surrounding Markdown rendering pipeline by ensuring consistent styling (md-link, md-hr, md-hex) across all blocks.

## Notes

- Mermaid rendering is gated by the streaming flag; if you rely on an instant diagram during streaming, it will not appear until streaming finishes.
- The code path that renders Mermaid diagrams relies on detecting language markers from className; if language detection fails, the block falls back to default rendering.
- Inline hex chips require a data-hex attribute on the code element to activate; without it, normal code rendering is used.

---

## expandText
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function expandText(value: string): RootContent[]
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `value` | `string` | — |

**Returns:** `RootContent[]`


Expands a plain string into a structured AST-like sequence by recognizing two inline constructs: galactic blocks delimited by GAL_OPEN and GAL_CLOSE, and hex escapes defined by HEX_RE. It starts from a single text stage containing the input value, then, if the galactic sentinels are configured, splits the text into galactic and text fragments. Galactic fragments become galactic nodes, and text around them remains as text. After that, every text fragment is scanned for hex escapes and transformed into hexChip nodes while preserving surrounding text. The resulting sequence of Inline pieces is finally mapped to RootContent nodes via toAstNode and returned. When GAL_OPEN and GAL_CLOSE are not configured (or are empty), galactic expansion is skipped entirely and the input is treated as plain text.


---

## languageOf
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function languageOf(className: string | undefined): string | null
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `className` | `string | undefined` | — |

**Returns:** `string | null`


Extracts a programming language tag from a code block's className by looking for a token that starts with language- and returns the captured language name, or null if none is found. It is useful when rendering or processing code blocks where the language is conveyed via a CSS class rather than a data attribute, allowing you to determine the appropriate syntax highlighting without hard-coding mappings.

## Remarks
This helper centralizes the language extraction logic behind a tiny, well-scoped regex. It tolerates multiple CSS classes and returns the first match found in the string. If the input is undefined or doesn't contain a language- token, it returns null rather than throwing; callers should handle the null case accordingly.

## Example
```typescript
// Common usage: extract language from className
languageOf("foo language-python bar"); // "python"

languageOf("language-js"); // "js"
```

## Notes
- Returns null if input is falsy or no language- token is present.
- If the input contains multiple language- tokens, only the first is used because the regex is executed once.

---

## remarkInlineEnrichments
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function remarkInlineEnrichments()
```


remarkInlineEnrichments is a Remark-compatible plugin factory that traverses Markdown text nodes and replaces inline text with enriched content produced by expandText. It deliberately avoids mutating nodes that live inside code or inlineCode to preserve code samples. When expandText returns multiple replacements (or a non-text replacement), the plugin replaces the original text node by splicing the new nodes into the parent’s children and advances the visitor past the newly inserted nodes. If expandText yields a single text replacement, the original node is left as-is. Use this plugin when you want inline tokens or patterns to be expanded into richer Markdown/HTML constructs during the Markdown-to-AST transformation rather than as a post-render step.

## Remarks
By performing inline enrichment at the AST level, this symbol isolates enrichment logic from rendering concerns and allows reuse across different Markdown processing pipelines. It also centralizes the safe handling of replacements and the skip-behavior, ensuring enrichments are applied deterministically while preserving code blocks.

## Notes
- Mutates the AST in place; be mindful in multi-pass pipelines where other plugins rely on stable node indices.
- Relies on expandText; changes to its contract may require updates to this plugin.
- Only touches inline text nodes; block-level content and fenced/code blocks are unaffected.

---

## remarkUnliftEmptyOrderedList
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function remarkUnliftEmptyOrderedList()
```


Transforms empty ordered lists in a Markdown Abstract Syntax Tree into a single paragraph containing the list's starting marker. It is intended for use in a Markdown processing pipeline to normalize documents by removing visually empty ordered lists while preserving their numbering context.

Specifically, when visiting a node of type 'list' that is ordered, and every item is empty (an item with no children or with only empty paragraphs), the transformer replaces that list node with a paragraph node whose text is the list's start value followed by a period (for example, '1.'). The start value defaults to 1 when undefined. After replacement, the transformer signals the traversal to skip ahead to avoid re-processing the new node.

## Remarks
This small transformer encapsulates a normalization rule into a reusable plugin, keeping the Markdown.tsx processing pipeline modular and predictable. It preserves the numbering context by emitting a textual marker ('start.') instead of rendering an empty list, which helps downstream renderers maintain layout consistency without introducing empty structural elements. It operates as a transformer in the Markdown AST pipeline and relies on the shape of list-related nodes (ordered lists with optional start) to decide when to apply the replacement.

## Notes
- Empty means: a list item has no meaningful content (the item has no children), or all its children are paragraphs with no content.
- Only ordered lists with this emptiness predicate are transformed; non-empty lists are left intact.
- The transformation mutates the AST in place and uses traversal control (SKIP) to avoid re-processing the newly inserted paragraph.

---

## toAstNode
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function toAstNode(inline: Inline): RootContent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `inline` | `Inline` | — |

**Returns:** `RootContent`


toAstNode transforms a small Inline fragment into a RootContent node for the Markdown renderer. It handles plain text, galactic-styled text, and inline code: text becomes a text node, galactic becomes a styled span, and other types render as inlineCode with a data-hex attribute.

## Remarks
This function centralizes the mapping from Inline variants to the concrete RootContent node shapes the renderer expects. The galactic variant is rendered as a span element (className 'gtw-galactic') with a data-galactic attribute, enabling styling without mutating the underlying text; the default path emits an inlineCode node with the original value stored in data-hex for potential styling or tooling use.

## Notes
- The implementation uses an explicit cast to RootContent in the non-text branches; this relaxes type checking and may require updates if RootContent or Inline shapes evolve.
- The function relies on specific data attributes (data-galactic, data-hex) and a CSS class (gtw-galactic) for styling; changing these breaks downstream styling/tests.

---

## toGalactic
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
export function toGalactic(s: string): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `s` | `string` | — |

**Returns:** `string`


toGalactic converts each character of the input string into its galactic counterpart by consulting a mapping named GAL_MAP. It iterates over the string, uppercases every character to perform a case-insensitive lookup, and appends the mapped value when available; otherwise it keeps the original character. The result is a new string that reflects the galactic substitutions while preserving characters that have no mapping.

## Remarks
This function encapsulates the substitution logic behind the galactic transformation, allowing callers to apply a consistent mapping without duplicating iteration or lookup code. By normalizing keys with toUpperCase, it treats 'a' and 'A' the same, ensuring deterministic substitutions. It is robust to unmapped characters, leaving them intact in the output.

## Notes
- Relies on a globally defined GAL_MAP; if GAL_MAP is undefined at runtime, this function will throw.
- If GAL_MAP contains a mapping to an empty string for a character, that character will be omitted from the result due to the nullish coalescing behavior.
- The implementation runs in O(n) time relative to the input string length and uses a straightforward per-character substitution.

---