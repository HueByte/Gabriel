# LocalDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/LocalDocsOptions.cs`  
> **Kind:** class

Options that control the local, on-disk documentation source used by Gabriel's documentation system. Use this when you want the runtime to read Markdown files from a repository or local folder (the LLM-native self-docs) instead of—or before—falling back to the GitHub-backed source.

## Remarks
This options class configures a pluggable local docs source that participates in a composite lookup with a GitHub fallback. The resolver treats an absolute Path that exists as authoritative; otherwise it probes Environment.CurrentDirectory and AppContext.BaseDirectory (walking up parent directories) to find a matching folder. If no match is found the local source behaves as empty and emits a one-time warning so the composite lookup can transparently fall back to the GitHub source. The SectionName constant is provided for configuration binding.

## Example
```csharp
// appsettings.json
{
  "Tools": {
    "Docs": {
      "Local": {
        "Enabled": true,
        "Path": "docs/gabriel-self-docs"
      }
    }
  }
}

// Bind in C# (Microsoft.Extensions.Configuration)
var localDocs = configuration.GetSection(LocalDocsOptions.SectionName).Get<LocalDocsOptions>();
```

## Notes
- If Path is a relative path that cannot be resolved by the probing logic, the local source will appear empty and a single warning will be logged; the system then relies on the GitHub fallback.
- Both forward and back slashes are accepted for Path; the resolver normalizes separators to the platform convention.
- Setting Enabled to false disables the local source entirely without changing any other docs configuration or wiring.
