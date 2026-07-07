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


Rand is a type alias for a no-argument function returning a number. It represents a pluggable source of numeric values, commonly used as a random-number generator. By depending on Rand instead of directly calling a specific RNG, APIs can swap in different number sources—such as Math.random, a seeded generator, or a deterministic stub—without changing their call sites.

## Remarks

Abstracting the number source behind Rand decouples generation from consumption, enabling dependency injection and easier testing. It signals the intent that randomness (or numeric output) is provided by a supplied function rather than baked into the caller. This makes it straightforward to swap in deterministic sequences for tests or to tailor the distribution without modifying surrounding logic.

## Example

```typescript
// Example usage
const randomSource: Rand = Math.random;
const samples = Array.from({ length: 5 }, () => randomSource());
```

## Notes

- Rand is a no-arg function that yields a number; the exact distribution is implementation-defined.
- For deterministic tests, provide a Rand that returns fixed values or follows a seeded sequence.
- If you pass a shared Rand across modules, ensure its stateful behavior remains predictable and documented.

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


Mulberry32(seed) returns a tiny, deterministic pseudo-random number generator (PRNG) seeded with the provided 32-bit unsigned seed. The returned function advances an internal state and yields numbers in [0, 1) with uniform distribution, making it ideal for lightweight randomness in tests, procedural generation, or simulations where cryptographic security is not required.

## Remarks
Because the returned value is a closure over its internal 32-bit state, the sequence is fully determined by the seed and the number of times you call it. Use this when you need repeatable randomness across runs or environments without pulling in a heavier RNG. It is not cryptographically secure and should not be used for security-critical purposes.

## Example
```typescript
const rnd = mulberry32(42);
console.log(rnd()); // deterministic value in [0, 1)
```

## Notes
- Not cryptographically secure; use for tests, demos, or non-secure simulations.
- The generator's internal state is captured in a closure; sharing a single instance across concurrent tasks may require external coordination.

---

## randomSeed
> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** function

```typescript
export function randomSeed(): number
```

**Returns:** `number`


Generates a 32-bit unsigned random seed by sampling Math.random and coercing the result into a non-negative integer. It multiplies the [0,1) value from Math.random by 0x100000000 (2^32) and then applies an unsigned right shift (>>> 0) to convert it to a 32-bit unsigned integer, yielding a value in the range 0 through 4294967295. This function is useful when you need a numeric seed to initialize a seeded RNG, ensuring you work with a fixed-width integer rather than a floating-point sample.

## Remarks
Encapsulating seed generation in randomSeed provides a single, discoverable source for 32-bit seeds. It decouples the concerns of randomness acquisition from RNG initialization, so components can swap RNG implementations without changing their seed-creation logic. The left-hand operation (Math.random) is a standard source of pseudo-random values in environments where cryptographic security is not required.

## Notes
- Not cryptographically secure; Math.random is not suitable for cryptographic seeds. For secure seeds, consider using crypto.getRandomValues (or Node's crypto module) to produce 32-bit seeds.

---