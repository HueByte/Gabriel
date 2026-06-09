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

Props carries the data needed by the Markdown renderer: the raw markdown text to render and an optional streaming flag used to indicate that the parent is performing a per-character typewriter-style reveal. Set streaming to true while text is still being revealed so the Markdown component can temporarily skip expensive plugins and renderers; omit or set false for static/historical messages.

## Remarks
This interface exists to allow the Markdown component to avoid costly re-processing while the text is mutating (for example, during a typewriter animation). When streaming is true the component disables syntax highlighting, KaTeX, and the Mermaid renderer until the stream completes, then snaps them back in for the final rendered output. That keeps per-tick updates cheap and avoids visual/CPU churn.

## Example
```typescript
// Parent using a typewriter effect should toggle `streaming` while updating `text`.
function ParentExample() {
  const [text, setText] = useState('');
  const [streaming, setStreaming] = useState(true);

  useEffect(() => {
    // pseudo typewriter: reveal chars, then mark streaming false
    revealChars("# Hello from Typewriter", (partial, done) => {
      setText(partial);
      if (done) setStreaming(false);
    });
  }, []);

  return <Markdown text={text} streaming={streaming} />;
}

// Non-streaming usage for historical messages:
<Markdown text={message} />
```

## Notes
- streaming is optional; when omitted it should be treated as false by the consumer.
- Ensure the parent sets streaming back to false when the reveal completes so plugins (highlight, KaTeX, Mermaid) are applied to the final content.
- Do not leave streaming true for static content — doing so will keep expensive renderers disabled.

---

## Inline

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** type

Represents an inline-level AST node used by the Markdown renderer in this module. The type is a discriminated union of object variants identified by a literal `type` property; each variant carries the data needed to render or transform that inline element.

## Remarks
This union centralizes every inline element shape so rendering and transformation code can narrow by the `type` field in a type-safe way. It enables exhaustive pattern-matching (e.g., with `switch` statements and `never`-checks) so adding new inline kinds surfaces compile-time errors where handling is required.

## Notes
- The provided source snapshot was truncated; consult src/webapp/src/components/Markdown.tsx for the full list of variants and their exact properties.
- Prefer exhaustive narrowing when processing values of this type to avoid silently ignoring new variants introduced later.

---

## MarkdownImpl

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Renders Markdown content provided via the `text` prop into the component tree. Use this component when you want a React-friendly renderer for Markdown strings; set `streaming` to true when the Markdown content is delivered or updated incrementally (for example, from a streaming API or an incremental parser).

## Remarks
Acts as an application-level Markdown renderer abstraction. The `streaming` flag indicates whether callers will provide partial updates to `text` and therefore the component should expect and display incremental changes; when `streaming` is false the component can treat `text` as a single, complete document. Keeping styling and link-handling decisions outside this component makes it easier to reuse across different contexts.

## Example
```typescript
// Render a complete markdown document
<MarkdownImpl text={markdownString} />

// Render incremental/streamed markdown updates
<MarkdownImpl text={partialMarkdown} streaming={true} />
```

## Notes
- Treat `text` as untrusted input: ensure the Markdown pipeline (inside this component or upstream) sanitizes output to prevent XSS.
- Streaming mode implies more frequent re-renders as `text` changes; avoid heavy synchronous work during render to keep the UI responsive.

---

## buildComponents

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Returns a set of React components tailored for rendering Markdown nodes (anchors, horizontal rules, pre, and code) and intended to be passed into a markdown renderer's `components` prop. The implementation adapts behavior based on the `streaming` flag: when streaming is true it avoids expensive or premature rendering (notably for mermaid diagrams), and when false it renders final enhancements like mermaid diagrams and hex color chips.

## Remarks
This factory centralizes rendering decisions that would otherwise be scattered across Markdown renderer callbacks. It prevents expensive or incorrect operations during character-by-character streaming (for example, blocking mermaid diagram rendering until streaming completes) and provides consistent markup and CSS classes (e.g. `md-link`, `md-hr`, `md-hex`) for styling. It also forwards unknown attributes (`...rest`) to the underlying elements so callers (or remark/rehype plugins) can inject metadata such as `data-hex` used to render inline color chips.

## Example
```typescript
// Typical usage with react-markdown
import ReactMarkdown from 'react-markdown';

function MarkdownView({ source, streaming }: { source: string; streaming: boolean }) {
  const components = buildComponents(streaming);
  return <ReactMarkdown components={components}>{source}</ReactMarkdown>;
}
```

