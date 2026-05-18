// Generates a randomized 16x16 animation and saves it as frames.json.
// Each frame is a 16x16 array of intensities in [0, 1].
// Usage: node generate.js [patternName]   (omit to pick randomly)

const fs = require('fs');
const path = require('path');
const { pickPalette } = require('./palettes');
const { pickPattern } = require('./patterns');

const FRAMES_PATH = path.join(__dirname, 'frames.json');

const SIZE = 16;
const FRAMES = 32;

function rand(min, max) {
  return min + Math.random() * (max - min);
}

function generate() {
  const palette = pickPalette();
  const { name: patternId, def: pattern } = pickPattern(process.argv[2]);
  const params = pattern.init(SIZE);
  const noiseAmp = rand(0.0, 0.12);
  // Ambient floor so quiescent pixels sit on the gradient's low color, not stop[0] hard.
  const ambient = rand(0.05, 0.18);

  const frames = [];
  // Synthetic playback time per frame — assumes play.js renders at 20fps.
  // Stateful patterns (e.g. shimmer) read this to advance per-pixel transitions.
  const SECONDS_PER_FRAME = 1 / 20;
  for (let i = 0; i < FRAMES; i++) {
    const t = i / FRAMES;
    const time = i * SECONDS_PER_FRAME;
    const grid = [];
    for (let y = 0; y < SIZE; y++) {
      const row = [];
      for (let x = 0; x < SIZE; x++) {
        let v = pattern.sample(t, x, y, params, time);
        v += (Math.random() - 0.5) * noiseAmp;
        v = ambient + v * (1 - ambient);
        row.push(Math.max(0, Math.min(1, v)));
      }
      grid.push(row);
    }
    frames.push(grid);
  }

  return {
    meta: { size: SIZE, frames: FRAMES, pattern: patternId, params, noiseAmp, ambient, palette },
    frames,
  };
}

const data = generate();
fs.writeFileSync(FRAMES_PATH, JSON.stringify(data));
console.log(`Wrote frames.json — ${data.meta.frames} frames @ ${data.meta.size}x${data.meta.size}`);
console.log(`Pattern: ${data.meta.pattern}   Palette: ${data.meta.palette.name}`);
