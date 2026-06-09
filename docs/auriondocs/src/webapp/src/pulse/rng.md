# rng.ts

> **Source:** `src/webapp/src/pulse/rng.ts`

## Contents

- [Rand](#rand)
- [mulberry32](#mulberry32)
- [randomSeed](#randomseed)

---

## Rand

> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** type

A type alias for a zero-argument function that returns a number. Use Rand when a function needs an injectable random-number source (for example to swap Math.random for a seeded or mock generator during testing) rather than depending directly on a particular RNG implementation.

## Remarks
This abstraction decouples consumers from any particular random number implementation and improves testability and determinism. Callers accept a Rand to allow the caller or test harness to provide a specific generator (e.g., Math.random, a seeded pseudorandom generator, or a mock that returns controlled values).

## Example
```typescript
// Use the global Math.random
const rand: Rand = Math.random;
console.log(rand()); // e.g. 0.123456

// Provide a simple seeded LCG as a Rand for deterministic sequences
function makeLCG(seed: number): Rand {
  let state = seed >>> 0;
  return () => {
    state = (1664525 * state + 1013904223) >>> 0;
    return state / 0x100000000; // normalize to [0,1)
  };
}

const seeded = makeLCG(42);
console.log(seeded());

// Injecting into a function that needs randomness
function rollDie(rand: Rand) {
  return Math.floor(rand() * 6) + 1;
}
``` 

## Notes
- The type does not specify range, distribution, or determinism; do not assume values are in [0,1) or follow any particular distribution unless the specific Rand implementation documents that contract.
- Implementations may be stateful or have side effects; treat Rand as an abstract dependency rather than a guaranteed pure function.
- Prefer passing a seeded or mock Rand for unit tests to make outcomes deterministic and reproducible.

---

## mulberry32

> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** function

Returns a small, fast, seeded pseudo-random number generator implementing the Mulberry32 algorithm. Call this when you need a deterministic, reproducible sequence of non-cryptographic random numbers (for example: procedural generation, unit tests, or anywhere Math.random()'s non-determinism is undesirable).

## Remarks
The function produces and closes over a 32-bit state derived from the provided seed (the seed is coerced to an unsigned 32-bit integer). Each call to the returned function advances the internal state and yields a uniformly distributed floating-point value in [0, 1). This implementation is designed for speed and simplicity rather than cryptographic strength — it provides reproducible sequences across runs given the same seed and works well for lightweight simulations and deterministic behaviours.

## Example
```typescript
// Create a seeded RNG
const rng = mulberry32(12345);

// Get a uniform float in [0, 1)
const f = rng();

// Get an integer in [0, 99]
const n = Math.floor(rng() * 100);

// Generate an array of 10 reproducible random numbers
const arr = Array.from({ length: 10 }, () => rng());
```

## Notes
- Not cryptographically secure — do not use for keys, tokens, or security-sensitive randomness.
- The seed is converted via `>>> 0` to a 32-bit unsigned integer; fractional parts are discarded and negative numbers are mapped into the 0..2^32-1 range.
- Output resolution is 32-bit (values are integers in 0..2^32-1 divided by 2^32), so expect at most 2^32 distinct values.
- The implementation uses `Math.imul` and bitwise operations; these are available in modern JavaScript engines. If targeting very old environments, verify `Math.imul` support or provide a polyfill.


---

## randomSeed

> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** function

Return a uniformly distributed 32-bit unsigned integer produced from Math.random(). Use this when you need a quick, non-cryptographic 32-bit seed or identifier (for example, seeding a PRNG or generating a compact random token) instead of producing raw floating-point values or manually converting ranges.

## Remarks
This small helper converts Math.random() (which yields a floating-point value in [0, 1)) into a 32-bit unsigned integer by multiplying by 2^32 and using the >>> 0 coercion. It exists as a convenience for code paths that expect a single 32-bit seed value rather than a floating-point random number.

## Example
```typescript
// Get a 32-bit unsigned seed
const seed = randomSeed();
console.log(seed); // e.g. 3498573495

// Use as hex if you need a compact string form
const hex = seed.toString(16).padStart(8, '0');
console.log(hex); // e.g. "d0a3f4b7"

// Example: pass to a PRNG constructor that accepts a 32-bit seed
// const rng = new MyPrng(seed);
```

## Notes
- This is not cryptographically secure. For cryptographic randomness use crypto.getRandomValues or a secure RNG API instead.
- The return value is a JavaScript number guaranteed to be an integer in the range 0..4294967295 (inclusive); the >>> 0 ensures an unsigned 32-bit result.
- Math.random implementations may provide fewer than 32 bits of entropy depending on the JS engine, so entropy may be limited compared to platform CSPRNGs.

---