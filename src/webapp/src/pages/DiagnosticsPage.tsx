import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { HiOutlineArrowLeft } from 'react-icons/hi2';
import { fetchGabrielSequence, type GabrielSequence } from '../api/sequence';
import { notifyError } from '../lib/notify';
import { paletteVarsFromStops, type RGB } from '../pulse/palettes';

// Diagnostics view: every frame of the Gabriel Sequence rendered as its own
// 16×16 canvas in an 8×8 grid. One canvas per frame (rather than one big
// canvas) keeps per-frame labels, hover affordances, and independent layout
// trivial — the extra DOM nodes are cheap at this size.

const FRAME_W = 16;
const FRAME_H = 16;
const PIXEL_COUNT = FRAME_W * FRAME_H;

export function DiagnosticsPage() {
  const { conversationId = '' } = useParams<{ conversationId: string }>();
  const navigate = useNavigate();
  const [sequence, setSequence] = useState<GabrielSequence | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!conversationId) return;
    setSequence(null);
    setError(null);
    const ctrl = new AbortController();
    fetchGabrielSequence(conversationId, ctrl.signal)
      .then(setSequence)
      .catch(e => {
        if ((e as Error).name === 'AbortError') return;
        setError((e as Error).message);
        notifyError(e, 'Failed to load sequence.');
      });
    return () => ctrl.abort();
  }, [conversationId]);

  // Re-apply the sequence's palette to local CSS vars so accent / gradient
  // colors on this page match the avatar's visual identity for this
  // conversation.
  const paletteVars = useMemo(() => {
    if (!sequence) return undefined;
    const stops: RGB[] = sequence.palette.map(c => [c[0], c[1], c[2]] as const);
    return paletteVarsFromStops(stops) as React.CSSProperties;
  }, [sequence]);

  const onBack = () => navigate(`/c/${encodeURIComponent(conversationId)}`);

  return (
    <div className="diagnostics palette-scope" style={paletteVars}>
      <div className="diagnostics-head">
        <button type="button" className="diagnostics-back" onClick={onBack}>
          <HiOutlineArrowLeft aria-hidden="true" />
          <span>Back</span>
        </button>
        <h1 className="diagnostics-title">Gabriel Sequence — Diagnostics</h1>
        {sequence && (
          <div className="diagnostics-meta">
            <span>seed {sequence.metadata.seed}</span>
            <span>v{sequence.version}</span>
            <span>{sequence.frames.length} frames</span>
            <span>{sequence.palette.length} colors</span>
          </div>
        )}
      </div>

      {error && <div className="error">{error}</div>}

      {!sequence && !error && (
        <div className="diagnostics-loading">Loading sequence…</div>
      )}

      {sequence && (
        <div className="diagnostics-grid" role="grid" aria-label="Sequence frames">
          {sequence.frames.map((frame, index) => (
            <DiagnosticsFrame
              key={index}
              frame={frame}
              palette={sequence.palette}
              index={index}
            />
          ))}
        </div>
      )}
    </div>
  );
}

interface DiagnosticsFrameProps {
  frame: number[];
  palette: number[][];
  index: number;
}

function DiagnosticsFrame({ frame, palette, index }: DiagnosticsFrameProps) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.imageSmoothingEnabled = false;
    const imageData = ctx.createImageData(FRAME_W, FRAME_H);
    const data = imageData.data;
    for (let p = 0; p < PIXEL_COUNT; p++) {
      const c = palette[frame[p]];
      const offset = p * 4;
      data[offset] = c[0];
      data[offset + 1] = c[1];
      data[offset + 2] = c[2];
      data[offset + 3] = 255;
    }
    ctx.putImageData(imageData, 0, 0);
  }, [frame, palette]);

  return (
    <div className="diagnostics-frame" role="gridcell">
      <canvas
        ref={canvasRef}
        width={FRAME_W}
        height={FRAME_H}
        className="diagnostics-canvas"
        aria-label={`Frame ${index}`}
      />
      <span className="diagnostics-frame-label">{index.toString().padStart(2, '0')}</span>
    </div>
  );
}
