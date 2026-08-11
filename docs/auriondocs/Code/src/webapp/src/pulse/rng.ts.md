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


Rand is a type alias for a no-argument function that returns a number. It represents a randomness source that can be passed around to code that needs random values without calling Math.random directly, enabling easy substitution of RNG implementations for testing or different environments.

## Remarks
Rand provides a contract for RNG providers used by the surrounding code. By abstracting randomness behind Rand, consumers stay decoupled from a concrete global RNG, enabling deterministic tests and flexible RNG strategies.

## Example
```typescript
const exampleRand: Rand = () => Math.random();
```

## Notes
- Rand is a type alias, not a value. A value of type Rand must be a function with signature () => number.
- Functions assigned to Rand must not require parameters; extra parameters will fail type-checking.
- Using a deterministic Rand in tests (e.g., returning fixed values or a preset sequence) helps verify behavior independent of true randomness.

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


mulberry32(seed) returns a small, fast, deterministic pseudo-random number generator (Rand) seeded by a 32-bit unsigned integer. The returned function, when called, advances its internal state and produces a uniform value in [0, 1).

## Remarks
This lightweight abstraction provides a reproducible source of randomness without a full RNG library. It uses a closure to keep internal state private and updates it with each invocation. It is not suitable for cryptographic needs; for security-sensitive randomness, use a cryptographically secure API (e.g., Web Crypto). If you need multiple streams, create separate generators by supplying different seeds.

## Example
```typescript
// Create a deterministic RNG from a numeric seed
const rng = mulberry32(42);
console.log(rng()); // 0 <= result < 1
console.log(rng()); // subsequent samples
```

## Notes
- Seed normalization: The seed is coerced to an unsigned 32-bit integer via seed >>> 0; non-integer or negative seeds wrap accordingly.
- The PRNG is implemented as a closure over a mutable 32-bit state; subsequent calls advance the state in a deterministic way.
- Not cryptographically secure; use for non-cryptographic tasks only.

---

## randomSeed
> **File:** `src/webapp/src/pulse/rng.ts`  
> **Kind:** function

```typescript
export function randomSeed(): number
```

**Returns:** `number`


Generates a fresh 32-bit unsigned seed by sampling Math.random. It returns a non-negative integer in the range 0 through 4294967295, intended for initializing a non-cryptographic RNG.

## Remarks

Encapsulating the seed generation provides a single, consistent API surface for obtaining seeds without exposing bitwise manipulation to call sites. It relies on non-cryptographic randomness (Math.random); for security-sensitive randomness, use a cryptographic source such as the Web Crypto API.

## Example
```typescript
// Example usage
const seed = randomSeed();
console.log('seed:', seed);
```

## Notes
- Not suitable for cryptographic purposes: Math.random() is not a cryptographically secure source.
- The produced value is a number in the inclusive range [0, 4294967295], representing a 32-bit unsigned seed.
- Sequences produced by repeated calls are not guaranteed to be unique; seeds may repeat over time depending on the underlying PRNG.


---