namespace Gabriel.Core.Entities;

public enum MessageRole
{
    System = 0,
    User = 1,
    Assistant = 2,
    // Observation message carrying the result of a tool invocation. ToolCallId
    // on the Message references the assistant tool_call this is answering.
    Tool = 3,
}
