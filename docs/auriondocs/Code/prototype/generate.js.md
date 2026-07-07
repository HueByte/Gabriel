# generate.js

> **Source:** `prototype/generate.js`

## Contents

- [generate](#generate)
- [rand](#rand)

---

## generate
> **File:** `prototype/generate.js`  
> **Kind:** function

```javascript
function generate()
```


Generates a complete animation sequence by selecting a procedural pattern based on the command-line argument, initializes it for a SIZE-by-SIZE grid, and renders a series of frames. Each frame samples the pattern across the grid over time, adds subtle random noise, and blends in an ambient floor so the quiescent color sits on the gradient rather than at zero, finally clamping values to the [0, 1] range. The function returns both the frames and a meta descriptor (size, frame count, pattern id, params, noise amplitude, ambient, and palette) suitable for a playback pipeline that renders at 20 frames per second.

## Remarks
This function centralizes synthetic animation generation by tying pattern selection, per-pixel sampling, and post-processing (noise and ambient shading) into a cohesive data package. It delegates pattern-specific behavior to the pattern object via init and sample while handling frame timing and output structure, enabling downstream renderers to reproduce visuals without re-running the generation logic. Because the CLI argument selects the pattern, it is most commonly used in CLI-driven workflows where patterns are interchangable and metadata must accompany the frames for playback.

## Example
```javascript
// Example: generate a dataset and inspect its metadata and frame count
const data = generate();
console.log(`Pattern: ${data.meta.pattern}, frames: ${data.frames.length}`);
```

## Notes
- Outputs are non-deterministic across runs because Math.random() is used for noise; for reproducible results, replace Math.random with a seeded RNG.
- The function relies on global constants SIZE and FRAMES and on CLI-driven pattern helpers (pickPattern, pickPalette). Ensure these exist in the runtime environment or the module's scope.
- Ambient and noise ranges are hard-coded; tweaking them will directly affect the visual texture and baseline shading of all frames across the generated sequence.

---

## rand
> **File:** `prototype/generate.js`  
> **Kind:** function

```javascript
function rand(min, max)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `min` | — | — |
| `max` | — | — |


Generates a uniformly distributed floating-point number in the range [min, max) (or (max, min] if min > max) by scaling Math.random() to the provided interval. This tiny utility is handy whenever you need a quick, dependency-free random value within a numeric range, such as simulations, UI randomness, or sampling tasks.

## Remarks
This function encapsulates the common formula for mapping a [0,1) random value to an arbitrary interval, making boundary behavior explicit at the call site and reducing duplicated math across the codebase. It also improves readability and reduces the risk of off-by-one mistakes by centralizing the range logic.

## Example
```javascript
// Common usage: random float in [min, max)
const r = rand(2, 5);

// Use for integers if needed:
const intR = Math.floor(rand(2, 5)); // yields 2, 3, or 4
```

## Notes
- Not suitable for cryptographic purposes; Math.random is not a cryptographically secure RNG.
- If min > max, the interval is reversed (the result will lie in (max, min]). Consider normalizing inputs with a swap if you require min <= max.

---