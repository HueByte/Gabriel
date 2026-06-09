# ListDirTool

> **File:** `src/api/Gabriel.Engine/Tools/Files/ListDirTool.cs`  
> **Kind:** class

Lists the contents of a directory within the agent's configured root (or the active project sandbox). Returns a human-readable, table-style listing (type, size, modified, name) sorted with directories first then alphabetically; supports an optional shallow recursive walk with entries indented by depth and enforces a maximum-entry cap configured via AgentToolsOptions.

## Remarks
ListDirTool is the agent-safe equivalent of a shell "ls -la": it resolves the requested path through IAgentPathResolver (so absolute or relative paths are validated and constrained to the configured host root or project sandbox), applies the configured default and hard cap for listing size, and produces a readable, truncated listing instead of exposing raw filesystem data. It returns error messages as strings for resolution or path errors rather than throwing, making it suitable for use directly by higher-level agent components that display results to users.

## Example
```csharp
// Constructing the tool (typical DI in the agent)
var tool = new ListDirTool(agentPathResolver, options);

// Call with JSON arguments to list the current project sandbox recursively,
// including hidden entries, and limiting to 100 entries.
string argsJson = JsonSerializer.Serialize(new
{
    path = ".",
    mode = "project",
    recursive = true,
    max_entries = 100,
    include_hidden = true
});

string result = await tool.ExecuteAsync(argsJson, CancellationToken.None);
Console.WriteLine(result);
```

## Notes
- The tool returns human-readable strings (listings or error messages); it does not return structured objects — callers that need machine-parsable output must parse the text or modify the tool.
- Listings are truncated when the max_entries limit is reached; AgentToolsOptions.MaxListEntries enforces a hard cap that cannot be exceeded by input.
- include_hidden toggles inclusion of Unix-style dotfiles and entries with the Hidden attribute; by default hidden entries are excluded.
- If an absolute path is supplied, it must canonicalize under the resolved root; otherwise path resolution fails and an error string is returned.