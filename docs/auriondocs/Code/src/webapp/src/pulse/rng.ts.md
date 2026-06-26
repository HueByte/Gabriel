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

A function type that produces a numeric random value when invoked. Use this alias to accept, provide, or swap random-number generators (Math.random, a seeded PRNG, or a mock) without coupling callers to a specific implementation.

## Remarks
Rand exists to decouple code that needs randomness from the global Math.random (or any concrete PRNG). That makes it easy to inject deterministic or test-friendly implementations, replace the distribution or range, and keep callers agnostic to generator state or seeding strategy.

## Example
```typescript
// Wrap the built-in Math.random
const mathRand: Rand = () => Math.random();

// A simple seeded linear congruential generator (LCG) — deterministic for tests
function createSeededRand(seed: number): Rand {
  let state = seed >>> 0;
  return () => {
    // Constants from Numerical Recipes
    state = (1664525 * state + 1013904223) >>> 0;
    return (state & 0x7fffffff) / 0x80000000; // returns value in [0, 1)
  };
}

// Consumer accepts Rand to avoid direct dependency on Math.random
function pickIndex(rand: Rand, length: number): number {
  return Math.floor(rand() * length);
}

const seeded = createSeededRand(42);
console.log(pickIndex(seeded, 10));
```

## Notes
- The type does not enforce range, distribution, or bounds; do not assume values are in [0, 1) unless the implementation documents that guarantee.
- Implementations may be stateful (e.g., seeded PRNGs) and produce different values on each call; treat them as potentially non-pure.
- For reproducible tests, inject a deterministic Rand rather than relying on Math.random.

---

## mulberry32

> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** function

Returns a small, fast, deterministic pseudo-random number generator initialized from the given integer seed. Call this to obtain a Rand (a zero-argument function) that produces uniformly distributed floating-point numbers in [0, 1) — useful for reproducible simulations, procedural generation, or tests where cryptographic strength is not required.

## Remarks
This implements the public-domain "mulberry32" algorithm: a compact 32-bit state PRNG that advances state via integer arithmetic and emits a 32-bit-derived fractional value. The function captures the internal state in a closure and returns a callable generator that mutates that state on each invocation, producing a consistent sequence for a given seed.

## Example
```typescript
// Create two independent generators with the same seed
const rng1 = mulberry32(12345);
const rng2 = mulberry32(12345);

console.log(rng1()); // 0..1 (deterministic)
console.log(rng1()); // next value in sequence

// rng2 will produce the same first value as rng1 did
console.log(rng2()); // equals first rng1() call above

// To get an integer in [0, n):
const n = 10;
const value = Math.floor(rng1() * n);
```

## Notes
- The seed is coerced to a 32-bit unsigned integer (seed >>> 0). Passing non-integer or negative numbers will be converted accordingly.
- Not suitable for cryptographic use; prefer Web Crypto APIs for security-sensitive randomness.
- The generator is stateful (closure over `state`); create separate instances if you need independent streams or reproducibility across concurrent contexts.
- Uses Math.imul and bitwise operations to ensure 32-bit wrap-around behaviour; output granularity is effectively 1/2^32.


---

## randomSeed

> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** function

Returns a 32-bit unsigned integer derived from Math.random(), suitable for use as a non-cryptographic numeric seed (for example, when initializing a seeded PRNG or stamping random identifiers). Use this helper when you need a quick uint32 seed value rather than a floating-point [0,1) value.

## Remarks
This small utility exists to convert the floating-point result of Math.random() into a JavaScript unsigned 32-bit integer (0 .. 2^32-1). It uses a multiply by 0x100000000 and the >>> 0 bitwise operation to coerce the result into the expected uint32 range.

## Example
```typescript
// Obtain a uint32 seed and pass it to a seeded RNG constructor
const seed = randomSeed();
console.log(`Using seed: ${seed}`);
const rng = new MySeededRng(seed); // hypothetical seeded RNG
```

## Notes
- The value is produced by Math.random(): it is not cryptographically secure and should not be used for security-sensitive purposes.
- The statistical quality and exact sequence depend on the JavaScript runtime's Math.random implementation.
- Returned range is an integer between 0 and 4294967295 inclusive (0 .. 2^32 - 1).
- This function is not deterministic across runs; to reproduce results you must supply a known seed explicitly rather than calling this helper.

---