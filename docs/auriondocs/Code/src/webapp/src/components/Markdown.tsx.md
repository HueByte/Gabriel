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

Props describes the properties accepted by the Markdown component: the text to render and an optional streaming flag used to indicate that the parent typewriter is still revealing characters. Set streaming=true while the component receives per-tick/partial updates so the renderer can temporarily disable expensive plugins (syntax highlighting, KaTeX) and the mermaid renderer; omit it for static or historical messages.

## Remarks
This small flag exists to avoid running costly renderers on every intermediate frame when text is being streamed in (for example, by a typewriter-like animator). It lets the Markdown component operate in a lightweight "streaming" mode during mutation and immediately re-enable full rendering once streaming finishes, preventing jank and excessive CPU work while preserving the final rendered output.

## Example
```typescript
// While a typewriter is animating text:
<Markdown text={currentPartial} streaming={true} />

// For a finished/historical message (full, static text):
<Markdown text={finalMessage} />
```

## Notes
- streaming is optional; when omitted the component behaves as non-streaming (full plugins enabled).
- Only set streaming while text is actively changing per-tick — leaving it true for static content will unnecessarily disable syntax highlighting and renderers.
- If callers fail to set streaming during rapid updates, expensive plugins may run repeatedly and cause performance issues or flicker.

---

## Inline

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** type

Represents a discriminated union of inline content nodes used by the Markdown component. Each variant is identified by its `type` discriminant so callers should narrow on `node.type` to handle specific inline kinds. The provided snippet only exposes the `'text'` variant; consult the full type definition in source for all variants and their fields.

## Remarks
Models inline-level AST nodes (the pieces that appear inside block-level content) and exists to make rendering and transformations type-safe. Using a `type` discriminant allows switch statements and type guards to narrow the union cleanly, enabling exhaustiveness checks and clearer renderer implementations.

## Example
```typescript
function renderInline(node: Inline) {
  switch (node.type) {
    case 'text':
      // node is narrowed to the 'text' variant here — render its fields (see full type for field names)
      return <span>{/* node.text */}</span>;
    // other cases for other inline variants...
    default:
      return null;
  }
}
```

## Notes
- The source shown for this type is truncated; verify the full definition to see all variants and their properties before relying on specific fields.
- Narrow on the `type` property rather than using unsafe casts; that preserves type safety and enables exhaustiveness checks.
- When adding new variants, update renderers and switch statements to avoid unhandled cases.

---

## MarkdownImpl

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Renders Markdown content supplied via the text prop. Use this component when you need an in-place Markdown renderer in the web UI; pass streaming={true} to enable the component's streaming/incremental rendering mode (defaults to false).

## Remarks
The source implementation for this symbol was not available in the provided snapshot; the function signature shows it accepts an object with text and an optional streaming boolean (default false). Conceptually this is a presentational Markdown renderer intended to be used wherever Markdown must be displayed in the app. The streaming flag exists to toggle between immediate full rendering and an incremental/streaming rendering strategy, but exact semantics are implementation-defined.

## Example
```typescript
// Render static markdown
<MarkdownImpl text={"**Hello** _world_"} />

// Use streaming mode (behaviour depends on implementation)
<MarkdownImpl text={someLargeMarkdownString} streaming={true} />
```

## Notes
- The implementation body was not available; do not assume HTML sanitization or XSS protection is performed — sanitize input upstream if required.
- The precise behaviour of streaming (how updates are applied, whether it accepts chunks, or how it handles partial markdown) is unspecified; review the implementation before relying on streaming for progressive rendering.
- Treat this as a UI/rendering convenience; if you need server-side rendering, pre-processing, or special plugin support, prefer a dedicated renderer or confirm capabilities in the component source.


---

## buildComponents

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Returns a set of React renderer overrides tailored for use with a markdown renderer (e.g. react-markdown). Use this when you need consistent rendering for links, horizontal rules, code blocks, fenced pre/code pairs, mermaid diagrams and an inline hex color chip enrichment — including special behavior while content is being streamed character-by-character.

