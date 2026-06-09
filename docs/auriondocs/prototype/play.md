# renderFrame

> **File:** `prototype/play.js`  
> **Kind:** function

Converts a 2D numeric grid into a single string of ANSI 24-bit color escape sequences that, when printed to a truecolor terminal, draws the frame using the Unicode upper-half block (▀). Each output character encodes two vertical samples: the top cell is drawn as the foreground color and the bottom cell as the background color. Use this when you need a compact terminal rendering of a numeric heatmap or frame buffer and you have a sampleGradient(stops, value) function that maps numeric samples to [r,g,b] colors.

## Remarks
This function pairs rows (y and y+1) and emits one character per pair and column, effectively doubling vertical sampling density compared with single-character-per-sample approaches. It delegates color mapping to sampleGradient so the same rendering logic can be used with arbitrary gradient definitions (the stops parameter). The output resets colors at the end of each line.

## Example
```javascript
// Minimal example: a grayscale sampleGradient and a 3x4 grid
function sampleGradient(stops, v) {
  // simple mapping: treat v as 0..255 and return gray RGB
  const c = Math.max(0, Math.min(255, Math.round(v)));
  return [c, c, c];
}

const grid = [
  [0, 64, 128, 192],
  [32, 96, 160, 224],
  [16, 80, 144, 208],
  [48, 112, 176, 240]
];

const stops = null; // unused by this simple sampleGradient
console.log(renderFrame(grid, stops));
```

## Notes
- The terminal must support 24-bit (truecolor) ANSI escape sequences and the '▀' glyph for the output to look correct.
- grid must be non-empty and rectangular (each row the same width). The function reads grid[0] for width without checking.
- If the grid has an odd number of rows, the final row's bottom sample is treated as 0 (black).
- sampleGradient(stops, value) must return an [r, g, b] array of integers (0–255); heavy or synchronous gradient computations may affect rendering performance for large grids.