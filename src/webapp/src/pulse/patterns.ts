// Animation patterns. Each pattern has:
//   init(size, rng) -> params  (called once, may randomize via the supplied rng)
//   sample(t, x, y, params, time) -> intensity in [0, 1]
// `t` is loop-normalized in [0, 1) and loops seamlessly at the boundary.
// `time` is monotonic real seconds; only patterns with per-pixel state need it.
// All randomness flows from the `Rand` passed into init (and stored on params
// when a pattern needs more randomness during sampling), so a given seed
// reproduces the same visual.

import type { Rand } from './rng';

const TWO_PI = Math.PI * 2;

const range = (rng: Rand, min: number, max: number) => min + rng() * (max - min);
const pick = <T>(rng: Rand, arr: readonly T[]): T => arr[Math.floor(rng() * arr.length)];

// Value noise --------------------------------------------------------------

function hash2(x: number, y: number, seed: number): number {
  let h = Math.imul(x | 0, 374761393) + Math.imul(y | 0, 668265263) + Math.imul(seed | 0, 982451653);
  h = Math.imul(h ^ (h >>> 13), 1274126177);
  return ((h ^ (h >>> 16)) >>> 0) / 4294967295;
}
const smooth = (t: number) => t * t * (3 - 2 * t);
function valueNoise(x: number, y: number, seed: number): number {
  const xi = Math.floor(x), yi = Math.floor(y);
  const xf = x - xi, yf = y - yi;
  const a = hash2(xi, yi, seed);
  const b = hash2(xi + 1, yi, seed);
  const c = hash2(xi, yi + 1, seed);
  const d = hash2(xi + 1, yi + 1, seed);
  const u = smooth(xf), v = smooth(yf);
  return a * (1 - u) * (1 - v) + b * u * (1 - v) + c * (1 - u) * v + d * u * v;
}
function fbm(x: number, y: number, seed: number, octaves: number): number {
  let sum = 0, amp = 1, total = 0, freq = 1;
  for (let i = 0; i < octaves; i++) {
    sum += valueNoise(x * freq, y * freq, seed + i * 17) * amp;
    total += amp;
    amp *= 0.5;
    freq *= 2;
  }
  return sum / total;
}

// Pattern interface --------------------------------------------------------

export interface Pattern<P = unknown> {
  init(size: number, rng: Rand): P;
  sample(t: number, x: number, y: number, params: P, time: number): number;
}

// Patterns -----------------------------------------------------------------

interface PulseParams { cx: number; cy: number; waveWidth: number; ripples: number; maxRadius: number; }
const pulse: Pattern<PulseParams> = {
  init(size, rng) {
    const waveWidth = range(rng, 1.8, 3.2);
    return {
      cx: range(rng, size / 2 - 1, size / 2 + 1),
      cy: range(rng, size / 2 - 1, size / 2 + 1),
      waveWidth,
      ripples: pick(rng, [1, 2]),
      maxRadius: Math.hypot(size, size) / 2 + waveWidth,
    };
  },
  sample(t, x, y, p, _time) {
    const dist = Math.hypot(x - p.cx + 0.5, y - p.cy + 0.5);
    let v = 0;
    for (let r = 0; r < p.ripples; r++) {
      const rPhase = (t + r / p.ripples) % 1;
      const waveR = rPhase * p.maxRadius;
      const delta = Math.abs(dist - waveR);
      const falloff = Math.max(0, 1 - delta / p.waveWidth);
      const trail = dist < waveR ? 0.7 : 1.0;
      v = Math.max(v, falloff * trail);
    }
    return v;
  },
};

interface NoiseParams { scale: number; loopR: number; seed: number; octaves: number; }
const noise: Pattern<NoiseParams> = {
  init(_size, rng) {
    return {
      scale: range(rng, 0.18, 0.32),
      loopR: range(rng, 0.6, 1.4),
      seed: Math.floor(rng() * 1e6),
      octaves: 3,
    };
  },
  sample(t, x, y, p, _time) {
    const ox = Math.cos(TWO_PI * t) * p.loopR;
    const oy = Math.sin(TWO_PI * t) * p.loopR;
    const n = fbm(x * p.scale + ox, y * p.scale + oy, p.seed, p.octaves);
    return Math.max(0, Math.min(1, (n - 0.2) / 0.6));
  },
};

interface WavesParams { angle: number; freq: number; speed: number; sharpness: number; cx: number; cy: number; }
const waves: Pattern<WavesParams> = {
  init(size, rng) {
    return {
      angle: range(rng, 0, TWO_PI),
      freq: range(rng, 0.6, 1.4),
      speed: pick(rng, [1, -1]),
      sharpness: range(rng, 1.2, 2.4),
      cx: size / 2 - 0.5,
      cy: size / 2 - 0.5,
    };
  },
  sample(t, x, y, p, _time) {
    const proj = (x - p.cx) * Math.cos(p.angle) + (y - p.cy) * Math.sin(p.angle);
    const s = Math.sin(proj * p.freq + p.speed * TWO_PI * t);
    return Math.pow((s + 1) / 2, p.sharpness);
  },
};

