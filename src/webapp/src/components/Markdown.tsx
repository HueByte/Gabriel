import ReactMarkdown, { type Components } from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { visit, SKIP } from 'unist-util-visit';
import type { Root, Text, Parent, RootContent, PhrasingContent } from 'mdast';

// Private-use sentinels for the galactic trail. The typewriter wraps its
// trailing cipher chars in these so a remark plugin can lift them out into
// a styled span inline within the markdown AST (instead of dangling after
// any block elements).
export const GAL_OPEN = '';
export const GAL_CLOSE = '';

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

// Walk every text node and rewrite occurrences of galactic sentinels and
// hex codes into custom inline elements. Custom elements use mdast's data.hName
// hint so they appear as plain spans in the rendered HTML — react-markdown's
// `components` prop then maps them to React components.
function remarkInlineEnrichments() {
  return (tree: Root) => {
    visit(tree, 'text', (node: Text, index, parent: Parent | undefined) => {
      if (!parent || index == null) return;
      // Skip text inside code/inlineCode — we don't want to mangle source.
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

  // 1) Pull out galactic sentinel runs.
  const afterGalactic: Inline[] = [];
  for (const part of stages) {
    if (part.type !== 'text') { afterGalactic.push(part); continue; }
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

  // 2) Pull out hex codes from the remaining text parts.
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
  // hexChip — emit as inlineCode so it visually reads as `#hex`, with
  // a data attribute for the swatch renderer to pick up.
  return {
    type: 'inlineCode',
    value: inline.value,
    data: {
      hProperties: { 'data-hex': inline.value },
    },
  } as unknown as RootContent;
}

// Custom react-markdown component overrides. `code` adds a color swatch
// when `data-hex` is present; `a` opens links in a new tab; `hr` is a
// styled separator.
const components: Components = {
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
  code({ className, children, ...rest }) {
    // react-markdown passes data-* through here too.
    const hex = (rest as Record<string, unknown>)['data-hex'] as string | undefined;
    if (hex) {
      return (
        <code className="md-hex">
          <span className="md-hex-swatch" style={{ background: hex }} aria-hidden="true" />
          <span className="md-hex-text">{children}</span>
        </code>
      );
    }
    return <code className={className}>{children}</code>;
  },
};

interface Props {
  text: string;
}

/**
 * Renders a streamed/finalized markdown string with our inline enrichments:
 *   - galactic sentinel runs render as styled spans (palette-tinted via CSS var)
 *   - `#hex` color codes render as inline code with a swatch
 *   - links open in a new tab with accent color
 *   - `---` renders as a styled separator
 */
export function Markdown({ text }: Props) {
  return (
    <div className="md">
      <ReactMarkdown
        remarkPlugins={[remarkGfm, remarkInlineEnrichments]}
        components={components}
      >
        {text}
      </ReactMarkdown>
    </div>
  );
}
