import { isValidElement, memo, useMemo, type ReactElement } from 'react';
import ReactMarkdown, { type Components } from 'react-markdown';
import type { PluggableList } from 'unified';
import remarkGfm from 'remark-gfm';
import remarkMath from 'remark-math';
import rehypeHighlight from 'rehype-highlight';
import rehypeKatex from 'rehype-katex';
import { visit, SKIP } from 'unist-util-visit';
import type { Root, Text, Parent, RootContent, PhrasingContent } from 'mdast';
import { Mermaid } from './Mermaid';

// Private-use sentinels for the galactic trail. The typewriter wraps its
// trailing cipher chars in these so a remark plugin can lift them out into
// a styled span inline within the markdown AST (instead of dangling after
// any block elements).
//
// IMPORTANT: written as `\uXXXX` escapes, NOT as raw chars. The raw form
// renders as zero-width in most editors/terminals, so a copy-paste through
// a tool that strips non-printable chars silently turns them into empty
// strings — at which point `''.indexOf('')` returns 0 and the loop below
// runs forever, allocating until the tab is OOM-killed. Don't ask how I
// know.
export const GAL_OPEN = '\uE001';
export const GAL_CLOSE = '\uE002';

// Standard Galactic-style cipher: A–Z → Tifinagh glyphs.
const GAL_MAP: Record<string, string> = {
  A: 'ⴰ', B: 'ⴱ', C: 'ⵛ', D: 'ⴷ', E: 'ⴻ', F: 'ⴼ', G: 'ⴳ',
  H: 'ⵀ', I: 'ⵉ', J: 'ⵊ', K: 'ⴽ', L: 'ⵍ', M: 'ⵎ', N: 'ⵏ',
  O: 'ⵄ', P: 'ⵒ', Q: 'ⵇ', R: 'ⵔ', S: 'ⵙ', T: 'ⵟ', U: 'ⵓ',
  V: 'ⵠ', W: 'ⵡ', X: 'ⵅ', Y: 'ⵢ', Z: 'ⵣ',
};

export function toGalactic(s: string): string {
  let out = '';
  for (let i = 0; i < s.length; i++) {
    out += GAL_MAP[s[i].toUpperCase()] ?? s[i];
  }
  return out;
}