interface FlowParams { angle: number; bandWidth: number; bands: number; period: number; speed: number; cx: number; cy: number; }
const flow: Pattern<FlowParams> = {
  init(size, rng) {
    return {
      angle: range(rng, 0, TWO_PI),
      bandWidth: range(rng, 2.5, 4.5),
      bands: pick(rng, [2, 3]),
      period: range(rng, 10, 16),
      speed: pick(rng, [1, -1]),
      cx: size / 2 - 0.5,
      cy: size / 2 - 0.5,
    };
  },
  sample(t, x, y, p, _time) {
    const proj = (x - p.cx) * Math.cos(p.angle) + (y - p.cy) * Math.sin(p.angle);
    const norm = ((proj + p.speed * t * p.period) % p.period + p.period) % p.period;
    let v = 0;
    for (let b = 0; b < p.bands; b++) {
      const bandPos = (b * p.period) / p.bands;
      const raw = Math.abs(norm - bandPos);
      const d = Math.min(raw, p.period - raw);
      v = Math.max(v, Math.max(0, 1 - d / p.bandWidth));
    }
    return v;
  },
};

interface SpiralParams { arms: number; tightness: number; speed: number; sharpness: number; cx: number; cy: number; }
const spiral: Pattern<SpiralParams> = {
  init(size, rng) {
    return {
      arms: pick(rng, [1, 2, 3]),
      tightness: range(rng, 0.5, 1.1),
      speed: pick(rng, [1, -1]),
      sharpness: range(rng, 1.4, 2.4),
      cx: size / 2 - 0.5,
      cy: size / 2 - 0.5,
    };
  },
  sample(t, x, y, p, _time) {
    const dx = x - p.cx, dy = y - p.cy;
    const theta = Math.atan2(dy, dx);
    const dist = Math.hypot(dx, dy);
    const s = Math.sin(theta * p.arms + dist * p.tightness - p.speed * TWO_PI * t);
    return Math.pow((s + 1) / 2, p.sharpness);
  },
};

interface PlasmaParams { a: number; b: number; c: number; d: number; sa: number; sb: number; sc: number; sd: number; cx: number; cy: number; }
const plasma: Pattern<PlasmaParams> = {
  init(size, rng) {
    return {
      a: range(rng, 0.35, 0.7),
      b: range(rng, 0.35, 0.7),
      c: range(rng, 0.25, 0.6),
      d: range(rng, 0.4, 0.8),
      sa: pick(rng, [1, -1, 2, -2]),
      sb: pick(rng, [1, -1, 2]),
      sc: pick(rng, [1, -1]),
      sd: pick(rng, [1, 2]),
      cx: size / 2 - 0.5,
      cy: size / 2 - 0.5,
    };
  },
  sample(t, x, y, p, _time) {
    const phase = TWO_PI * t;
    const v = (
      Math.sin(x * p.a + phase * p.sa) +
      Math.sin(y * p.b + phase * p.sb) +
      Math.sin((x + y) * p.c + phase * p.sc) +
      Math.sin(Math.hypot(x - p.cx, y - p.cy) * p.d + phase * p.sd)
    ) / 4;
    return (v + 1) / 2;
  },
};

interface ShimmerCell { from: number; to: number; startTime: number; duration: number; }
interface ShimmerParams { pixels: ShimmerCell[][]; minDur: number; maxDur: number; rng: Rand; }
const shimmer: Pattern<ShimmerParams> = {
  init(size, rng) {
    const minDur = 0.9;
    const maxDur = 2.8;
    const pixels: ShimmerCell[][] = [];
    for (let y = 0; y < size; y++) {
      const row: ShimmerCell[] = [];
      for (let x = 0; x < size; x++) {
        const duration = range(rng, minDur, maxDur);
        row.push({
          from: rng(),
          to: rng(),
          startTime: -range(rng, 0, duration),
          duration,
        });
      }
      pixels.push(row);
    }
    return { pixels, minDur, maxDur, rng };
  },
  sample(_t, x, y, p, time) {
    const px = p.pixels[y][x];
    while (time - px.startTime >= px.duration) {
      px.startTime += px.duration;
      px.from = px.to;
      px.to = p.rng();
      px.duration = range(p.rng, p.minDur, p.maxDur);
    }
    const progress = (time - px.startTime) / px.duration;
    const eased = progress * progress * (3 - 2 * progress);
    return px.from + (px.to - px.from) * eased;
  },
};

export const PATTERNS = { pulse, noise, waves, flow, spiral, plasma, shimmer } as const;
export type PatternName = keyof typeof PATTERNS;

export function pickPattern(rng: Rand, name?: PatternName): { name: PatternName; def: Pattern } {
  const keys = Object.keys(PATTERNS) as PatternName[];
  const chosen = name && PATTERNS[name] ? name : keys[Math.floor(rng() * keys.length)];
  return { name: chosen, def: PATTERNS[chosen] as Pattern };
}
