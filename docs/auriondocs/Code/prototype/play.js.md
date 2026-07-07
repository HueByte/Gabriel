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


Renders a 2D grid into a colored terminal representation by pairing rows and using the upper-half block glyph to blend top and bottom colors. It returns a string of ANSI truecolor codes that can be printed to render a heatmap-like visualization in a terminal.

## Remarks
By processing two rows at a time, this function achieves higher vertical density than rendering each cell individually while remaining text-based. It delegates the color mapping to sampleGradient, so swapping color stops changes the visualization without altering the rendering logic. The approach relies on 24-bit color escape sequences to express precise RGB colors in the terminal.

## Notes
- Requires terminal support for truecolor (ANSI 38;2 / 48;2 sequences). If unsupported, colors may render incorrectly or be ignored.
- If the grid height is odd, the bottom color falls back to 0 as a default.
- Large grids can incur noticeable performance costs due to repeated gradient sampling and string concatenation; consider caching results or reducing grid resolution for frequent renders.