// Hex pattern: #RGB | #RGBA | #RRGGBB | #RRGGBBAA, with a non-word boundary
// guard so it doesn't match the middle of an identifier (e.g. "foo#bar").
const HEX_RE = /(^|[^A-Za-z0-9_])(#(?:[0-9a-fA-F]{3,4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8}))(?=$|[^A-Za-z0-9_])/g;

function remarkInlineEnrichments() {
  return (tree: Root) => {
    visit(tree, 'text', (node: Text, index, parent: Parent | undefined) => {
      if (!parent || index == null) return;
      if (parent.type === 'code' || parent.type === 'inlineCode') return;

      const replacements = expandText(node.value);
      if (replacements.length === 1 && replacements[0].type === 'text') return;
      parent.children.splice(index, 1, ...(replacements as PhrasingContent[]));
      return [SKIP, index + replacements.length];
    });
  };
}

type Inline =
  | { type: 'text'; value: string }
  | { type: 'galactic'; value: string }
  | { type: 'hexChip'; value: string };

function expandText(value: string): RootContent[] {
  const stages: Inline[] = [{ type: 'text', value }];

  // Safety guard — if the sentinel constants ever get stripped to empty
  // strings (some tools normalize non-printable PUA chars away), the loop
  // below would spin forever (`''.indexOf('')` returns 0). Detect the
  // misconfig and bail before allocating into oblivion.
  const galacticSafe = GAL_OPEN.length > 0 && GAL_CLOSE.length > 0;

  const afterGalactic: Inline[] = [];
  for (const part of stages) {
    if (part.type !== 'text' || !galacticSafe) { afterGalactic.push(part); continue; }
    let s = part.value;
    while (true) {
      const o = s.indexOf(GAL_OPEN);
      if (o === -1) break;
      const c = s.indexOf(GAL_CLOSE, o + 1);
      if (c === -1) break;
      if (o > 0) afterGalactic.push({ type: 'text', value: s.slice(0, o) });
      afterGalactic.push({ type: 'galactic', value: s.slice(o + 1, c) });
      s = s.slice(c + 1);
    }
    if (s) afterGalactic.push({ type: 'text', value: s });
  }

  const afterHex: Inline[] = [];
  for (const part of afterGalactic) {
    if (part.type !== 'text') { afterHex.push(part); continue; }
    HEX_RE.lastIndex = 0;
    let last = 0;
    let m: RegExpExecArray | null;
    while ((m = HEX_RE.exec(part.value)) !== null) {
      const prefix = m[1];
      const hex = m[2];
      const start = m.index + prefix.length;
      if (start > last) afterHex.push({ type: 'text', value: part.value.slice(last, start) });
      afterHex.push({ type: 'hexChip', value: hex });
      last = start + hex.length;
    }
    if (last < part.value.length) afterHex.push({ type: 'text', value: part.value.slice(last) });
  }

  return afterHex.map(toAstNode);
}

function toAstNode(inline: Inline): RootContent {
  if (inline.type === 'text') {
    return { type: 'text', value: inline.value };
  }
  if (inline.type === 'galactic') {
    return {
      type: 'text',
      value: inline.value,
      data: {
        hName: 'span',
        hProperties: { className: 'gtw-galactic', 'data-galactic': 'true' },
      },
    } as unknown as RootContent;
  }
  return {
    type: 'inlineCode',
    value: inline.value,
    data: {
      hProperties: { 'data-hex': inline.value },
    },
  } as unknown as RootContent;
}

// Extracts a language hint from a className like "language-rust hljs".
function languageOf(className: string | undefined): string | null {
  if (!className) return null;
  const m = /(?:^|\s)language-([\w-]+)/.exec(className);
  return m ? m[1] : null;
}

// Builds the react-markdown component overrides. While streaming, mermaid
// blocks render as plain code (no diagram) — calling mermaid.render on
// partial source many times per second leaks SVG/DOM nodes. Once streaming
// flips false the components map swaps and the real Mermaid renderer takes
// over with the complete source.
function buildComponents(streaming: boolean): Components {
  return {
    a({ href, children, ...rest }) {
      return (
        <a href={href} target="_blank" rel="noopener noreferrer" className="md-link" {...rest}>
          {children}
        </a>
      );
    },
    hr() {
      return <hr className="md-hr" />;
    },
    pre({ children, ...rest }) {
      // react-markdown wraps fenced code as <pre><code class="language-X">...</code></pre>.
      // For mermaid we render an SVG instead, which doesn't belong inside <pre>.
      // While streaming we keep the <pre> wrap because we're rendering it as
      // plain code anyway.
      if (!streaming && isValidElement(children)) {
        const child = children as ReactElement<{ className?: string }>;
        if (languageOf(child.props.className) === 'mermaid') {
          return <>{children}</>;
        }
      }
      return <pre {...rest}>{children}</pre>;
    },
    code({ className, children, ...rest }) {
      // 1) Mermaid — language-mermaid block becomes a rendered diagram, but
      // only once streaming has stopped. Rendering on every char-reveal tick
      // spawns rapid mermaid.render() calls on incomplete sources.
      if (!streaming && languageOf(className) === 'mermaid') {
        const source = String(children).replace(/\n$/, '');
        return <Mermaid source={source} />;
      }

      // 2) Hex chip — inline code marked by remarkInlineEnrichments.
      const hex = (rest as Record<string, unknown>)['data-hex'] as string | undefined;
      if (hex) {
        return (
          <code className="md-hex">
            <span className="md-hex-swatch" style={{ background: hex }} aria-hidden="true" />
            <span className="md-hex-text">{children}</span>
          </code>
        );
      }

      // 3) Default — for highlighted blocks className carries `hljs language-xxx`
      // and children is an array of token spans (from rehype-highlight).
      return <code className={className}>{children}</code>;
    },
  };
}

// Two stable identity component maps so swapping in/out the mermaid renderer
// based on streaming doesn't reallocate a fresh object every render.
const COMPONENTS_IDLE = buildComponents(false);
const COMPONENTS_STREAMING = buildComponents(true);

// Plugin arrays hoisted to module scope so react-markdown sees stable
// identities across renders.
const REMARK_PLUGINS: PluggableList = [remarkGfm, remarkMath, remarkInlineEnrichments];

// Precomputed rehype-plugin permutations. We pick from these by inspecting
// the rendered text — running rehype-highlight on a message with no fenced
// code is wasted work, and running rehype-katex on a message with no `$` is
// the same. Plus during streaming we skip both unconditionally because they
// re-process the full doc on every char-reveal tick.
const REHYPE_NONE: PluggableList = [];
const REHYPE_HIGHLIGHT: PluggableList = [[rehypeHighlight, { ignoreMissing: true }]];
const REHYPE_KATEX: PluggableList = [[rehypeKatex, { throwOnError: false }]];
const REHYPE_BOTH: PluggableList = [
  [rehypeHighlight, { ignoreMissing: true }],
  [rehypeKatex, { throwOnError: false }],
];

function pickRehypePlugins(text: string, streaming: boolean): PluggableList {
  if (streaming) return REHYPE_NONE;
  const hasFence = text.includes('```');
  // Math is hot on the `$` char — quick reject keeps the common case (plain
  // prose) on the cheap path. The plugin itself validates the rest.
  const hasMath = text.includes('$');
  if (hasFence && hasMath) return REHYPE_BOTH;
  if (hasFence) return REHYPE_HIGHLIGHT;
  if (hasMath) return REHYPE_KATEX;
  return REHYPE_NONE;
}

interface Props {
  text: string;
  /** True while the parent typewriter is still revealing chars. Disables
   *  the expensive plugins (highlight, katex) and the mermaid renderer
   *  while text is mutating per-tick; they snap in the instant streaming
   *  ends. Non-streaming callers (history messages) can omit. */
  streaming?: boolean;
}

function MarkdownImpl({ text, streaming = false }: Props) {
  // useMemo so the picked plugin array's identity is stable for the lifetime
  // of this text — keeps react-markdown's internal caching working.
  const rehypePlugins = useMemo(
    () => pickRehypePlugins(text, streaming),
    [text, streaming],
  );
  const components = streaming ? COMPONENTS_STREAMING : COMPONENTS_IDLE;
  return (
    <div className="md">
      <ReactMarkdown
        remarkPlugins={REMARK_PLUGINS}
        rehypePlugins={rehypePlugins}
        components={components}
      >
        {text}
      </ReactMarkdown>
    </div>
  );
}

export const Markdown = memo(MarkdownImpl);
