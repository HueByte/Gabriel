import { useEffect, useRef, useState } from 'react';
import { Markdown, toGalactic, GAL_OPEN, GAL_CLOSE } from './Markdown';

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
 * The revealed text is rendered through markdown so formatting (links, lists,
 * code, hr, hex swatches) snaps in as chars cross. The galactic trail is
 * appended as a sentinel-wrapped slice; remarkInlineEnrichments lifts it into
 * a styled span inside the markdown AST so it flows inline at the end of the
 * last paragraph rather than dangling after block elements.
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
  const nextRevealRef = useRef(0);
  const rafRef = useRef<number | null>(null);
  const [, bump] = useState(0);

  useEffect(() => {
    targetRef.current = text;

    if (!animating) {
      cursorsRef.current.gal = text.length;
      cursorsRef.current.en = text.length;
      bump(n => n + 1);
      return;
    }

    if (cursorsRef.current.gal > text.length) cursorsRef.current.gal = text.length;
    if (cursorsRef.current.en > text.length) cursorsRef.current.en = text.length;

    const { gal, en } = cursorsRef.current;
    const needsWork = gal < text.length || en < text.length;
    if (rafRef.current == null && needsWork) {
      nextRevealRef.current = 0;
      rafRef.current = requestAnimationFrame(tick);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [text, animating]);

  useEffect(() => () => {
    if (rafRef.current != null) {
      cancelAnimationFrame(rafRef.current);
      rafRef.current = null;
    }
  }, []);

  function tick(now: number) {
    const c = cursorsRef.current;
    const target = targetRef.current;
    const isGalactic = galacticRef.current;

    if (nextRevealRef.current === 0) nextRevealRef.current = now;

    let advanced = false;

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

    if (advanced) bump(n => n + 1);

    if (c.gal < target.length || c.en < target.length) {
      rafRef.current = requestAnimationFrame(tick);
    } else {
      rafRef.current = null;
    }
  }

  const { gal, en } = cursorsRef.current;
  const englishPart = text.slice(0, en);
  const trailRaw = text.slice(en, gal);
  const trail = galactic ? toGalactic(trailRaw) : trailRaw;
  const stillTyping = animating && (gal < text.length || en < text.length);

  // Build markdown source: english prefix + sentinel-wrapped trail (so the
  // remark plugin pulls the trail into the AST as a styled span inside the
  // last inline context).
  const markdownSource = trail.length > 0
    ? `${englishPart}${GAL_OPEN}${trail}${GAL_CLOSE}`
    : englishPart;

  return (
    <>
      {/* `streaming` tells Markdown to skip rehype-highlight and rehype-katex
          while the typewriter is still revealing chars — those plugins would
          otherwise re-run on every tick (up to 200/sec). Colors and math
          snap in the instant typing finishes. */}
      <Markdown text={markdownSource} streaming={stillTyping} />
      {caret && stillTyping && <span className="caret" aria-hidden="true">▍</span>}
    </>
  );
}
