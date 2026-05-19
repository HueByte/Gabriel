import { useEffect, useState } from 'react';

// Lazy-loaded mermaid integration. The mermaid bundle is ~700KB so we keep
// it out of the initial chunk and only pull it in the first time a mermaid
// code block is actually rendered. The module-level promise dedupes
// concurrent loads from sibling diagrams.

type MermaidModule = typeof import('mermaid');
let mermaidPromise: Promise<MermaidModule['default']> | null = null;

function loadMermaid(): Promise<MermaidModule['default']> {
  if (mermaidPromise) return mermaidPromise;
  mermaidPromise = import('mermaid').then(mod => {
    const mermaid = mod.default;
    mermaid.initialize({
      startOnLoad: false,
      theme: 'dark',
      // We render diagrams from chat content — `strict` blocks any inline HTML
      // that would otherwise execute in the SVG output.
      securityLevel: 'strict',
      fontFamily: '"Geist", -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif',
    });
    return mermaid;
  });
  return mermaidPromise;
}

// Unique render ids — mermaid uses the id to namespace SVG defs / arrowheads.
// Module-level counter so two diagrams on the same page can't collide.
let idCounter = 0;
const nextId = () => `md-mermaid-${++idCounter}`;

// Debounce in ms before attempting a render. Streamed assistant messages
// re-render Markdown on every token; rapidly invoking mermaid.render on
// half-typed source wastes cycles and floods the console with parse errors.
// 200ms keeps things responsive without thrashing.
const RENDER_DEBOUNCE_MS = 200;

interface MermaidProps {
  source: string;
}

export function Mermaid({ source }: MermaidProps) {
  const [svg, setSvg] = useState<string | null>(null);
  const [errored, setErrored] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setErrored(false);

    const handle = setTimeout(() => {
      loadMermaid()
        .then(m => m.render(nextId(), source))
        .then(({ svg }) => {
          if (!cancelled) setSvg(svg);
        })
        .catch(() => {
          // Common during streaming when the source isn't yet a complete
          // diagram. Fall back to the raw source so the user still sees
          // what's coming through.
          if (!cancelled) setErrored(true);
        });
    }, RENDER_DEBOUNCE_MS);

    return () => {
      cancelled = true;
      clearTimeout(handle);
    };
  }, [source]);

  if (errored) {
    // Source fallback — same visual as a normal code block so it doesn't
    // look broken, just unrendered.
    return (
      <pre className="md-mermaid-fallback"><code>{source}</code></pre>
    );
  }

  if (!svg) {
    // Reserve no space while loading — most diagrams render fast enough that
    // a "rendering…" flash is more distracting than a brief gap.
    return null;
  }

  // mermaid.render() returns a complete, sanitized SVG string. We trust it
  // because we initialized mermaid with securityLevel: 'strict', which
  // strips any user-provided HTML before rendering.
  return (
    <div
      className="md-mermaid"
      dangerouslySetInnerHTML={{ __html: svg }}
    />
  );
}
