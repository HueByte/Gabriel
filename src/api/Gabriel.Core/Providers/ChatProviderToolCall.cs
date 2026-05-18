namespace Gabriel.Core.Providers;

// A single tool invocation requested by the assistant. ArgumentsJson is the
// raw JSON string the model emitted — validation against the tool's parameter
// schema happens at execution time.
public record ChatProviderToolCall(string Id, string Name, string ArgumentsJson);