## Remarks
The returned object implements custom components for anchor (<a>), horizontal rule (<hr>), <pre> blocks and <code> elements. Key responsibilities:
- External links open in a new tab with safe rel attributes and a consistent CSS class.
- <hr> receives a presentation class for styling.
- <pre> unwraps fenced-code children when they contain a mermaid diagram so the diagram (rendered as an SVG) is not nested inside a <pre>; this unwrap only happens once streaming has finished.
- <code> handles three cases: rendering a mermaid diagram (when not streaming), rendering an inline hex "chip" when a data-hex attribute is present, and falling back to a regular <code> with the provided className (used by syntax highlighters).

The streaming boolean disables any expensive or stateful rendering (like mermaid.render) while content is progressively revealed, preventing repeated renders on incomplete sources.

## Example
```typescript
// Typical use with react-markdown
import React from 'react';
import ReactMarkdown from 'react-markdown';

// streaming might be true while content is being typed or revealed;
// set to false after the full content is available so mermaid diagrams render.
const markdownComponents = buildComponents(/* streaming */ false);

function MarkdownView({ source }: { source: string }) {
  return <ReactMarkdown components={markdownComponents}>{source}</ReactMarkdown>;
}
```

## Notes
- Mermaid rendering only occurs when streaming is false; this avoids calling mermaid.render on partial source as it's typed/revealed.
- The code strips a trailing newline from fenced mermaid code (replace(/\n$/, '')) before passing to the Mermaid renderer — that matters if exact source formatting is significant.
- The inline hex chip requires a data-hex attribute (e.g. injected by a remark plugin). If that attribute is missing the inline code falls back to normal rendering.
- The color swatch element is aria-hidden to avoid redundant information for assistive tech; the hex text remains visible.
- The implementation assumes helpers (languageOf, isValidElement, Mermaid component) are available in scope; ensure those utilities/components are provided where this function is used.


---

## expandText

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Splits a plain string into tokenized AST content by extracting two special inline token types: "galactic" segments delimited by the GAL_OPEN/GAL_CLOSE sentinels, and "hexChip" tokens matched by HEX_RE. Use this when you need the Markdown pipeline to recognize and convert those embedded sentinel-delimited pieces and hex-like tokens into the editor's RootContent nodes instead of treating them as plain text.

## Remarks
The function performs two sequential passes over the input text. First it scans for GAL_OPEN/GAL_CLOSE pairs and produces Inline pieces of type 'galactic' for the enclosed content (with surrounding text preserved as 'text' pieces). A safety check avoids attempting that pass if the GAL_* sentinels are empty (an empty sentinel would cause an infinite loop because ''.indexOf('') returns 0). Second, it runs a global HEX_RE over the remaining text pieces to extract hex-like tokens (the regex is expected to capture a prefix and the hex payload); matches are emitted as 'hexChip' Inline pieces while non-matching substrings remain as 'text'. Finally, every Inline is converted to a RootContent node via toAstNode.

## Example
```typescript
// given GAL_OPEN, GAL_CLOSE and HEX_RE configured elsewhere
const src = `begin ${GAL_OPEN}secret-token${GAL_CLOSE} middle 0xDEADBEEF end`;
const astNodes = expandText(src);
// astNodes will contain RootContent nodes corresponding to:
// [{type: 'text', value: 'begin '}, {type: 'galactic', value: 'secret-token'},
//  {type: 'text', value: ' middle '}, {type: 'hexChip', value: 'DEADBEEF'}, {type: 'text', value: ' end'}]
```

