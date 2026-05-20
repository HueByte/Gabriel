namespace Gabriel.Core.Entities;

// Behavioural bias attached to a single Conversation. Drives which
// "Fragments.Mode*" snippet gets spliced into the per-turn system prompt,
// re-weighting the persona without rewriting it.
//
// Stored as an int on Conversation.Mode (nullable; null = Chatty default).
// Adding a new mode is three coordinated edits:
//   1. New value here.
//   2. New `Fragments.Mode*` const + `PromptKey.Mode*` constant +
//      `PromptRegistry` mapping.
//   3. New case in the mode→PromptKey switch in GabrielSystemPromptBuilder.
public enum GabrielMode
{
    Chatty      = 0,  // default — current persona, unchanged
    Elaborative = 1,  // longer artifacts, more comments, named trade-offs
    Concise     = 2,  // shortest correct answer, no preamble / closer
    Tutor       = 3,  // step-by-step, examples-first, explain the *why*
    Critic      = 4,  // skeptical stance, finds flaws before validating
}
