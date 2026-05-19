import { useEffect, useRef } from 'react';
import { fetchGabrielSequence, type GabrielSequence } from '../api/sequence';

// Renders a Gabriel Sequence (64 × 16×16 palette-indexed frames) at the
// configured display size. The animation linearly interpolates between
// consecutive frames with smoothstep easing, looping the full 64-frame cycle.
//
// Rendering strategy: draw at the canvas's native 16×16 and rely on the CSS
// `image-rendering: pixelated` scaling to keep crisp pixel edges. One
// `putImageData` per frame is significantly cheaper than 256 `fillRect` calls.
//
// The sequence is refetched whenever `conversationId` or `refreshKey` changes
// so Live State (frames 48-63) tracks ConversationState as it evolves.

// 280ms × 64 ≈ 18s full cycle. Snappier than the prior 600ms — at that pace
// the smoothstep pause-at-frame-boundaries read as a "muscle spasm". Faster
// linear motion between snapshots feels like a glide instead.
const FRAME_DURATION_MS = 280;
const FRAMES = 64;
const FRAME_W = 16;
const FRAME_H = 16;
const PIXEL_COUNT = FRAME_W * FRAME_H;

interface GabrielSequenceViewProps {
  conversationId: string;
  /** Bump this to force a refetch (e.g. after sending a message). */
  refreshKey?: number;
  /** Display size in pixels (square). Defaults to 200 to match the prior Three.js avatar. */
  size?: number;
  /** Fired once per successful fetch with the raw sequence so the parent can
   *  derive shared UI accents (galactic gradient, thinking-pulse colors, link
   *  tints) from the same server-driven palette. */
  onSequenceLoaded?: (sequence: GabrielSequence) => void;
}

export function GabrielSequenceView({
  conversationId,
  refreshKey,
  size = 200,
  onSequenceLoaded,
}: GabrielSequenceViewProps) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const sequenceRef = useRef<GabrielSequence | null>(null);
  const startTimeRef = useRef<number>(performance.now());

  // Fetch on conversation change + every refreshKey bump. The animation loop
  // reads sequenceRef so swap-in is seamless — no remount on refetch.
  useEffect(() => {
    const ctrl = new AbortController();
    fetchGabrielSequence(conversationId, ctrl.signal)
      .then(seq => {
        sequenceRef.current = seq;
        onSequenceLoaded?.(seq);
      })
      .catch(e => {
        if ((e as Error).name !== 'AbortError') {
          // Keep the previous frame visible on error rather than wiping to black.
          console.warn('Gabriel Sequence fetch failed:', e);
        }
      });
    return () => ctrl.abort();
  }, [conversationId, refreshKey]);

  // Animation loop. Survives sequence refetches; cancels on unmount.
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.imageSmoothingEnabled = false;

    let raf = 0;
    const draw = (now: number) => {
      const seq = sequenceRef.current;
      if (seq) {
        const elapsed = now - startTimeRef.current;
        const totalCycle = FRAME_DURATION_MS * FRAMES;
        const cyclePos = (elapsed % totalCycle) / FRAME_DURATION_MS;
        const i = Math.floor(cyclePos);
        const next = (i + 1) % FRAMES;
        // Pure linear interpolation — smoothstep eased into / out of every
        // frame boundary, which compounded with short distances between
        // adjacent palette indices to read as a per-frame "jolt". Linear feels
        // like continuous flow.
        const t = cyclePos - i;

        const a = seq.frames[i];
        const b = seq.frames[next];
        const imageData = ctx.createImageData(FRAME_W, FRAME_H);
        const data = imageData.data;
        for (let p = 0; p < PIXEL_COUNT; p++) {
          const ca = seq.palette[a[p]];
          const cb = seq.palette[b[p]];
          const offset = p * 4;
          data[offset    ] = Math.round(ca[0] + (cb[0] - ca[0]) * t);
          data[offset + 1] = Math.round(ca[1] + (cb[1] - ca[1]) * t);
          data[offset + 2] = Math.round(ca[2] + (cb[2] - ca[2]) * t);
          data[offset + 3] = 255;
        }
        ctx.putImageData(imageData, 0, 0);
      }
      raf = requestAnimationFrame(draw);
    };
    raf = requestAnimationFrame(draw);

    return () => cancelAnimationFrame(raf);
  }, []);

  return (
    <canvas
      ref={canvasRef}
      width={FRAME_W}
      height={FRAME_H}
      style={{
        width: size,
        height: size,
        imageRendering: 'pixelated',
        display: 'block',
        flex: '0 0 auto',
      }}
      aria-label="Gabriel Sequence avatar"
    />
  );
}
