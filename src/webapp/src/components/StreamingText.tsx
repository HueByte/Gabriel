import { useEffect, useMemo, useRef, useState } from 'react';

// Reveal cadence — chars per second.
//   actualRate = min(MAX_RATE, BASE_RATE + backlog * SPEEDUP_PER_BACKLOG_CHAR)
// Tunes so that short replies type at ~22 chars/sec (clear typewriter feel)
// and long ones accelerate toward MAX_RATE so a 1000-char reply doesn't crawl.
const BASE_RATE = 22;
const SPEEDUP_PER_BACKLOG_CHAR = 0.6;
const MAX_RATE = 200;

// Chars at the leading edge that stay in galactic cipher before english
// translation catches up. Keeps the alien-script crest visible regardless
// of message length.
const GALACTIC_LEAD = 25;

// A–Z → Tifinagh glyphs. Non-letters pass through untouched.
const GAL_MAP: Record<string, string> = {
  A: 'ⴰ', B: 'ⴱ', C: 'ⵛ', D: 'ⴷ', E: 'ⴻ', F: 'ⴼ', G: 'ⴳ',
  H: 'ⵀ', I: 'ⵉ', J: 'ⵊ', K: 'ⴽ', L: 'ⵍ', M: 'ⵎ', N: 'ⵏ',
  O: 'ⵄ', P: 'ⵒ', Q: 'ⵇ', R: 'ⵔ', S: 'ⵙ', T: 'ⵟ', U: 'ⵓ',
  V: 'ⵠ', W: 'ⵡ', X: 'ⵅ', Y: 'ⵢ', Z: 'ⵣ',
};
function toGalactic(s: string): string {
  let out = '';
  for (let i = 0; i < s.length; i++) {
    const ch = s[i];
    out += GAL_MAP[ch.toUpperCase()] ?? ch;
  }
  return out;
}

interface Props {
  text: string;
  /** True while the SSE stream is still appending to `text`. Captured at mount;
   *  toggling later doesn't abort the in-flight typewriter — it always finishes. */
  animate: boolean;
  caret?: boolean;
  /** Render the leading edge of the reveal in galactic cipher; english
   *  translation trails behind by GALACTIC_LEAD chars. */
  galactic?: boolean;
}

/**
 * Smooth typewriter with two cursors:
 *   - `gal` is the leading edge (always 1 char per scheduled reveal moment).
 *   - `en`  trails `gal` either flush (non-galactic) or by GALACTIC_LEAD chars
 *           (galactic mode). Once `gal` reaches the end of the known text,
 *           `en` is allowed to catch up.
 *
 * Driven by `requestAnimationFrame` + time-based scheduling, so it paces to
 * wall-clock rather than to `setInterval`'s drift. One char per reveal moment
 * (no chunked bursts) → no visible jumps.
 */
export function StreamingText({ text, animate, caret = false, galactic = false }: Props) {
  const prefersReduced = typeof window !== 'undefined'
    && window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;

  // Captured once on mount. Future toggles of `animate` don't stop the loop —
  // it keeps running until visible catches `text`.
  const [animating] = useState(animate && !prefersReduced);
  const cursorsRef = useRef({
    gal: animating ? 0 : text.length,
    en: animating ? 0 : text.length,
  });
  const targetRef = useRef(text);
  const galacticRef = useRef(galactic);
  galacticRef.current = galactic;

  // Scheduled time (performance.now() domain) for the next char to appear.
  // 0 means "not initialized" — first tick sets it to `now`.
  const nextRevealRef = useRef(0);
  const rafRef = useRef<number | null>(null);
  // Trivial counter so React re-renders when cursors advance.
  const [, bump] = useState(0);

  // Cache the full galactic translation. Recomputes only when `text` changes,
  // not every frame. Cheap even for 1k-char text; just no reason to redo it.
  const galacticFull = useMemo(
    () => (galactic ? toGalactic(text) : ''),
    [text, galactic],
  );

  useEffect(() => {
    targetRef.current = text;

    if (!animating) {
      cursorsRef.current.gal = text.length;
      cursorsRef.current.en = text.length;
      bump(n => n + 1);
      return;
    }

    // Guard against text shrinking under the cursors.
    if (cursorsRef.current.gal > text.length) cursorsRef.current.gal = text.length;
    if (cursorsRef.current.en > text.length) cursorsRef.current.en = text.length;

    // If the rAF loop is idle but there's now work to do, restart it.
    const { gal, en } = cursorsRef.current;
    const needsWork = gal < text.length || en < text.length;
    if (rafRef.current == null && needsWork) {
      // Reset the schedule — start fresh at the next frame.
      nextRevealRef.current = 0;
      rafRef.current = requestAnimationFrame(tick);
    }
    // Note: we intentionally don't cancel/restart when text changes mid-run.
    // The running loop reads targetRef each frame and picks up new text.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [text, animating]);

  // Cleanup on unmount.
  useEffect(() => () => {
    if (rafRef.current != null) {
      cancelAnimationFrame(rafRef.current);
      rafRef.current = null;
    }
  }, []);

  // The rAF tick. Defined after the effects so it can close over the refs
  // without dependency-warning gymnastics. Stable closure across renders.
  function tick(now: number) {
    const c = cursorsRef.current;
    const target = targetRef.current;
    const isGalactic = galacticRef.current;

    // First tick: anchor the schedule to wall-clock so we don't burst-advance.
    if (nextRevealRef.current === 0) nextRevealRef.current = now;

    let advanced = false;

    // Reveal as many chars as the wall-clock schedule allows. At base rate
    // (~45ms/char) this is usually 0–1 chars/frame; at high backlog the loop
    // may emit several chars per frame, but always one-per-scheduled-moment.
    while (
      now >= nextRevealRef.current
      && (c.gal < target.length || c.en < target.length)
    ) {
      const leadBacklog = target.length - c.gal;
      const enCap = isGalactic
        ? (c.gal >= target.length ? target.length : Math.max(0, c.gal - GALACTIC_LEAD))
        : c.gal;

      if (c.gal < target.length) c.gal++;
      if (c.en < enCap) c.en++;

      const rate = Math.min(MAX_RATE, BASE_RATE + leadBacklog * SPEEDUP_PER_BACKLOG_CHAR);
      nextRevealRef.current += 1000 / rate;
      advanced = true;
    }

    // Single re-render per frame regardless of how many chars advanced.
    if (advanced) bump(n => n + 1);

    if (c.gal < target.length || c.en < target.length) {
      rafRef.current = requestAnimationFrame(tick);
    } else {
      rafRef.current = null;
    }
  }

  const { gal, en } = cursorsRef.current;
  const englishPart = text.slice(0, en);
  const galacticPart = galactic
    ? galacticFull.slice(en, gal)
    : text.slice(en, gal);
  const stillTyping = animating && (gal < text.length || en < text.length);

  return (
    <>
      {englishPart}
      {galacticPart.length > 0 && (
        <span className={galactic ? 'gtw-galactic' : undefined}>{galacticPart}</span>
      )}
      {caret && stillTyping && <span className="caret" aria-hidden="true">▍</span>}
    </>
  );
}
