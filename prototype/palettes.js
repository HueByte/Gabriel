// Curated 2-3 stop gradients. stops[0] is the "quiescent" color (rendered when no
// pulse is touching a pixel), stops[last] is the brightest peak of the pulse.
// Black only appears where it is explicitly part of a palette.

const PALETTES = [
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
  // Grok-avatar inspired: deep purple → magenta → hot pink → warm peach.
  { name: 'grok',    stops: [[50, 20, 60],  [200, 40, 130],  [255, 100, 130], [255, 200, 160]] },
];

function pickPalette() {
  return PALETTES[Math.floor(Math.random() * PALETTES.length)];
}

function sampleGradient(stops, t) {
  t = Math.max(0, Math.min(1, t));
  if (stops.length === 1) return stops[0];
  const seg = 1 / (stops.length - 1);
  const i = Math.min(stops.length - 2, Math.floor(t / seg));
  const k = (t - i * seg) / seg;
  const a = stops[i];
  const b = stops[i + 1];
  return [
    Math.round(a[0] + (b[0] - a[0]) * k),
    Math.round(a[1] + (b[1] - a[1]) * k),
    Math.round(a[2] + (b[2] - a[2]) * k),
  ];
}

// Dual export: works as a Node module (require) and as a browser <script> (globals).
if (typeof module !== 'undefined' && module.exports) {
  module.exports = { PALETTES, pickPalette, sampleGradient };
}
