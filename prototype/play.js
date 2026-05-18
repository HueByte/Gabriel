// Loads frames.json and animates it in the terminal using 24-bit ANSI colors.
// Uses the half-block trick (▀) so each cell renders ~square instead of 2:1 tall.

const fs = require('fs');
const path = require('path');
const { sampleGradient } = require('./palettes');

const FPS = 20;
const FRAME_MS = 1000 / FPS;

function renderFrame(grid, stops) {
  const h = grid.length;
  const w = grid[0].length;
  let out = '';
  for (let y = 0; y < h; y += 2) {
    for (let x = 0; x < w; x++) {
      const [tr, tg, tb] = sampleGradient(stops, grid[y][x]);
      const bottom = y + 1 < h ? grid[y + 1][x] : 0;
      const [br, bg, bb] = sampleGradient(stops, bottom);
      // Foreground = top half, background = bottom half, char = upper half block.
      out += `\x1b[38;2;${tr};${tg};${tb}m\x1b[48;2;${br};${bg};${bb}m▀`;
    }
    out += '\x1b[0m\n';
  }
  return out;
}

const data = JSON.parse(fs.readFileSync(path.join(__dirname, 'frames.json'), 'utf8'));
const frames = data.frames;
const stops = data.meta.palette.stops;

process.stdout.write('\x1b[?25l'); // hide cursor
process.on('SIGINT', () => {
  process.stdout.write('\x1b[?25h\x1b[0m\n');
  process.exit(0);
});

let i = 0;
console.clear();
console.log(`pattern: ${data.meta.pattern}   palette: ${data.meta.palette.name}`);
setInterval(() => {
  process.stdout.write('\x1b[H\n'); // cursor home, skip palette label line
  process.stdout.write(renderFrame(frames[i % frames.length], stops));
  i++;
}, FRAME_MS);
