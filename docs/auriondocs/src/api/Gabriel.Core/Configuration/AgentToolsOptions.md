# AgentToolsOptions

> **File:** `src/api/Gabriel.Core/Configuration/AgentToolsOptions.cs`  
> **Kind:** class

Configuration for the filesystem and shell agent tools. This options object is bound from the "AgentTools" configuration section and is used to enable/limit host-mode operations (via HostRoot) and to set safety/default limits for file previews and directory listings.

## Remarks
AgentToolsOptions centralizes safety knobs that the agent tooling consults before performing filesystem or shell operations. HostRoot is the primary security boundary: when set, host-mode operations are canonicalized under that directory and any path resolving outside it is rejected. Other properties provide conservative defaults and hard caps to avoid expensive or unsafe operations (large previews or runaway directory listings).

## Example
```csharp
// appsettings.json
{
  "AgentTools": {
    "HostRoot": "C:\\workspaces\\project-123",
    "MaxPreviewBytes": 10485760,
    "MaxListEntries": 1000,
    "DefaultListEntries": 200,
    "DefaultPreviewLines": 6
  }
}

// Reading the options from IConfiguration
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var toolsOptions = config
    .GetSection(AgentToolsOptions.SectionName)
    .Get<AgentToolsOptions>();
```

## Notes
- HostRoot is intentionally opt-in: leaving it null or empty disables host-mode access (project-sandbox mode remains available). Set it deliberately; there is no fallback to the process cwd.
- MaxPreviewBytes is a hard byte limit for file previews (head/tail); files larger than this will have size reported but their preview skipped.
- MaxListEntries is an absolute safety cap on any single listing operation; DefaultListEntries only controls the default page size and does not override the hard cap.
