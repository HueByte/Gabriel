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


Props defines the shape of the props consumed by the Markdown renderer used in this web app. It includes the markdown text to render and an optional streaming flag that indicates whether a parent typewriter is currently revealing characters. While streaming is true, the expensive plugins (highlight, katex) and the mermaid renderer are disabled during per-tick mutations and re-enable once streaming ends. Callers that render without streaming can omit this flag.

## Remarks
This interface serves as a contract between the streaming mechanism and the Markdown rendering logic. By exposing a single streaming flag, it allows the rendering path to skip heavy features during incremental updates, preserving responsiveness. It mirrors the lifecycle: during streaming, the full feature set is temporarily bypassed; when streaming finishes, the renderer can show the full, enhanced output again. It also decouples streaming concerns from text content.

## Example
```typescript
// Common case: streaming is not active
const props: Props = {
  text: "# Hello world"
};

// Streaming case
const streamingProps: Props = {
  text: "Typing...",
  streaming: true
};
```

## Notes
- During streaming, heavy features are disabled; if your UI relies on those features during typing, you must ensure they are re-enabled after streaming ends.
- Omitting the streaming property means the component will operate in non-streaming mode; do not assume a default of streaming being true.
- This interface is intended for typing the Markdown renderer's props—avoid using it as a general-purpose data-transfer object outside the Markdown rendering path.

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


Inline is a discriminated union that models small inline tokens used by the Markdown rendering component in the web UI. Each token carries a type discriminator and a string value. The three variants cover plain text and two specialized inline forms: galactic and hexChip. This abstraction lets the render layer pattern-match on token.type to render the appropriate visuals and enforces exhaustive handling of all variants, avoiding ad-hoc string concatenation.

## Remarks
This type serves as a narrow, extensible token representation that cleanly separates raw inline text from UI-specific inline decorations (galactic and hexChip). It sits between parsed content and the rendering components, enabling cohesive styling decisions in one place and simplifying testing by letting tokens be unit-tested.

## Example
```typescript
// Example usage of Inline tokens
const exampleInline: Inline[] = [
  { type: 'text', value: 'Welcome to ' },
  { type: 'galactic', value: 'Milky Way' },
  { type: 'text', value: ' documentation.' },
  { type: 'hexChip', value: '#1A2B' }
];
```

## Notes
- Use exhaustive pattern matching on token.type to ensure all variants are handled.
- All tokens carry a string-valued value; the rendering component is responsible for interpreting that string appropriately for each variant.

---

