# MemorySaveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs`  
> **Kind:** class

Save a named memory entry into the persistent memory store so future conversations (or the current project) can retrieve it. Use this tool when the user provides durable information — personal preferences, feedback, project context, or external references — that should be remembered across turns. Choose scope 'user' to make the memory visible across all projects, or 'project' to restrict it to the current project; the tool is idempotent for a given (scope, name) pair and will update an existing entry rather than creating duplicates.

## Remarks
This tool is a thin adapter that validates the agent-provided JSON, enforces a small, explicit schema (scope and type enums plus required fields), and then delegates persistence to an IMemoryService. Constraining the allowed values for scope and type prevents the model from inventing arbitrary categories. The tool returns human-readable success or error messages (suitable for the agent) rather than throwing for common validation mistakes.

## Example
```csharp
// Example arguments JSON the agent should pass to ExecuteAsync
var argsJson = """
{
  "scope": "project",
  "type": "feedback",
  "name": "dont-email-weekends",
  "description": "User prefers no email on weekends",
  "body": "Do not send marketing emails on weekends.\nWhy: User reacts negatively to weekend contact.\nHow to apply: Schedule campaign sends only on weekdays."
}
""";

// The agent calls ExecuteAsync(argsJson, cancellationToken) and will get
// a string like: "Saved project-scope memory [feedback] 'dont-email-weekends'."
```

## Notes
- If scope='project' but the conversation isn't attached to a project, the tool returns an error instructing the agent to attach the conversation or use scope='user'.
- Saving with the same name in the same scope updates the existing entry (idempotent behavior); names should be short kebab-case slugs and unique within the chosen scope.
- The tool validates JSON deserialization and required fields (name, description, body); invalid JSON or missing/empty required fields produce readable error messages instead of exceptions.
