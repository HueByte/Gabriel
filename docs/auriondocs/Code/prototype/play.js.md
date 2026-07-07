# renderFrame

> **File:** `prototype/play.js`  
> **Kind:** function

```javascript
function renderFrame(grid, stops)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `grid` | — | — |
| `stops` | — | — |


Renders a frame by turning a 2D grid into a colored ASCII-art representation for a terminal. It walks the grid two rows at a time, using the top cell's color as the foreground and the immediately following bottom cell's color as the background, with the upper-half block character (▀) composing both colors in a single character. The return value is a string containing ANSI 24-bit color codes; calling code can emit it to the terminal or capture it for further processing.

## Remarks
Separates color mapping from I/O by delegating the actual color computation to the gradient sampler and confines rendering to a reusable frame-assembly routine. This pattern enables reusing renderFrame with different grid sources or gradient definitions while keeping terminal output concerns isolated from data preparation.

## Example
```javascript
// Example usage: render a tiny 2x2 grid
const grid = [
  [0, 1],
  [2, 3]
];
const stops = [
  { pos: 0, color: [0, 0, 0] },
  { pos: 1, color: [255, 255, 255] }
];
const frame = renderFrame(grid, stops);
console.log(frame);
```

## Notes
- Requires terminal support for ANSI 24-bit color and the block glyph U+2580 (upper-half block); otherwise colors may render incorrectly.
- If the grid height is odd, the last row pairs with a bottom color of 0, which can yield unexpected hues on the final line.