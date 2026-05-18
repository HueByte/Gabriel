// Animation patterns. Each pattern has:
//   init(size) -> params  (called once at generation time, may randomize)
//   sample(t, x, y, params) -> intensity in [0, 1]
// t is the normalized frame phase in [0, 1). All patterns are designed to loop
// seamlessly at t=1 -> t=0.

const TWO_PI = Math.PI * 2;

function rand(min, max) { return min + Math.random() * (max - min); }
function pick(arr) { return arr[Math.floor(Math.random() * arr.length)]; }

// --- Value noise (Perlin-ish, cheap, no gradient table) ---------------------
function hash2(x, y, seed) {
  let h = Math.imul(x | 0, 374761393) + Math.imul(y | 0, 668265263) + Math.imul(seed | 0, 982451653);
  h = Math.imul(h ^ (h >>> 13), 1274126177);
  return ((h ^ (h >>> 16)) >>> 0) / 4294967295;
}
function smooth(t) { return t * t * (3 - 2 * t); }
function valueNoise(x, y, seed) {
  const xi = Math.floor(x), yi = Math.floor(y);
  const xf = x - xi, yf = y - yi;
  const a = hash2(xi, yi, seed);
  const b = hash2(xi + 1, yi, seed);
  const c = hash2(xi, yi + 1, seed);
  const d = hash2(xi + 1, yi + 1, seed);
  const u = smooth(xf), v = smooth(yf);
  return a * (1 - u) * (1 - v) + b * u * (1 - v) + c * (1 - u) * v + d * u * v;
}
function fbm(x, y, seed, octaves) {
  let sum = 0, amp = 1, total = 0, freq = 1;
  for (let i = 0; i < octaves; i++) {
    sum += valueNoise(x * freq, y * freq, seed + i * 17) * amp;
    total += amp;
    amp *= 0.5;
    freq *= 2;
  }
  return sum / total;
}

// --- Patterns ---------------------------------------------------------------

