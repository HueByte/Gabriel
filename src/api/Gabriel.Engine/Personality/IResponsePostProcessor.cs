using Gabriel.Core.Personality;

namespace Gabriel.Engine.Personality;

// Cleans the model's raw response before it's persisted to the DB. Currently
// only strips residual AI-ism openers/closers and applies a length cap derived
// from ConversationState. Markdown is intentionally preserved (Discord-style).
//
// Per user choice ("stream raw, clean on save"), the controller forwards raw
// deltas to the client; only the saved-and-final form of the message gets cleaned.
public interface IResponsePostProcessor
{
    string Clean(string raw, ConversationState? state);
}