## Notes
- If GAL_OPEN or GAL_CLOSE is an empty string the function skips galactic extraction entirely; this guard prevents an infinite loop but means sentinel parsing silently becomes a no-op.
- HEX_RE is treated as a global/iterative regex (the code resets HEX_RE.lastIndex = 0). Provide a regex with the expected capture groups (prefix then hex payload) or matches may produce incorrect start offsets.
- The function returns RootContent nodes by mapping Inline pieces through toAstNode; callers that rely on Inline shapes should inspect the input before the final mapping or adapt to the RootContent output.

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


Extracts a language identifier from an element class string by looking for a token of the form `language-<id>`. Returns the captured identifier when present, or `null` if the input is falsy or no `language-` token is found.

## Remarks
This small utility centralizes the logic for reading a syntax-highlighting language out of an HTML `class` attribute (for example, a `<code>` element rendered from Markdown). Keeping the regex here avoids duplicating parsing rules and makes it easy to adjust the token format in one place.

## Example
```typescript
languageOf("foo language-typescript bar"); // "typescript"
languageOf("language-js"); // "js"
languageOf(undefined); // null
languageOf("my-language-js"); // null (no whitespace or start before `language-`)
```

## Notes
- Returns `null` for falsy input (e.g., `undefined` or empty string).
- The regex captures characters matching `[\w-]`, so the identifier may include letters, digits, underscore and hyphens; matching is case-sensitive and only succeeds when `language-` appears at the start or immediately after whitespace.
- The function returns the raw captured identifier and does not validate it against a list of known languages; if multiple `language-` tokens exist, the first match is returned.

---

## remarkInlineEnrichments

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Replaces plain text nodes in a remark (mdast) tree with the nodes returned by expandText, allowing inline textual tokens to be transformed into richer phrasing nodes. It skips text that is inside code or inlineCode nodes and performs the replacement in-place, returning control information to the visitor so traversal continues after the newly inserted nodes.

## Remarks
This is a small remark/unified plugin factory: calling remarkInlineEnrichments() returns a transformer that visits every text node and delegates conversion to expandText. It exists to centralize inline enrichment logic (mentions, emojis, inline links, or other token-to-node expansion) at the AST level so downstream remark plugins or renderers see structured phrasing nodes instead of raw strings.

## Example
```typescript
import { unified } from 'unified';
import remarkParse from 'remark-parse';
import remarkHtml from 'remark-html';
import remarkInlineEnrichments from './Markdown';

const result = unified()
  .use(remarkParse)
  .use(remarkInlineEnrichments)
  .use(remarkHtml)
  .processSync('Hello @alice and :smile:')
  .toString();

console.log(String(result));
```

## Notes
- The plugin mutates the mdast tree in-place by splicing replacement nodes into parent.children; callers should not rely on the original text node remaining unchanged.
- expandText must return an array of mdast PhrasingContent nodes. If it returns a single plain text node, this plugin leaves the node as-is.
- The transform deliberately skips text nodes whose parent is a code or inlineCode node to avoid altering code samples.
- The visitor return value uses the [SKIP, nextIndex] convention to advance traversal past the newly inserted nodes; ensure the visitor utility in use supports that control return value.

---

## remarkUnliftEmptyOrderedList

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Replaces ordered-list nodes in the remark (mdast) tree where every list item is empty with a simple paragraph containing the list's start number and a trailing dot (e.g. "3."). Use this plugin when you want empty ordered lists preserved as a single-line marker instead of keeping an actual list node with empty items.

## Remarks
This is a small remark plugin that walks the AST and "unlifts" ordered list nodes that contain only empty items (either no children or paragraphs with zero children). It mutates the parent node array in-place, swapping the matching list node for a paragraph text node that shows the list start value and a period. The intent is to simplify rendering or downstream processing for intentionally empty ordered lists while preserving the declared start index.

## Example
```typescript
import { remark } from 'remark';
import { remarkUnliftEmptyOrderedList } from './Markdown';

const processor = remark().use(remarkUnliftEmptyOrderedList);
const out = processor.processSync('1.\n   \n2.\n   \n').toString();
// The empty ordered list will be converted to a paragraph like "1." (or the list's start value)
```

