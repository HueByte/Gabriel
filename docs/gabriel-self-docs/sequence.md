# Gabriel Sequence (avatar)

## PURPOSE
The 64-frame, 16×16 RGB avatar engine: how a sequence is generated, animated, and modulated by `ConversationState`.

## USE WHEN
- User asks about the avatar, the pixel-art face, the animated icon.
- User asks what determines the colors / pattern / motion.
- User asks how mood affects the avatar.
- User asks about the `.gemo` file format or "emotional passport".
- User asks about the `/sequence` endpoint.

## QUICK REFERENCE

| Property | Value |
| --- | --- |
| Frames per sequence | 64 (fixed) |
| Frame dimensions | 16 × 16 |
| Palette size | 16 entries × RGB |
| Raw payload | ~16 KB |
| JSON payload | ~50 KB |
| Generation cost | sub-ms server CPU |
| Render | canvas2d `putImageData(16×16)` + CSS upscale |
| Per-frame display | 280 ms (linear interpolation) |
| Full cycle | ~17.92 s |
| Storage | none — regenerated on demand from `(AvatarSeed, ConversationState)` |
| Endpoint | `GET /api/conversations/{convId}/sequence` |

## DETAILS

### Frame layers

| Layer | Frames | Mutability | Modulated by |
| --- | --- | --- | --- |
| DNA Core | 0..15 | Immutable | Seed only |
| Stable Traits | 16..31 | Very slow drift (weeks) | Seed (drift not yet implemented) |
| Context | 32..47 | Medium (hours / days) | Seed (drift not yet implemented) |
| Live State | 48..63 | Per turn | Seed + `ConversationState` |

All four layers share the same pattern in v1; what differs is the time window, palette window, and (for Live State) the mood-driven modulation.

### Data model

```csharp
public sealed record GabrielSequence(
    int Version,
    Palette Palette,
    IReadOnlyList<Frame> Frames,   // exactly 64
    SequenceMetadata Metadata);

public sealed record Palette(IReadOnlyList<RgbColor> Colors);
public readonly record struct RgbColor(byte R, byte G, byte B);

public sealed record Frame(byte[] Pixels);      // 256 palette indices
public sealed record SequenceMetadata(long Seed, DateTimeOffset GeneratedAt, string? StateSummary);
```

### Seed → visual decisions (three independent mixes)

| Decision | Source | Output |
| --- | --- | --- |
| Palette family | `seed ^ (seed >> 32) ^ 0x9E3779B1` mod 16 | One of: heat, ice, plasma, matrix, sunset, ocean, aurora, rose, cyber, amber, lime, sakura, mono, void, forge, alive |
| Pattern kind | `|seed ^ (seed >> 32)|` mod 5 | One of: Plasma, Waves, Spiral, Pulse, Shimmer |
| Pattern params | `seed ^ (seed >> 32) ^ 0xC2B2AE35` as `Random` seed | Pattern-specific |

The xor salts are distinct so a 1-bit seed change affects all three axes (not just one).

### Pattern primitives (all `(x,y,t) → [0,1]`)

1. **Plasma** — superposed sines; most parameter-sensitive; "alive without obvious motion".
2. **Waves** — directional wave along angle θ; reads as "flowing".
3. **Spiral** — rotating arms; instantly recognizable as a pattern, not noise.
4. **Pulse** — expanding ripples from center with comet-tail trail; clean rhythm.
5. **Shimmer** — per-pixel independent phase + speed → starfield twinkle. The only primitive without correlated motion.

All primitives loop cleanly: sampling at `t = 0` and `t = 1` produce the same image.

### Layer rendering

```
Layer            Frames   Time window           Palette window    Intensity
DNA Core         0..15    t = i/16              [0..15]           1.0
Stable Traits    16..31   t = i/16 + 0.25       [2..15]           0.95
Context          32..47   t = i/16 + 0.5        [1..14]           1.0
Live State       48..63   t = i/16 + 0.75 + ν   [pMin..pMax]      I
```

Per pixel:
```
v = SamplePattern(bundle, x, y, t)
v' = clamp(v * intensity, 0, 1)
idx = paletteMin + round(v' * (paletteMax - paletteMin))
frame.pixels[y*16 + x] = idx
```

### Live State modulation

`LiveStateProfile(PaletteMin, PaletteMax, Intensity, PhaseNudge, Summary)` precomputed from `ConversationState`.

| Mood | (pMin, pMax) | Intensity | Feel |
| --- | --- | --- | --- |
| Playful | (8, 15) | 1.10 | Hot, energetic |
| Venting | (0, 8) | 0.80 | Dim, sunk |
| Serious | (5, 10) | 0.90 | Compressed, focused |
| Curious | (1, 15) | 1.05 | Wide, alive |
| LowEnergy | (1, 7) | 0.75 | Sleepy |
| Neutral | (1, 15) | 1.0 | Full range, default |

**Consecutive-shorts pinch** (when `ConsecutiveShortMessages ≥ 2`):
```
mid = (pMin + pMax) / 2
δ = max(1, (pMax - pMin) / 3)
pMin' = max(0, mid - δ); pMax' = min(15, mid + δ)
```
Reads as the avatar getting slightly tense / restricted.

**Per-turn phase nudge**:
```
ν = (TurnCount · 0.073 + LastUserTokenCount · 0.0013) mod 1
```
Prevents two same-mood turns from producing identical Live State frames.

### API surface

```
GET /api/conversations/{convId}/sequence
```

Returns:
```jsonc
{
  "version": 1,
  "palette": [[r,g,b], ...],            // 16 entries
  "frames": [[idx, ...], ...],          // 64 arrays of 256 indices
  "metadata": {
    "seed": 3421997825,
    "generatedAt": "ISO-8601",
    "stateSummary": "pattern=plasma, mood=curious, turn=12, lastTok=47"
  }
}
```

User-scoped load (cross-tenant fetches 404). Pure read, no mutation.

**Client refresh cadence**: after `send` (Live State changed) and after `reroll-avatar` (seed changed). NOT on rename / delete / new-chat.

### Client renderer

`GabrielSequenceView.tsx`: `<canvas width=16 height=16>` with CSS scaling and `imageRendering: pixelated`. Per-frame cost is one `putImageData(16×16)`. Frame-to-frame interpolation is linear RGB lerp between adjacent frames at `t ∈ [0,1)`.

## INVARIANTS

- Always exactly 64 frames, 16×16.
- Palette always 16 entries.
- Sequence is regenerated on demand; nothing persists.
- Seed lives on `Conversation.AvatarSeed`; state on `Conversation.StateJson`.
- The same `ConversationState` that drives the system prompt drives the Live State layer — both react to the same `Mood`.

## PITFALLS

- "The avatar froze on the same frame" — without the per-turn phase nudge it would; verify the nudge formula didn't get zeroed.
- `Stable Traits` and `Context` drift is **designed but not implemented in v1** — both layers use only the seed today.
- The `.gemo` binary file format is specified (in `.dev/notes/emotion-engine.md`) but **not yet implemented**. The wire format is JSON.
- Don't tell the user "Gabriel persists your avatar" — it doesn't; it regenerates each fetch.

## SEE ALSO

- `personality.md` — `ConversationState` and `Mood` (the Live State inputs).
- Human-prose companion: `Gabriel.Engine/gabriel-sequence.md` (includes pattern math).
- Spec: `.dev/notes/emotion-engine.md` (gitignored; planning doc).
