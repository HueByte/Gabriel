# generate.js

> **Source:** `prototype/generate.js`

## Contents

- [generate](#generate)
- [rand](#rand)

---

## generate

> **File:** `prototype/generate.js`  
> **Kind:** function

Generates a sequence of SIZE×SIZE grayscale frames using a selected pattern and palette, returning both per-frame pixel grids and metadata. Use this when you need a ready-to-render frameset (for play.js-style playback) produced from a pattern chosen via the process arguments and mildly randomized by per-frame noise and an ambient floor.

## Remarks
generate coordinates a few responsibilities: it selects a palette and a pattern (via pickPalette and pickPattern), initializes pattern-specific parameters, and then samples the pattern across a SIZE×SIZE grid for FRAMES steps. Each sample is jittered by a small random noise amplitude and pushed toward an ambient floor so quiescent pixels don't sit exactly at the gradient's minimum. The function also supplies a synthetic playback-time value (seconds) to support stateful patterns that advance over real time; that time is based on a fixed 20 FPS assumption.

## Example
```javascript
// run in Node: node script.js <patternName?>
const result = generate();
console.log(result.meta); // { size, frames, pattern, params, noiseAmp, ambient, palette }
console.log(result.frames.length); // equals meta.frames (FRAMES)
console.log(result.frames[0][0][0]); // sample pixel value in first frame
```

## Notes
- generate depends on global constants SIZE and FRAMES; ensure these are set before calling.
- The pattern is selected using process.argv[2]; calling with different CLI args yields different pattern choices.
- Output is non-deterministic: ambient, noiseAmp and per-pixel noise are randomized and there is no built-in seed control.
- Time supplied to patterns assumes playback at 20 FPS (SECONDS_PER_FRAME = 1/20); change this constant if your renderer uses a different frame rate.

---

## rand

> **File:** `prototype/generate.js`  
> **Kind:** function

Returns a uniformly distributed floating-point number between min (inclusive) and max (exclusive). Use this when you need a simple pseudo-random floating value inside a half-open interval; for integer results or cryptographically secure randomness, use additional conversion or a different source.

## Remarks
This is a thin convenience wrapper around Math.random(); the result is computed as min + Math.random() * (max - min), so the distribution is uniform over the interval. The function does not validate or swap its arguments — if min equals max the function returns min, and if min > max the produced values fall between max and min (the interval is effectively reversed). Also, Math.random() is not suitable for cryptographic or security-sensitive use.

## Example
```javascript
// floating-point in [0, 1)
const f = rand(0, 1);

// floating-point in [10, 20)
const r = rand(10, 20);

// integer in [10, 19] (common pattern: floor the float)
const i = Math.floor(rand(10, 20));

// integer in [10, 20] inclusive (adjust upper bound)
const inclusive = Math.floor(rand(10, 21));
```

## Notes
- Upper bound is exclusive because Math.random() returns values in [0, 1).
- If min > max the interval is reversed rather than automatically swapped; validate inputs if that would be surprising.
- Passing non-numeric or undefined arguments results in NaN.
- Not cryptographically secure; use a crypto-safe RNG for security-sensitive needs.

---