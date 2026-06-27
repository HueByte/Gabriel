# MemorySaveTool

> **File:** `src/api/Gabriel.Engine/Tools/Memory/MemorySaveTool.cs`  
> **Kind:** class

Saves a named memory entry into the system so future conversations can retrieve it. Use this tool when the agent is told something durable about the user or project (preferences, project-specific facts, or feedback). The tool is idempotent within a scope: saving a memory with the same name updates the existing entry instead of creating duplicates. Choose scope = "user" for data that applies across all projects or scope = "project" for data tied to the current project; pick a type from {user, feedback, project, reference}.

## Remarks
This class wraps an IMemoryService and uses IToolExecutionContext to determine whether a project-scoped memory is allowed (it requires the conversation to be attached to a project). It exposes a JSON schema for arguments with explicit enum values for scope and type to prevent invalid or invented values from callers (especially helpful when the caller is a model). The ExecuteAsync method performs validation (non-empty name/description/body, valid type, project presence for project-scope), calls the memory service to save or update the entry, and returns human-readable status or error messages.

## Example
```csharp
// Example arguments JSON passed to ExecuteAsync
var argsJson = JsonSerializer.Serialize(new
{
    scope = "project",
    type = "feedback",
    name = "dont-log-secrets",
    description = "Avoid logging secrets to stdout",
    body = "Rule: never write API keys to logs.\nWhy: leaking logs can expose credentials.\nHow to apply: filter or redact sensitive fields before writing logs."
});

// Call the tool (ct is a CancellationToken)
var result = await memorySaveTool.ExecuteAsync(argsJson, ct);
// Possible result: "Saved project-scope memory [feedback] 'dont-log-secrets'."
```

## Notes
- If scope is "project" the current conversation must be attached to a project; otherwise the tool returns an error telling you to attach the conversation or use scope="user".
- The fields name, description, and body are required and must be non-empty; invalid or malformed JSON returns an error string rather than throwing.
- Allowed type values are exactly: user, feedback, project, reference. Saving twice with the same (scope, name) updates the existing memory rather than creating a duplicate.