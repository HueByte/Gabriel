# LocalDocsOptions

> **File:** `src/api/Gabriel.Core/Configuration/LocalDocsOptions.cs`  
> **Kind:** class

Configuration for the local on-disk documentation source used by Gabriel's LLM-native self-docs. Use this to enable/disable the local docs source and to point the resolver at a directory containing .md files (commonly the repo folder "docs/gabriel-self-docs").

## Remarks
This options object controls the primary on-disk docs source in the composite docs lookup (the GitHub-backed source is used as a fallback). Toggling Enabled off skips the local source entirely; when enabled the resolver will treat Path as either an absolute path (if it exists) or a relative path to probe from the application's current directory and base directory while walking up parent folders. If no match is found the source behaves as empty and emits a one-time warning so the composite lookup can transparently fall back to the remote (GitHub) source.

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

// Bind in startup
var localSection = configuration.GetSection(LocalDocsOptions.SectionName);
var localOptions = localSection.Get<LocalDocsOptions>();
```

## Notes
- Path may be absolute or relative; when relative the resolver probes Environment.CurrentDirectory and AppContext.BaseDirectory and walks parent directories — results can differ between dev and deployed environments.
- The resolver normalizes path separators, so forward or back slashes are accepted on Windows.
- If the configured path cannot be found the local source will act as empty and log a single warning; this is intentional to allow automatic fallback to the GitHub source.