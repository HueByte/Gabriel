# IResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/IResponsePostProcessor.cs`  
> **Kind:** interface

Cleans a model-generated response before it is persisted by removing residual "AI-ism" openers/closers and enforcing a length cap derived from ConversationState while intentionally preserving Markdown (Discord-style). Reach for this interface when you need a single place to normalize and size-limit the final form of messages that will be saved to the database (as opposed to the raw deltas streamed to clients).

## Remarks
This abstraction centralizes the persistence-only normalization step so streaming and client-visible behavior can remain raw and unmodified. Implementations apply a small set of post-processing rules (strip AI framing, enforce per-conversation length limits) without touching formatting, which helps avoid display regressions in downstream renderers. ConversationState drives the length cap and allows per-conversation policy; the nullable state parameter signals that callers may omit per-conversation limits and implementations should fall back to a sensible default.

## Notes
- ConversationState is nullable; implementers must handle null by applying a default length cap or opting out of truncation.
- Markdown is preserved intentionally — truncation must avoid leaving unterminated Markdown constructs (for example, unclosed code fences) where possible.
- This interface is not a substitute for output sanitization/encoding for untrusted renderers (e.g., HTML); perform appropriate escaping or sanitization at render time or add a dedicated sanitization step if needed.