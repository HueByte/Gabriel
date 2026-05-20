import { useCallback, useEffect, useRef, useState } from 'react';

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
      // We render diagrams from chat content - `strict` blocks any inline HTML
      // that would otherwise execute in the SVG output.
      securityLevel: 'strict',
      fontFamily: '"Geist", -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif',
    });
    return mermaid;
  });
  return mermaidPromise;
}

// Unique render ids - mermaid uses the id to namespace SVG defs / arrowheads.
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
  const [maximized, setMaximized] = useState(false);

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

  const onMaximize = useCallback(() => setMaximized(true), []);
  const onClose = useCallback(() => setMaximized(false), []);

  if (errored) {
    // Source fallback - same visual as a normal code block so it doesn't
    // look broken, just unrendered.
    return (
      <pre className="md-mermaid-fallback"><code>{source}</code></pre>
    );
  }

  if (!svg) {
    // Reserve no space while loading - most diagrams render fast enough that
    // a "rendering…" flash is more distracting than a brief gap.
    return null;
  }

  // mermaid.render() returns a complete, sanitized SVG string. We trust it
  // because we initialized mermaid with securityLevel: 'strict', which
  // strips any user-provided HTML before rendering.
  return (
    <>
      <div className="md-mermaid">
        <div
          className="md-mermaid-svg"
          dangerouslySetInnerHTML={{ __html: svg }}
        />
        <button
          type="button"
          className="md-mermaid-maximize"
          onClick={onMaximize}
          aria-label="Maximize diagram"
          title="Maximize"
        >
          <MaximizeIcon />
        </button>
      </div>
      {maximized && <MermaidModal svg={svg} source={source} onClose={onClose} />}
    </>
  );
}

interface ModalProps {
  svg: string;
  source: string;
  onClose: () => void;
}

// Full-viewport modal with the same SVG sized to fill the available space.
// The wrapper sets max-width/max-height: 100% on the inner svg via CSS so
// the diagram scales up to the modal bounds while preserving aspect ratio.
// Esc closes; clicking the backdrop closes; the inner panel stops
// propagation so clicks on the SVG itself don't dismiss.
function MermaidModal({ svg, source, onClose }: ModalProps) {
  const overlayRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', onKey);
    // Prevent the page underneath from scrolling while the modal is open.
    const prevOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => {
      window.removeEventListener('keydown', onKey);
      document.body.style.overflow = prevOverflow;
    };
  }, [onClose]);

  const onBackdropClick = useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === overlayRef.current) onClose();
  }, [onClose]);

  const onCopySource = useCallback(() => {
    void navigator.clipboard?.writeText(source);
  }, [source]);

  return (
    <div
      ref={overlayRef}
      className="md-mermaid-modal-overlay"
      onClick={onBackdropClick}
      role="dialog"
      aria-label="Diagram preview"
      aria-modal="true"
    >
      <div className="md-mermaid-modal">
        <div className="md-mermaid-modal-toolbar">
          <button
            type="button"
            className="md-mermaid-modal-btn"
            onClick={onCopySource}
            title="Copy source"
          >
            Copy source
          </button>
          <button
            type="button"
            className="md-mermaid-modal-btn"
            onClick={onClose}
            aria-label="Close"
            title="Close (Esc)"
          >
            ×
          </button>
        </div>
        <div
          className="md-mermaid-modal-body"
          dangerouslySetInnerHTML={{ __html: svg }}
        />
      </div>
    </div>
  );
}

function MaximizeIcon() {
  return (
    <svg
      width="14"
      height="14"
      viewBox="0 0 16 16"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M3 6V3h3" />
      <path d="M13 6V3h-3" />
      <path d="M3 10v3h3" />
      <path d="M13 10v3h-3" />
    </svg>
  );
}
