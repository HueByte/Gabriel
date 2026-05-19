// 2-3 stop gradients. stops[0] is the "quiescent" color, stops[last] is the
// brightest peak of the pulse. Black only appears where it is explicitly part
// of a palette ("mono", "void").

import type { Rand } from './rng';
import { mulberry32 } from './rng';

export type RGB = readonly [number, number, number];

export interface Palette {
  name: string;
  stops: RGB[];
}

export const PALETTES: Palette[] = [
  { name: 'heat',    stops: [[40, 0, 10],   [255, 140, 20],  [255, 255, 200]] },
  { name: 'ice',     stops: [[20, 40, 90],  [120, 200, 255], [240, 250, 255]] },
  { name: 'plasma',  stops: [[60, 0, 80],   [220, 40, 160],  [255, 220, 240]] },
  { name: 'matrix',  stops: [[10, 40, 10],  [60, 220, 80],   [220, 255, 200]] },
  { name: 'sunset',  stops: [[100, 20, 90], [240, 90, 60],   [255, 220, 140]] },
  { name: 'ocean',   stops: [[15, 50, 90],  [40, 160, 180],  [200, 250, 240]] },
  { name: 'aurora',  stops: [[30, 20, 60],  [60, 200, 180],  [200, 140, 240]] },
  { name: 'rose',    stops: [[60, 20, 40],  [240, 90, 140],  [255, 220, 230]] },
  { name: 'cyber',   stops: [[40, 0, 80],   [200, 0, 220],   [40, 220, 255]] },
  { name: 'amber',   stops: [[80, 30, 10],  [255, 180, 60]] },
  { name: 'lime',    stops: [[40, 60, 0],   [180, 240, 40]] },
  { name: 'sakura',  stops: [[80, 40, 60],  [255, 180, 200]] },
  { name: 'mono',    stops: [[0, 0, 0],     [180, 180, 180], [255, 255, 255]] },
  { name: 'void',    stops: [[0, 0, 0],     [80, 40, 120],   [200, 160, 255]] },
  { name: 'forge',   stops: [[20, 10, 30],  [220, 80, 40],   [255, 230, 120]] },
  { name: 'alive',   stops: [[50, 20, 60],  [200, 40, 130],  [255, 100, 130], [255, 200, 160]] },
];

export function pickPalette(rng: Rand, name?: string): Palette {
  if (name) {
    const found = PALETTES.find(p => p.name === name);
    if (found) return found;
  }
  return PALETTES[Math.floor(rng() * PALETTES.length)];
}

export function sampleGradient(stops: readonly RGB[], t: number): RGB {
  const clamped = t < 0 ? 0 : t > 1 ? 1 : t;
  if (stops.length === 1) return stops[0];
  const seg = 1 / (stops.length - 1);
  const i = Math.min(stops.length - 2, Math.floor(clamped / seg));
  const k = (clamped - i * seg) / seg;
  const a = stops[i];
  const b = stops[i + 1];
  return [
    Math.round(a[0] + (b[0] - a[0]) * k),
    Math.round(a[1] + (b[1] - a[1]) * k),
    Math.round(a[2] + (b[2] - a[2]) * k),
  ];
}

// Deterministic palette for a given avatar seed. Mirrors the consumption order
// inside Avatar.tsx (palette first, then pattern) so the avatar and any
// UI tinted from the seed agree on the same palette.
export function paletteForSeed(seed: number, name?: string): Palette {
  return pickPalette(mulberry32(seed), name);
}

export function rgbToCss([r, g, b]: RGB, alpha = 1): string {
  return alpha >= 1 ? `rgb(${r} ${g} ${b})` : `rgb(${r} ${g} ${b} / ${alpha})`;
}

// Pick the brightest stop of a palette as a single "accent" color. Used where
// a gradient isn't appropriate (e.g. solid glow color, link tint).
export function paletteAccent(palette: Palette): RGB {
  return brightestStop(palette.stops);
}

export function paletteGradientCss(palette: Palette): string {
  return gradientCssFromStops(palette.stops);
}

// ----- stop-array variants -----
// For palettes that don't come from PALETTES (e.g. server-driven Gabriel
// Sequence palettes), operate on a bare RGB[] directly. Used by App.tsx when
// the sequence response arrives.

export function brightestStop(stops: readonly RGB[]): RGB {
  // Sum-of-channels is a cheap proxy for luminance and produces sensible
  // "most vivid" picks for the small curated palettes we ship and the larger
  // ones the server emits. For the pulse palettes the last stop is already
  // brightest, so existing behavior is preserved.
  let best = stops[0];
  let bestSum = best[0] + best[1] + best[2];
  for (let i = 1; i < stops.length; i++) {
    const s = stops[i];
    const sum = s[0] + s[1] + s[2];
    if (sum > bestSum) { best = s; bestSum = sum; }
  }
  return best;
}

export function gradientCssFromStops(stops: readonly RGB[]): string {
  if (stops.length === 0) return 'transparent';
  if (stops.length === 1) return rgbToCss(stops[0]);
  // Many-stop palettes (server sequences ship 16+) can read as muddy when
  // every color is given equal weight. Sort by brightness and take a
  // representative 5-stop subset so the gradient has clear dim→bright flow.
  const sorted = [...stops].sort((a, b) => (a[0] + a[1] + a[2]) - (b[0] + b[1] + b[2]));
  const sample = sorted.length <= 5
    ? sorted
    : [0, 0.25, 0.5, 0.75, 1].map(t => sorted[Math.round(t * (sorted.length - 1))]);
  const parts = sample.map((s, i) => `${rgbToCss(s)} ${(i / (sample.length - 1)) * 100}%`);
  return `linear-gradient(90deg, ${parts.join(', ')})`;
}

/** Build the three palette CSS variables consumed by .palette-scope. */
export function paletteVarsFromStops(stops: readonly RGB[]): Record<string, string> {
  const accent = brightestStop(stops);
  return {
    '--palette-accent': rgbToCss(accent),
    '--palette-accent-soft': rgbToCss(accent, 0.22),
    '--palette-gradient': gradientCssFromStops(stops),
  };
}
