# rng.ts

> **Source:** `src/webapp/src/pulse/rng.ts`

## Contents

- [Rand](#rand)
- [mulberry32](#mulberry32)
- [randomSeed](#randomseed)

---

## Rand
> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** type alias

```typescript
export type Rand = () => number
```


Rand is a type alias for a function that takes no arguments and returns a number. It represents a numeric supplier—something that yields a value when invoked—without requiring a concrete numeric value up front. Use Rand when a consumer should remain agnostic about where the number comes from, allowing you to swap in an actual RNG, a seeded generator, or a test double without changing call sites.

## Remarks

By abstracting the source of numbers behind Rand, components can inject different RNG strategies, improving testability and configurability. It sits at the boundary between pure data types and behavior, enabling deterministic behavior in tests and pluggable randomness in production.

## Example

```typescript
// Common usage: provide a Rand implementation
const myRand: Rand = () => Math.random();

// Consumer that uses Rand
function nextValue(rng: Rand): number {
  return rng();
}

console.log(nextValue(myRand));
```

## Notes

- Rand is a type alias; it does not create a runtime function. You must supply a function value whose type matches Rand.
- If you need deterministic tests, inject a Rand that returns a fixed sequence or a seeded generator to reproduce results.

---

## mulberry32
> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** function

```typescript
export function mulberry32(seed: number): Rand
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `seed` | `number` | — |

**Returns:** `Rand`


mulberry32(seed) creates a deterministic pseudorandom number generator and returns a function that, on each call, produces a uniform value in [0, 1). Use it when you need reproducible randomness keyed by a seed (for tests, procedural content, or simulations) instead of Math.random.

## Remarks
Because mulberry32 uses a closure over a 32-bit state, it provides a lightweight, self-contained RNG without shared global state. The function relies on a small mix of arithmetic and bitwise operations to transform the internal state into a new pseudorandom value each call. This makes it fast and deterministic, but not suitable for cryptographic purposes. It’s ideal for reproducible experiments where you want to replay the same sequence by using the same seed.

## Notes
- Not cryptographically secure; use for non-secure randomness only.
- The seed is masked to a 32-bit unsigned integer (seed >>> 0); NaN or non-numeric seeds map to 0.
- Each call advances the internal state and returns a value in [0, 1).

---

## randomSeed
> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** function

```typescript
export function randomSeed(): number
```

**Returns:** `number`


Generates a non-sensitive 32-bit seed from the built-in Math.random source. It scales Math.random() by 2^32 and converts the result to an unsigned 32-bit integer, yielding a value in the range 0 to 4294967295. Call randomSeed() when you need a numeric seed to initialize a non-cryptographic PRNG or to parameterize tests; it is not suitable for cryptographic purposes because Math.random is not cryptographically secure.

## Remarks
This utility provides a simple, consistent way to obtain a 32-bit seed from the standard randomness source, decoupling seed creation from a specific RNG implementation. It guarantees the returned value fits a 32-bit unsigned range, which aligns with common PRNG interfaces. If cryptographic randomness is required, use a crypto API (for example, crypto.getRandomValues) instead.

## Notes
- Not cryptographically secure; do not use for security tokens or password generation.
- Math.random quality can vary by environment; for reproducible seeds, consider using a dedicated PRNG or a fixed seed.
- The return type is a JavaScript number; all values up to 2^32 - 1 are exactly representable in IEEE-754 double precision.


---