const PATTERNS = {
  // Expanding radial ripples from a near-center point.
  pulse: {
    init(size) {
      const waveWidth = rand(1.8, 3.2);
      return {
        cx: rand(size / 2 - 1, size / 2 + 1),
        cy: rand(size / 2 - 1, size / 2 + 1),
        waveWidth,
        ripples: pick([1, 2]),
        maxRadius: Math.hypot(size, size) / 2 + waveWidth,
      };
    },
    sample(t, x, y, p) {
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
  },

  // Drifting cloudy fBm noise field. Loops by orbiting the sample point in noise space.
  noise: {
    init() {
      return {
        scale: rand(0.18, 0.32),
        loopR: rand(0.6, 1.4),
        seed: Math.floor(Math.random() * 1e6),
        octaves: 3,
      };
    },
    sample(t, x, y, p) {
      const ox = Math.cos(TWO_PI * t) * p.loopR;
      const oy = Math.sin(TWO_PI * t) * p.loopR;
      const n = fbm(x * p.scale + ox, y * p.scale + oy, p.seed, p.octaves);
      return Math.max(0, Math.min(1, (n - 0.2) / 0.6));
    },
  },

  // Traveling sine waves at a random angle.
  waves: {
    init(size) {
      return {
        angle: rand(0, TWO_PI),
        freq: rand(0.6, 1.4),
        speed: pick([1, -1]),
        sharpness: rand(1.2, 2.4),
        cx: size / 2 - 0.5,
        cy: size / 2 - 0.5,
      };
    },
    sample(t, x, y, p) {
      const proj = (x - p.cx) * Math.cos(p.angle) + (y - p.cy) * Math.sin(p.angle);
      const s = Math.sin(proj * p.freq + p.speed * TWO_PI * t);
      return Math.pow((s + 1) / 2, p.sharpness);
    },
  },

  // Soft bright bands sweeping diagonally and looping by modulo.
  flow: {
    init(size) {
      return {
        angle: rand(0, TWO_PI),
        bandWidth: rand(2.5, 4.5),
        bands: pick([2, 3]),
        period: rand(10, 16),
        speed: pick([1, -1]),
        cx: size / 2 - 0.5,
        cy: size / 2 - 0.5,
      };
    },
    sample(t, x, y, p) {
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
  },

  // Rotating spiral arms.
  spiral: {
    init(size) {
      return {
        arms: pick([1, 2, 3]),
        tightness: rand(0.5, 1.1),
        speed: pick([1, -1]),
        sharpness: rand(1.4, 2.4),
        cx: size / 2 - 0.5,
        cy: size / 2 - 0.5,
      };
    },
    sample(t, x, y, p) {
      const dx = x - p.cx, dy = y - p.cy;
      const theta = Math.atan2(dy, dx);
      const dist = Math.hypot(dx, dy);
      const s = Math.sin(theta * p.arms + dist * p.tightness - p.speed * TWO_PI * t);
      return Math.pow((s + 1) / 2, p.sharpness);
    },
  },

  // Per-pixel smooth color drift on independent random schedules.
  // Each pixel holds a from/to pair and eases between them over a random duration;
  // when it arrives, it picks a new target and a new duration. Requires `time`
  // (in seconds) — `t` is loop-normalized and won't carry across a wrap.
  shimmer: {
    init(size) {
      const minDur = 0.9;
      const maxDur = 2.8;
      const pixels = [];
      for (let y = 0; y < size; y++) {
        const row = [];
        for (let x = 0; x < size; x++) {
          const duration = rand(minDur, maxDur);
          row.push({
            from: Math.random(),
            to: Math.random(),
            // Negative stagger so pixels are already mid-transition at time=0 — no synchronized start.
            startTime: -rand(0, duration),
            duration,
          });
        }
        pixels.push(row);
      }
      return { pixels, minDur, maxDur };
    },
    sample(_t, x, y, p, time) {
      const px = p.pixels[y][x];
      // Advance through any transitions that finished since the last sample.
      while (time - px.startTime >= px.duration) {
        px.startTime += px.duration;
        px.from = px.to;
        px.to = Math.random();
        px.duration = rand(p.minDur, p.maxDur);
      }
      const progress = (time - px.startTime) / px.duration;
      const eased = progress * progress * (3 - 2 * progress);
      return px.from + (px.to - px.from) * eased;
    },
  },

  // Classic demoscene plasma. Integer frequency multipliers keep the loop seamless.
  plasma: {
    init(size) {
      return {
        a: rand(0.35, 0.7),
        b: rand(0.35, 0.7),
        c: rand(0.25, 0.6),
        d: rand(0.4, 0.8),
        sa: pick([1, -1, 2, -2]),
        sb: pick([1, -1, 2]),
        sc: pick([1, -1]),
        sd: pick([1, 2]),
        cx: size / 2 - 0.5,
        cy: size / 2 - 0.5,
      };
    },
    sample(t, x, y, p) {
      const phase = TWO_PI * t;
      const v = (
        Math.sin(x * p.a + phase * p.sa) +
        Math.sin(y * p.b + phase * p.sb) +
        Math.sin((x + y) * p.c + phase * p.sc) +
        Math.sin(Math.hypot(x - p.cx, y - p.cy) * p.d + phase * p.sd)
      ) / 4;
      return (v + 1) / 2;
    },
  },
};

function pickPattern(name) {
  if (name && PATTERNS[name]) return { name, def: PATTERNS[name] };
  if (name) {
    console.warn(`Unknown pattern "${name}". Available: ${Object.keys(PATTERNS).join(', ')}. Picking randomly.`);
  }
  const names = Object.keys(PATTERNS);
  const chosen = names[Math.floor(Math.random() * names.length)];
  return { name: chosen, def: PATTERNS[chosen] };
}

// Dual export: works as a Node module (require) and as a browser <script> (globals).
if (typeof module !== 'undefined' && module.exports) {
  module.exports = { PATTERNS, pickPattern };
}
