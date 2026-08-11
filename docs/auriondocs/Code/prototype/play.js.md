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


Renders a terminal frame by turning a 2D grid of color indices into a colored text image. It processes two rows at a time: the top row colors are used as the foreground, the bottom row colors as the background, and a single Unicode upper-half block character ('▀') combines them into one cell. Each color is produced by sampling via sampleGradient(stops, value), and emitted as 24-bit ANSI color codes (foreground 38;2;R;G;B and background 48;2;R;G;B). The function returns the assembled string, which can be printed to the terminal to display a gradient frame.

## Remarks
Separating gradient sampling from the frame assembly lets you reuse renderFrame with different grids or color stops without touching the rendering logic. It exploits true color ANSI escapes and the Unicode upper-half block to achieve two colors per character cell, yielding higher vertical fidelity than a single-color per cell approach. The routine assumes a rectangular grid and relies on sampleGradient for actual RGB computation from the stops array.

## Notes
- Terminal support: requires 24-bit color (true color) and Unicode block characters for correct rendering.
- Odd height handling: if the grid height is odd, the last top row renders with a default bottom color (0); ensure your input accounts for this or handle it as needed.
- Color reset behavior: the code appends an ANSI reset after each line to avoid color leakage into subsequent output.