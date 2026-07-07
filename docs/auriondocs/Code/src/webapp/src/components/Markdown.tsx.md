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


Props defines the inputs for the Markdown renderer component used in the web UI. The text prop is the Markdown content to render. The optional streaming flag signals that a parent typewriter is revealing characters; when streaming is true, the renderer disables the expensive plugins (highlight, katex) and the Mermaid renderer while the text mutates per-tick, and these features snap back once streaming ends. Non-streaming callers (history messages) can omit this flag.

## Remarks
This abstraction centralizes the rendering behavior that depends on whether content is being streamed in real time versus rendered statically. By encapsulating this concern behind a simple flag, the Markdown renderer can switch to a lightweight streaming mode during live typing, preserving responsiveness, while still supporting full-featured rendering for static content.

## Example
```typescript
// Most common usage
<Markdown text={message.content} streaming={isStreaming} />
```

## Notes
- Undefined streaming is treated as false; pass streaming explicitly to enable streaming behavior.
- If you toggle streaming rapidly, coordinate the timing in the parent to avoid visual flicker.
- Do not mutate the text prop inside the Markdown renderer during streaming; rely on prop changes from the owner to drive updates.

---

## Inline
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** type alias

```typescript
type Inline =
  |
```


Inline is a TypeScript discriminated union that encodes the various forms of inline content used by the Markdown renderer in Markdown.tsx. The union includes a text variant (type: 'text'), and is intended for developers who need to model, validate, or render inline markdown fragments in a type-safe way rather than juggling ad-hoc objects.

## Remarks
By centralizing inline shapes into a single type, the renderer can switch on the type field and render the corresponding React element. This abstraction isolates formatting concerns from layout, reduces runtime type errors, and makes it easier to evolve the inline model alongside the Markdown syntax.

## Notes
- If you extend Inline with new variants, ensure all render branches handling Inline are updated to avoid silent fallbacks.
- Rely on the type discriminator for exhaustive handling in switch statements to prevent surprising fall-through at runtime.

---