## Notes
- The plugin only targets ordered lists (node.ordered === true); unordered lists are ignored.
- A list is considered empty only if every listItem has no children or only paragraph children with zero children; nested lists or other child node types prevent the replacement.
- The implementation performs unchecked casts and mutates the tree in-place; downstream plugins expecting a List node may be affected.

---

## toAstNode

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Converts a small Inline token into a Markdown AST (RootContent) node. Reach for this when transforming your custom inline token representation into nodes suitable for further remark/rehype processing or rendering (for example, turning parsed inline fragments into text, span-wrapped text, or inline code nodes).

## Remarks
This function maps known Inline.type values to mdast-compatible nodes and attaches `data`/`h*` properties that downstream remark/rehype plugins can use when serializing to HTML. Specifically: `"text"` becomes a plain text node; `"galactic"` becomes a text node annotated with `data.hName = 'span'` and `hProperties` (so it will render as a <span class="gtw-galactic" data-galactic="true">...); any other inline type is emitted as an `inlineCode` node with a `data.hProperties['data-hex']` attribute. The implementation uses type assertions (`as unknown as RootContent`) to satisfy the expected return type.

## Example
```typescript
// Given Inline values
const a: Inline = { type: 'text', value: 'hello' };
const b: Inline = { type: 'galactic', value: 'azul' };
const c: Inline = { type: 'hex', value: '#ffee00' };

console.log(toAstNode(a));
// { type: 'text', value: 'hello' }

console.log(toAstNode(b));
// { type: 'text', value: 'azul', data: { hName: 'span', hProperties: { className: 'gtw-galactic', 'data-galactic': 'true' } } }

console.log(toAstNode(c));
// { type: 'inlineCode', value: '#ffee00', data: { hProperties: { 'data-hex': '#ffee00' } } }
```

## Notes
- The function asserts the returned objects as `RootContent`; callers should ensure the Inline shape is valid because invalid shapes may only be caught at runtime.
- The `galactic` branch intentionally produces a `text` node with `data.hName`/`hProperties` so that HTML output can be rendered as a styled <span>; it does not produce an explicit `element`/`html` node.
- The produced `data.hProperties` entries are used by remark/rehype converters and may expect specific value types (e.g., className sometimes being an array); the function always sets string values here.

---

## toGalactic

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Converts an input string into its "Galactic" representation by replacing each character with the value found in the global GAL_MAP using an uppercase lookup; when a character has no mapping the original character is preserved. Use this when rendering or displaying text with a custom character-mapping (glyph) layer rather than when performing locale-sensitive or multi-character text transformations.

## Remarks
This is a simple, character-by-character transformation utility intended to be used in presentation/rendering code (for example, a Markdown renderer that wants to show text in a fanciful glyph set). The function looks up each character after calling toUpperCase() so mapping keys are expected to be uppercase; mapping values may be any string and are concatenated into the output.

## Example
```typescript
// Minimal example showing how to provide a GAL_MAP and call toGalactic
const GAL_MAP: Record<string, string> = {
  A: 'λ',
  B: 'β',
  C: '¢',
  '!': '¡'
};

// assuming toGalactic is in scope
const input = 'Abc!a';
const output = toGalactic(input);
// output will be: 'λβ¢¡λ' because characters are looked up using uppercase keys
```

## Notes
- The function uppercases each character for the lookup; if you need locale-sensitive case handling use toLocaleUpperCase before calling or change the mapping strategy.  
- If a mapping value is the empty string (''), that empty string will be used (effectively removing the character). Only null or undefined will trigger the fallback to the original input character because the code uses the nullish coalescing operator (??).  
- For very large strings repeated '+' concatenation may be less efficient than building an array of pieces and joining them; for typical UI strings this implementation is sufficient.

---