## Notes
- Mermaid diagrams are rendered only when `streaming` is false; during streaming the code leaves mermaid blocks as plain code to avoid repeated, partial renders.
- The `pre` wrapper is removed for mermaid blocks when not streaming — the code detects this by checking that the `children` is a valid React element and that its `className` indicates `language-mermaid`.
- Inline hex chips are produced when an element carries a `data-hex` attribute (passed via `...rest`); the component expects the hex string there and renders a swatch plus text using `md-hex*` classes.

---

## expandText

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Splits a plain string into inline AST pieces, extracting two special token types: "galactic" sections delimited by the GAL_OPEN/GAL_CLOSE sentinels, and "hexChip" tokens matched by HEX_RE. Use this when you need the raw text broken into typed Inline pieces (then mapped to RootContent nodes) so later rendering or processing can treat galactic fragments and hex chips specially rather than as plain text.

## Remarks
This function performs two sequential passes over the input: first it scans for sentinel-delimited galactic segments and separates them from surrounding text, then it scans remaining text fragments with a regular expression to locate hex-chip tokens. The result is converted to the final RootContent form via toAstNode. The two-pass design keeps the galactic sentinel extraction independent from the regex-based hex extraction so each rule is applied in a well-defined order.

## Notes
- GAL_OPEN and GAL_CLOSE must be non-empty strings; the function guards against empty sentinels and will skip galactic extraction if either is empty to avoid an infinite loop.
- HEX_RE is used with RegExp.exec in a loop and the code resets HEX_RE.lastIndex = 0; HEX_RE should be written to capture the expected groups (prefix and hex) and typically needs the global flag when reused in repeated calls.
- The function mutates and reuses the shared HEX_RE object (via lastIndex). If HEX_RE is shared elsewhere concurrently in non-atomic ways, results may be surprising — ensure calls are not interleaved with other code that relies on HEX_RE's state.

---

## languageOf

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Extracts a language identifier from an element className following the common "language-<lang>" convention used by syntax highlighters and Markdown renderers. Use this when you need to detect the code block language from an element's class list (for example, to choose a highlighter or language label) instead of parsing markup manually.

## Remarks
This small helper isolates the regex logic for reading language tokens from class names like "language-javascript" or "foo language-typescript bar". It returns the first matching token and deliberately returns null when the input is falsy or does not contain a language- prefix, letting callers handle the absence of a detected language.

## Example
```typescript
console.log(languageOf('language-typescript')); // 'typescript'
console.log(languageOf('foo language-python bar')); // 'python'
console.log(languageOf('no-language-here')); // null
console.log(languageOf(undefined)); // null
```

## Notes
- Returns null for falsy inputs (undefined, empty string) or when no "language-<token>" segment is present.
- The regex allows word characters and hyphens in the token (\w includes letters, digits, and underscore), so values like "language-cpp" and "language-foo_bar" will match.
- Only the first match is returned; if multiple "language-" segments exist, later ones are ignored.

---

## remarkInlineEnrichments

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Rewrites plain text nodes in a remark AST into richer phrasing content using the project-specific expandText routine. Use this plugin when you need inline transformations (links, custom nodes, annotations, etc.) produced from plain text fragments while preserving code spans.

## Remarks
This returns a remark transformer that visits every text node and asks expandText for replacement nodes. It intentionally skips nodes whose parent is a code or inlineCode node so that code spans remain untouched. When expandText returns only an unchanged text node, the tree is left as-is; otherwise the original text node is replaced in-place with the returned phrasing content nodes.

## Example
```typescript
import { remark } from 'remark';
import remarkInlineEnrichments from './Markdown';

const md = 'This is inline -> enrich me.';
const out = remark().use(remarkInlineEnrichments).processSync(md);
console.log(String(out));
```

## Notes
- The plugin mutates the AST in-place by splicing parent.children; callers should expect the original tree to be modified.
- It deliberately avoids processing text inside code / inlineCode nodes.
- The visitor returns [SKIP, newIndex] to avoid re-visiting nodes that were just inserted.

---

## remarkUnliftEmptyOrderedList

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Replaces an ordered list node whose items contain no content with a plain paragraph containing the list's start marker (for example "1."). Use this as a remark plugin when you need to preserve the visible numbering token for empty ordered lists instead of keeping an empty list node that downstream processors or renderers might drop or change.