## MarkdownImpl
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function MarkdownImpl(
```


MarkdownImpl renders Markdown text into React elements by taking the Markdown source from the text prop and producing UI content. The streaming flag toggles incremental rendering if supported by the underlying parser, enabling smoother feedback for long Markdown bodies.

## Remarks
MarkdownImpl centralizes Markdown parsing and rendering behind a stable, simple interface. It isolates Markdown concerns from content components, so other parts of the UI can render Markdown consistently regardless of the underlying parser. By providing a single integration point for typography and styling of Markdown content, it helps maintain a cohesive look-and-feel across the app.

## Example
```typescript
// Example usage
<MarkdownImpl text={markdown} />
```

## Notes
- If the input might contain untrusted HTML, ensure the Markdown processor sanitizes HTML blocks to prevent XSS.
- When streaming is enabled, rendering may occur in chunks; consumers should not rely on the initial render containing the full content.
- Avoid re-parsing the same Markdown on every render; memoize text to improve performance.

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


Factory function that builds a Components map used by the Markdown renderer to customize how content is rendered in React. It accepts a streaming flag and returns renderers for anchors, horizontal rules, and code blocks, with special handling for mermaid diagrams, inline hex chips, and syntax-highlighted blocks; the streaming flag gates when mermaid diagrams are instantiated to avoid rendering incomplete diagrams during incremental reveals.

## Remarks
By centralizing the rendering logic, this function provides a single place to enforce consistent styling (e.g., md-link for anchors, md-hr for horizontal rules) while enabling dynamic rendering for advanced markdown features. The mermaid path is intentionally deferred until streaming has ceased to prevent partially rendered diagrams during live reveals, and the hex/code path adds lightweight inline visuals without altering the source content.

## Example
```typescript
// Example usage: render mermaid after streaming ends
const components = buildComponents(false);
<ReactMarkdown components={components}>{markdown}</ReactMarkdown>
```

## Notes
- Mermaid rendering occurs only when the code block language is mermaid and streaming is false; ensure your Mermaid blocks specify language-mermaid.
- Inline hex chips rely on a data-hex attribute on the code wrapper; supply the hex value via rest props (data-hex) to render the swatch and text.


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


Converts a plain string into a RootContent array by desugaring literal text into segments and turning embedded markers into semantic nodes. It walks the input in two passes: first, it splits on galactic markers delimited by GAL_OPEN and GAL_CLOSE (when configured), emitting galactic nodes for the content between the markers and preserving surrounding text as text nodes; second, it scans text fragments for hexadecimal chip patterns defined by HEX_RE and replaces each match with a hexChip node while keeping the surrounding text intact. The result is an AST-like sequence that downstream renderers can interpret as plain text, galactic tokens, or hex chips. A safety guard prevents galactic parsing if the sentinel values are empty, avoiding an infinite loop if these markers are stripped by tooling.

## Remarks
Why this exists: It centralizes the parsing logic for a Markdown-like input that mixes literal content with special inline tokens. By converting galactic markers and hex codes into dedicated node types, higher-level renderers can render or transform these tokens without re-scanning the raw string. It also isolates sentinel configuration (GAL_OPEN/GAL_CLOSE/HEX_RE) from the rest of the rendering pipeline, making it easier to swap how tokens are represented without touching consumer code.

## Notes
- Be aware the galactic parsing is skipped when GAL_OPEN or GAL_CLOSE is empty; in that case the function only applies hex-chip extraction and text splitting.
- Hex chip extraction uses HEX_RE with global flag; lastIndex is reset before each part to ensure correct multiple matches within the same text segment.
- If a galactic block is opened but not closed (o !== -1 but c === -1), the code gracefully falls back to treating the remainder as text rather than looping forever.

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


Parses a CSS className string to extract a language hint for code blocks. It searches for a token of the form language-<tag> anywhere in the string (at the start or after whitespace) and returns the captured tag, such as 'js' or 'typescript'. If className is undefined or if no language- token is found, it returns null. This utility is typically used by the Markdown renderer to determine the language to apply syntax highlighting without requiring a separate metadata field.

## Remarks
By isolating the language extraction logic in a tiny helper, the rendering pipeline remains decoupled from how language hints are encoded in DOM classes. It centralizes the convention of embedding language information in class names, enabling multiple rendering paths (e.g., Markdown code blocks, fenced blocks) to share the same heuristic. The function is deliberately small and side-effect free.

## Example
```typescript
languageOf("language-js"); // "js"
languageOf("foo language-typescript bar"); // "typescript"
languageOf(undefined); // null
```

## Notes
- Returns null for falsy input or when the string doesn't contain a language- token.
- If multiple language- tokens exist, only the first match is returned.


---

## remarkInlineEnrichments
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function remarkInlineEnrichments()
```


remarkInlineEnrichments is a Remark plugin that, during AST traversal, expands text nodes into richer inline content by calling the expandText helper and splicing the resulting nodes into the tree. It mutates the AST in place and avoids altering code blocks or inline code, so only plain text fragments are enriched.

## Remarks
This abstraction centralizes the inline enrichment logic for Markdown by operating over the text nodes in the syntax tree, allowing downstream renderers to see richer inline structures without manually manipulating each node type. It cleanly separates the enrichment policy from the traversal mechanics and ensures code integrity by skipping code-like parents.

## Example
```ts
import { remark } from 'remark';
import { remarkInlineEnrichments } from './Markdown';

const processor = remark().use(remarkInlineEnrichments);
```

## Notes
- It mutates the input AST in place; if you need immutability, clone the tree before applying this transformer.
- It deliberately skips text nodes that live inside code blocks or inline code to avoid corrupting code syntax.
- This plugin relies on expandText producing valid PhrasingContent[]; ensure that function handles edge cases for your enrichments.

---

## remarkUnliftEmptyOrderedList
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function remarkUnliftEmptyOrderedList()
```


Removes an empty ordered list in a Markdown AST by replacing it with a paragraph that contains the list's starting index followed by a period (e.g., '1.'). Use this when normalizing MDASTs produced by Remark so downstream renderers don't have to deal with no-op ordered lists; it's implemented as a plugin factory that returns a transformer function.

## Remarks
This abstraction centralizes the empty-list normalization logic as a small Remark plugin, keeping the transformation logic isolated from the rest of the pipeline. It traverses the MDAST, and when it finds an ordered list whose items are effectively empty, it replaces the list node with a paragraph containing the list's start value and a trailing dot. The traversal uses the SKIP control to advance safely after a replacement, preserving correct visitation semantics while mutating the tree in place.

## Example
```typescript
import { remark } from 'remark';
import remarkUnliftEmptyOrderedList from './path/to/remarkUnliftEmptyOrderedList';

const processor = remark().use(remarkUnliftEmptyOrderedList);
const input = '1.\n\n'; // an ordered list with empty items
const output = processor.processSync(input).toString();
// Output will contain a paragraph like '1.' instead of the empty list
```

## Notes
- The plugin mutates the MDAST in place; avoid reusing the original tree after processing.
- It only affects ordered lists, and only when every item is effectively empty.
- The replacement uses the list's start value when present, defaulting to 1 if undefined (producing '1.').

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


ToAstNode maps an Inline value into a RootContent AST node used by the Markdown renderer in the web UI. It handles three cases: 'text' yields a plain text node; 'galactic' yields a text node augmented with a DOM-like span data (class 'gtw-galactic', data-galactic attribute) to enable galactic styling; any other inline variant is treated as an inlineCode node carrying the original value and a data-hex attribute for easy identification or testing. This centralizes the transformation from the semantic Inline type to the low-level renderable AST, ensuring consistent rendering behavior across the Markdown component.

## Remarks
This function acts as an adaptor between the higher-level Inline domain and the concrete RootContent nodes consumed by the UI renderer. By encoding the galactic variant as a text node with a dedicated span wrapper, it keeps the AST shape stable while allowing specialized styling without introducing new node kinds. Centralizing this mapping reduces duplication and makes future rendering adjustments isolated to this function.

## Example
```typescript
// Common cases
toAstNode({ type: 'text', value: 'Hello' });
// => { type: 'text', value: 'Hello' }

toAstNode({ type: 'galactic', value: '银河' });
// => { type: 'text', value: '银河', data: { hName: 'span', hProperties: { className: 'gtw-galactic', 'data-galactic': 'true' } } }

toAstNode({ type: 'inlineCode', value: 'x' });
// => { type: 'inlineCode', value: 'x', data: { hProperties: { 'data-hex': 'x' } } }
```

## Notes
- The galactic path adds a span-like wrapper with className 'gtw-galactic' and data-galactic flag, which affects rendering rather than the core text content.
- The function uses a type assertion (as unknown as RootContent) for non-plain-text branches; downstream consumers should rely on the exposed data shape rather than type alone.
- The data-hex attribute preserves the input value for potential tooling or styling needs without altering the visible text.

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


toGalactic converts an input string into its galactic representation by replacing each character with the value found in the global GAL_MAP. It looks up the uppercase form of every character, so the translation is effectively case-insensitive with respect to the mapping. Characters without a mapping are preserved in the output. This helper is handy when you need to render text using a custom symbol set without performing the mapping inline at each call.

## Remarks
The lookup uses the character's uppercase form, so A and a map to the same galactic glyph when defined. The function returns a new string and does not mutate the input. GAL_MAP is expected to be a runtime global; ensure it is defined in the module where this function runs. Note that if a mapping yields an empty string, that character will be omitted from the result due to the nullish coalescing operator.

## Example
```typescript
// Example assuming GAL_MAP maps A -> 'Λ' and B -> 'β'
toGalactic("ABab!") // => "ΛβΛβ!"
```

## Notes
- If GAL_MAP contains an entry that maps a character to the empty string, that character will be removed from the output.
- This function concatenates the result in a loop; for extremely long inputs you might consider a more efficient accumulation strategy, though modern engines perform well here.

---