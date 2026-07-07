# IResponsePostProcessor

> **File:** `src/api/Gabriel.Engine/Personality/IResponsePostProcessor.cs`  
> **Kind:** interface

```csharp
public interface IResponsePostProcessor
```


The IResponsePostProcessor interface defines a hook for cleansing raw model responses before they are persisted to the database. Implementations perform lightweight normalization—stripping residual AI-isms and applying a length cap derived from the current ConversationState—while intentionally preserving Markdown formatting. This separation ensures that streaming raw deltas can be emitted to clients while only the finalized, sanitized form is stored.

## Remarks
This abstraction centralizes response hygiene in the save pathway, enabling a consistent policy across all producers without forcing cleanup at multiple call sites. It relies on ConversationState to tailor the final length or content constraints, tying the persistence-layer cleanliness to per-conversation context. The design supports swapping post-processing strategies behind the interface without altering producers, which simplifies testing and future policy changes.

## Example
```csharp
// Example usage: clean a raw model response before saving
string raw = "AI: This is an example response...";
ConversationState state = /* obtain current state from your application */;
IResponsePostProcessor postProcessor = /* obtain via DI or factory */;
string cleaned = postProcessor.Clean(raw, state);
```