namespace Gabriel.Core.Providers;

// Provider-facing view of a tool. ParametersJsonSchema is the raw JSON schema
// object describing the tool's argument shape, passed through to the model.
public record ToolDescriptor(string Name, string Description, string ParametersJsonSchema);