## Remarks
This plugin walks the Markdown AST and targets ordered list nodes where every list item is empty or only contains paragraph nodes with no children. Rather than leaving an empty list structure, it mutates the parent to insert a paragraph node with the numeric marker derived from the list's start value (defaulting to 1). It runs as an in-place transformation during the remark/unified pipeline and intentionally replaces the list node (so list semantics are lost and only the textual marker remains).

## Example
```typescript
import { remark } from 'remark';
import remarkUnliftEmptyOrderedList from './Markdown'; // or wherever the plugin is exported

const md = '1.\n\n'; // an ordered list with an empty item
const processed = remark().use(remarkUnliftEmptyOrderedList).processSync(md);
console.log(String(processed)); // prints: "1." (a paragraph containing the marker)
```

## Notes
- The transformation mutates the AST in-place by splicing parent.children; downstream plugins that expect list nodes will not see them after this runs.
- Only ordered lists are affected; unordered lists are left untouched.
- A list's start property (when present) is used to generate the marker; if absent the marker defaults to "1.".
- The plugin treats list items with a single paragraph node that has zero children as empty as well.
- The implementation relies on visit/SKIP behavior and assumes the node shapes used in the checks; it performs light type coercion rather than strict type-checking.

---

## toAstNode

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Converts a small Inline token into a remark/rehype-compatible AST node (RootContent). Use this when mapping inline pieces from your tokenizer/lexer into nodes that the Markdown/rehype pipeline can render — it produces different AST shapes for plain text, 'galactic' tokens, and all other inline kinds.

## Remarks
This function centralizes the mapping between the app's Inline union and the AST shapes expected by the downstream Markdown/rehype renderer. It attaches HTML rendering hints via the node's data (data.hName / data.hProperties) so that rehype/remark plugins can emit specific elements and attributes (for example, a span with the class "gtw-galactic"). The implementation uses type assertions to coerce the returned objects to RootContent to satisfy typing differences between the hand-shaped objects and the AST types.

## Example
```typescript
// Given an array of Inline tokens, convert them to AST nodes for remark processing
const inlines: Inline[] = [
  { type: 'text', value: 'Hello' },
  { type: 'galactic', value: 'Σ' },
  { type: 'hex', value: '0xFF' },
];

const nodes: RootContent[] = inlines.map(toAstNode);
// nodes now contains text nodes, a text node decorated to render as a <span>,
// and an inlineCode node with a data-hex attribute for the 'hex' token.
```

## Notes
- The function uses `as unknown as RootContent` assertions; if the AST shape or RootContent type changes, update these returns to match the real types instead of asserting. 
- The mapping relies on downstream tooling honoring `data.hName` and `data.hProperties` (typical for remark/rehype integrations). If a different renderer is used, these hints may be ignored.
- Values are inserted directly into node fields; ensure token values are already sanitized/escaped if they can contain unsafe content.


---

## toGalactic

> **File:** `src/webapp/src/components/Markdown.tsx`  
> **Kind:** function

Converts an input string into a "Galactic" stylized form by mapping each character to a corresponding glyph from the GAL_MAP lookup table. Characters are looked up using their uppercase form; if a character has no mapping, it is left unchanged. Use this when you need a simple, deterministic transliteration for display (for example, rendering decorative text) rather than locale-aware or semantic transformations.

## Remarks
This is a small, pure transformer that separates the character-to-glyph mapping (the GAL_MAP table) from the iteration logic. It performs a per-character lookup using the uppercase form of each input character, so the mapping table only needs keys in a single case. The function returns a new string and has no side effects.

## Example
```typescript
// Given a GAL_MAP like { A: 'Λ', B: 'β', '!': '¡' }
const input = "Hello, World!";
const galactic = toGalactic(input);
// Result: each character mapped where possible; unmapped characters (e.g. ',', ' ') remain as-is
console.log(galactic);
```

## Notes
- The lookup uses s[i].toUpperCase() so any case information in the original string is not preserved by case-specific mappings.
- If GAL_MAP does not contain a key for an uppercased character, the original character is appended unchanged.
- For very large strings, repeated string concatenation may be less efficient than collecting mapped characters in an array and joining them; this implementation favors simplicity over micro-optimizations.

---