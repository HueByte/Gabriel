import { useEffect, useRef, useState } from 'react';

// Standard Galactic-style cipher: A–Z → Tifinagh glyphs.
// Tifinagh's geometric strokes read as "alien script" without needing a custom font file.
// Non-letters pass through untouched so spaces, punctuation, and numbers preserve shape.
const GAL_MAP: Record<string, string> = {
  A: 'ⴰ', B: 'ⴱ', C: 'ⵛ', D: 'ⴷ', E: 'ⴻ', F: 'ⴼ', G: 'ⴳ',
  H: 'ⵀ', I: 'ⵉ', J: 'ⵊ', K: 'ⴽ', L: 'ⵍ', M: 'ⵎ', N: 'ⵏ',
  O: 'ⵄ', P: 'ⵒ', Q: 'ⵇ', R: 'ⵔ', S: 'ⵙ', T: 'ⵟ', U: 'ⵓ',
  V: 'ⵠ', W: 'ⵡ', X: 'ⵅ', Y: 'ⵢ', Z: 'ⵣ',
};

function toGalactic(ch: string): string {
  return GAL_MAP[ch.toUpperCase()] ?? ch;
}

interface Props {
  text: string;
  /** ms per character during both passes */
  charMs?: number;
  /** delay between the galactic pass finishing and the english pass starting */
  pauseMs?: number;
  onDone?: () => void;
}

/**
 * Two-pass typewriter: first reveals `text` rendered in a galactic cipher, then
 * a short pause, then a second pass overwrites each character with its English
 * form left-to-right — as if the message were being translated in place.
 */
export function GalacticTypewriter({ text, charMs = 22, pauseMs = 450, onDone }: Props) {
  const [galCount, setGalCount] = useState(0);
  const [enCount, setEnCount] = useState(0);
  const onDoneRef = useRef(onDone);
  onDoneRef.current = onDone;

  useEffect(() => {
    setGalCount(0);
    setEnCount(0);

    const len = text.length;
    if (len === 0) {
      onDoneRef.current?.();
      return;
    }

    let galTimer: ReturnType<typeof setInterval> | null = null;
    let enTimer: ReturnType<typeof setInterval> | null = null;
    let pauseTimer: ReturnType<typeof setTimeout> | null = null;

    let i = 0;
    galTimer = setInterval(() => {
      i += 1;
      setGalCount(Math.min(i, len));
      if (i >= len) {
        if (galTimer) clearInterval(galTimer);
        pauseTimer = setTimeout(() => {
          let j = 0;
          enTimer = setInterval(() => {
            j += 1;
            setEnCount(Math.min(j, len));
            if (j >= len) {
              if (enTimer) clearInterval(enTimer);
              onDoneRef.current?.();
            }
          }, charMs);
        }, pauseMs);
      }
    }, charMs);

    return () => {
      if (galTimer) clearInterval(galTimer);
      if (enTimer) clearInterval(enTimer);
      if (pauseTimer) clearTimeout(pauseTimer);
    };
  }, [text, charMs, pauseMs]);

  const englishPart = text.slice(0, enCount);
  const galacticPart = text
    .slice(enCount, galCount)
    .split('')
    .map(toGalactic)
    .join('');
  const showCaret = enCount < text.length;

  return (
    <span className="gtw">
      <span>{englishPart}</span>
      <span className="gtw-galactic">{galacticPart}</span>
      {showCaret && <span className="gtw-caret" aria-hidden="true">▍</span>}
    </span>
  );
}