## MarkdownImpl
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function MarkdownImpl(
```


MarkdownImpl renders markdown content from the given text prop and is a reusable UI component for displaying markdown in the app. Use it whenever you want consistent markdown rendering across the interface instead of duplicating parsing and escaping logic inline; the streaming prop enables a streaming rendering path for large or incremental content.

## Remarks
MarkdownImpl abstracts markdown rendering behind a simple prop surface, so changes to parsing, escaping, or styling can be updated in one place without touching call sites. It also acts as a boundary between raw text data and UI concerns, making it easier to test rendering behavior and enforce consistent typography with the rest of the app. If the app ever needs to render markdown differently for accessibility or performance, this component is the natural place to evolve that behavior.

## Example
```tsx
<MarkdownImpl text="**Bold** text" />
<MarkdownImpl text="Hello, world!" streaming={true} />
```

## Notes
- Streaming mode may render content incrementally; ensure parent components can handle intermediate states.
- Be mindful to keep the text prop stable to avoid unnecessary re-renders in performance-critical parts of the UI.

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


Provides custom renderers for Markdown content, including how anchors, horizontal rules, and code blocks are rendered. The returned object is used by a Markdown renderer to open external links in a new tab, render mermaid diagrams after streaming completes, show inline color swatches for hex codes, and preserve syntax highlighting for code blocks. The streaming parameter gates mermaid rendering and the pre-wrapping behavior to balance live content delivery with deterministic diagram rendering.

## Remarks
buildComponents centralizes how Markdown primitives are turned into React elements. It encapsulates the specialized handling needed for Mermaid diagrams, hex color chips, and syntax-highlighted blocks, while delegating the rest to default rendering. By gating Mermaid rendering on the streaming flag, it prevents repeated heavy rendering during incremental reveal, while still enabling full diagrams once the stream finishes. This makes the markdown rendering responsive during streaming while preserving accurate diagrams and styling after completion.

## Dependencies
- Components

## Notes
- Mermaid rendering is performed only when streaming is false; during streaming, Mermaid blocks are kept in a state suitable for incremental rendering and are not re-rendered repeatedly.
- When a code block uses the mermaid language and streaming is disabled, the code is rendered via a dedicated Mermaid component with the cleaned source (trailing newline removed).
- Inline hex support relies on the presence of a data-hex attribute on the code's rest props to render a color swatch alongside the text.
- The pre renderer contains a special-case: if the content is a mermaid block and streaming is off, it returns the children directly (avoiding an extra <pre> wrapper) so Mermaid can mount correctly.

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


Parses a string and expands in-text galactic markers and hex chips into a RootContent[] AST suitable for rendering in the Markdown component. It begins by treating the input as a single Inline text stage, then, if GAL_OPEN and GAL_CLOSE are defined, extracts portions between these markers as galactic nodes while preserving surrounding text. In a second pass, plain-text segments are scanned for hex tokens defined by HEX_RE, and each match becomes a hexChip node. The resulting sequence of Inline fragments is finally converted to RootContent nodes via toAstNode and returned.

## Remarks
Encapsulates the translation of textual markup into a stable AST, decoupling rendering from string parsing. The galactic and hex-chip markers are defined by constants and a regex, so their presence and content drive the emitted node types; this keeps the parser adaptable without touching rendering code.

## Notes
- If GAL_OPEN or GAL_CLOSE is empty, galactic expansion is disabled to prevent potential infinite loops or misparsing.
- HEX_RE-based parsing only recognizes well-formed hex tokens; malformed or overlapping patterns are handled defensively by design, preserving non-matching text.
- toAstNode is responsible for converting Inline fragments into RootContent nodes; changes to the Inline shape may require corresponding updates to the mapping logic.

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


Extracts a language identifier from a className string. If the input is undefined or does not contain a language-<name> token, it returns null. Use this when you have a space-delimited set of CSS class tokens and you want to determine the code language (for syntax highlighting or code block rendering) without parsing the entire string yourself. The function returns the captured language name (for example, typescript) or null if none is found.

---

## remarkInlineEnrichments
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function remarkInlineEnrichments()
```


remarkInlineEnrichments is a higher-order Markdown plugin that returns a transformer for a Markdown AST. It walks text nodes (excluding those inside code blocks or inline code), expands their content via expandText, and, when a non-trivial set of replacements is produced, replaces the original text node with those inline nodes. If the expansion yields a single text fragment, no change is applied. The traversal is advanced with SKIP to prevent re-processing the newly inserted nodes.

## Remarks
The plugin mutates the AST in place by substituting a text node with the results of expandText, allowing inline enrichment to be composed without altering non-inline code. It intentionally avoids touching code and inlineCode nodes to preserve their literals. This separation between expansion logic (expandText) and application logic (remarkInlineEnrichments) makes the enrichment pipeline reusable and easier to compose with other remark plugins.

## Notes
- In-place mutation means downstream visitors may be affected; the SKIP mechanism helps prevent re-processing of inserted nodes.
- If expandText returns multiple nodes, they must conform to PhrasingContent[] to remain valid inline content; mismatches could produce invalid ASTs.

---

## remarkUnliftEmptyOrderedList
> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

```typescript
function remarkUnliftEmptyOrderedList()
```


Converts any completely empty ordered lists in a Markdown AST into a single paragraph containing the list's starting number followed by a dot (for example, "1."). This transformer is intended for use in a remark/unified pipeline to normalize content that accidentally includes empty ordered lists, replacing them with a simple, unambiguous marker instead of leaving an empty structure in the document.

## Remarks
This abstraction exists to normalize Markdown produced by diverse sources where empty ordered lists can appear—which can otherwise render awkwardly or be misinterpreted by consumers. By preserving the original starting value (or defaulting to 1) and replacing the list with a plain marker, it keeps the intent of the content intact while ensuring consistent rendering downstream. The transformation is designed to be safe to include in a pipeline alongside other MD AST transforms and is effectively idempotent in typical usage, since once an empty list is replaced, there is no list node left to process.

## Notes
- Only applies to ordered lists; unordered lists or lists with actual content are left untouched. 
- Mutates the AST in place by replacing the list node in the parent's children array, so downstream plugins should account for an updated tree structure after this pass.
- Uses the list's start value when present (defaulting to 1 if absent) to generate the replacement marker (e.g., "5." for a list starting at 5).


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


toAstNode converts an Inline into a RootContent node used by the Markdown renderer. It returns a simple text node for plain text, wraps galactic text in a span with a styling class and data attribute, and renders other inline values as inlineCode while attaching a data-hex attribute to preserve the original value.

## Remarks
This helper centralizes the translation from the domain Inline type to the MD AST representation, enabling consistent styling for galactic text and encoding for inline code without leaking rendering details to callers. The galactic path intentionally wraps the text in a span so styling can be applied via CSS while preserving the underlying value.

## Example
```ts
// galactic text becomes a styled span
toAstNode({ type: 'galactic', value: 'GZ' } as Inline)
// yields
{ type: 'text', value: 'GZ', data: { hName: 'span', hProperties: { className: 'gtw-galactic', 'data-galactic': 'true' } } } as RootContent
```
```ts
// plain text remains a simple text node
toAstNode({ type: 'text', value: 'hello' } as Inline)
// yields
{ type: 'text', value: 'hello' }
```
```ts
// non-text, non-galactic inline is treated as inlineCode with a data-hex attribute
toAstNode({ type: 'inlineCode', value: '0xFF' } as Inline)
// yields
{ type: 'inlineCode', value: '0xFF', data: { hProperties: { 'data-hex': '0xFF' } } }
```

## Notes
- The else path covers Inline variants beyond 'text' and 'galactic' by rendering them as inlineCode and encoding the original value via data-hex.
- The implementation uses a TypeScript cast to RootContent (as unknown as RootContent) to align the runtime shape with the MD AST type, which is worth keeping in mind when extending the type system or adding new Inline variants.

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


Converts an input string into a galactic-styled representation by mapping each character through GAL_MAP using the upper-case form for lookup. Characters without a mapping are preserved unchanged.

## Remarks
toGalactic serves as a single, reusable transformer for rendering strings in a galactic-style. It centralizes character-by-character substitution through GAL_MAP, making it easy to update or swap the styling without touching call sites. By uppercasing input for the lookup, it ensures consistent mappings regardless of input case, while preserving characters that have no mapping. It is a pure function with no side effects, suitable for use in UI rendering code such as markdown rendering components.

## Notes
- The per-character string concatenation pattern can incur allocations for very long inputs; for performance-critical paths, consider collecting characters in an array and joining them at the end.

---