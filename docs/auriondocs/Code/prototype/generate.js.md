# generate.js

> **Source:** `prototype/generate.js`

## Contents

- [generate](#generate)
- [rand](#rand)

---

## generate

> **File:** `prototype/generate.js`  
> **Kind:** function

Generates a sequence of procedurally sampled grayscale frames from a chosen pattern and palette, applying per-pixel noise and an ambient floor. Use this when you need a packaged set of frame grids (and metadata) for playback or export rather than sampling a pattern manually.

## Remarks
This function centralizes pattern initialization, per-frame time-stepping, and post-processing (noise + ambient floor) so callers get a ready-to-use stream of normalized pixel values. Patterns are selected via the CLI argument passed to pickPattern, initialized with SIZE, and sampled for each (x,y) across FRAMES; a synthetic playback time (1/20s per frame) is supplied so stateful patterns can advance consistently. The returned meta object contains the parameters needed to reproduce or describe the output.

## Example
```javascript
// Basic usage in a Node.js script
const { writeFileSync } = require('fs');
const result = generate();
console.log(result.meta);
// Persist full output for a player that expects JSON
writeFileSync('out.json', JSON.stringify(result));
```

## Notes
- The function expects global constants SIZE and FRAMES to be defined; each frame is a SIZE x SIZE grid.
- Returned structure: { meta, frames } where frames is an array of frames, each frame is an array of rows, and each row is an array of floats in [0,1] indexed by [frame][y][x].
- Noise amplitude and ambient floor are randomized per-run (noiseAmp in ~[0.0,0.12], ambient in ~[0.05,0.18]), so outputs are not deterministic unless upstream randomness is controlled.
- Pixel values are clamped to [0,1] after noise and ambient adjustments.
- Time passed to patterns is computed as i * (1/20) seconds; change the synthetic framerate by adjusting SECONDS_PER_FRAME if consumers expect a different playback rate.
- The pattern API must provide init(size) -> params and sample(t,x,y,params,time) -> value; pattern behavior (stateless vs. stateful) depends on its implementation.


---

## rand

> **File:** `prototype/generate.js`  
> **Kind:** function

Returns a pseudo‑random floating‑point number uniformly distributed between min (inclusive) and max (exclusive). Use this when you need a random fractional value within a numeric range; it is a thin helper that scales Math.random().

## Remarks
This is a simple scaling of Math.random() into an arbitrary interval: it computes min + Math.random() * (max - min). It produces IEEE‑754 double values (floating point) and is not suitable for cryptographic purposes. For integer results or inclusive upper bounds, apply additional rounding or adjustments outside this helper.

## Example
```javascript
// random float in [0, 1):
rand(0, 1);

// random float in [10, 20):
const value = rand(10, 20);
console.log(value);

// random integer in [10, 20] (inclusive):
function randIntInclusive(min, max) {
  // use Math.floor and add 1 to include max
  return Math.floor(rand(min, max + 1));
}
```

## Notes
- The range is half‑open: min is included, max is excluded because Math.random() returns values in [0, 1).
- Inputs are treated as numbers via JS arithmetic; non‑numeric arguments may produce NaN. Prefer passing numeric values.
- Not suitable for cryptographic use or for statistically secure randomness; use the Web Crypto API (crypto.getRandomValues) if required.

---