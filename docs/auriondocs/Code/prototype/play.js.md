# renderFrame

> **File:** `prototype/play.js`  
> **Kind:** function

Renders a 2D numeric grid into a single string of ANSI truecolor characters suitable for printing to a terminal. Each pair of grid rows is drawn as one text row using the upper-half block (▀): the top cell's color is emitted as the foreground (38;2) and the bottom cell's color as the background (48;2). Use this when you want a compact terminal representation of a pixel grid with per-cell colors produced by sampleGradient.

## Remarks
This function packs two pixel rows into one character row by using the Unicode upper-half block, which preserves more vertical resolution than drawing one terminal row per pixel row. It delegates color mapping to sampleGradient(stops, value) so callers control how numeric grid values map to RGB colors. The grid must be rectangular (each row same length) and non-empty.

## Example
```javascript
// Minimal sampleGradient that maps a 0..1 value to grayscale 0..255
function sampleGradient(stops, v) {
  const c = Math.max(0, Math.min(1, v));
  const g = Math.round(c * 255);
  return [g, g, g];
}

const grid = [
  [0, 0.5, 1],
  [1, 0.5, 0],
  [0.25, 0.75, 0.25]
];
const stops = null; // unused by the minimal sampleGradient above

const frame = renderFrame(grid, stops);
console.log(frame);
```

## Notes
- The function assumes grid[0] exists and that every row has the same length; passing an empty grid or jagged rows will cause errors or incorrect output.
- It emits 24-bit (truecolor) ANSI escape sequences; terminals that do not support truecolor will not render accurate colors.
- If the grid has an odd number of rows, the missing bottom pixel for the last row is treated as value 0 (sampleGradient(stops, 0)).