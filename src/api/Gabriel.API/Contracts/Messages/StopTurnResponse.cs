namespace Gabriel.API.Contracts.Messages;

// Stopped=false means no turn was running for this conversation (it already
// finished, or hadn't started streaming yet) - the client should fall back to
// closing its stream.
public record StopTurnResponse(bool Stopped);
