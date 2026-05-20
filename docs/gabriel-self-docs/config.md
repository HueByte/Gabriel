# Configuration reference

## PURPOSE
Every options POCO bound from `appsettings*.json`, its section name, defaults, and env-var binding.

## USE WHEN
- User asks where to set X / how to change Y.
- User asks about env var names or secret loading.
- User asks about a default value.
- User asks about Infisical / secret-management wiring.

## QUICK REFERENCE — sections

| Section | Type | Project | Purpose |
| --- | --- | --- | --- |
| `Agent` | `AgentOptions` | Engine | Loop iteration caps, compact knobs. |
| `Personality` | `PersonalityOptions` | Engine | Persona name, typing-tempo (SSE pacing). |
| `AgentTools` | `AgentToolsOptions` | Engine | Tool-level toggles (per-tool enable/disable, future). |
| `Providers:Grok` | `GrokOptions` | Engine | xAI Grok provider + per-model catalog. |
| `Tools:Web:Active` | string | n/a | `"ddg"` (default) or `"brave"`. |
| `Tools:Web:Brave` | `BraveSearchOptions` | Engine | Brave Search API key + endpoint. |
| `Tools:Docs:GitHub` | `GitHubDocsOptions` | Engine | GitHub-backed self-docs fallback. |
| `Tools:Docs:Local` | `LocalDocsOptions` | Engine | **Local LLM-native self-docs (primary).** |
| `Projects:Files` | `ProjectFilesOptions` | Engine | Project file storage (disk root). |
| `Auth` | `AuthOptions` | Engine | Registration enabled + seed admin user. |
| `Jwt` | `JwtOptions` | Engine | JWT issuer / audience / lifetimes. |
| `Infisical` | `InfisicalOptions` | Engine | Secret-source config. |
| `ConnectionStrings:Default` | string | API | SQLite path; default `Data Source=gabriel.db`. |

## DETAILS

### `Agent` — `AgentOptions`

```jsonc
{
  "Agent": {
    "MaxIterations": 8,           // ReAct loop ceiling
    "CompactThreshold": 0.8,      // fire compact at θ × W
    "CompactKeepLast": 6          // keep ≥ this many messages post-cut
  }
}
```

### `Personality` — `PersonalityOptions`

```jsonc
{
  "Personality": {
    "Name": "Gabriel",
    "MinThinkingDelayMs": 400,    // SSE typing-tempo
    "MaxThinkingDelayMs": 1100,
    "MinCharsPerSecond": 55,
    "MaxCharsPerSecond": 85
  }
}
```

Typing-tempo knobs are consumed by the SSE controller in `Gabriel.API`, not Engine.

### `Providers:Grok` — `GrokOptions`

```jsonc
{
  "Providers": {
    "Grok": {
      "ApiKey": "<via secret>",
      "BaseUrl": "https://api.x.ai/v1/",
      "TimeoutSeconds": 60,
      "Temperature": 0.85,
      "TopP": 0.9,
      "Models": [
        {
          "Name": "grok-4.3",
          "IsActive": true,
          "ContextWindowTokens": 1000000,
          "CompactThreshold": 0.18,         // per-model override
          "ToolMode": "Native",             // Native | Emulated | None
          "InputPricePerMTokens": 1.25,
          "OutputPricePerMTokens": 2.50,
          "CacheReadPricePerMTokens": 0.0,
          "CacheWritePricePerMTokens": 0.0
        }
      ]
    }
  }
}
```

**Wiring rule**: provider registers ONLY if section exists AND `Models` has at least one entry. `ApiKey` is required (validated at startup via `.Validate(...)`). At most one model can have `IsActive=true`.

**`ToolMode`** declares how tools are transported for this specific model. `Native` (default) uses the provider's first-class `tools` field; `Emulated` wraps the provider with `GabrielToolBridge` so the model speaks XML-tagged JSON in its content stream (used for models whose providers don't expose tool-calling); `None` drops the tool descriptors entirely (chat-only models). See `agent-loop.md` → "Tool transport modes" for the full mechanics.

### `Tools:Docs:Local` — `LocalDocsOptions`

```jsonc
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
```

**Primary source** for `docs_list` / `docs_read`. Path resolution:
1. If absolute and exists → use as-is.
2. Else search up from `Environment.CurrentDirectory` and `AppContext.BaseDirectory` for the relative path.
3. First match wins; missing → empty source + one-time warning.

### `Tools:Docs:GitHub` — `GitHubDocsOptions`

```jsonc
{
  "Tools": {
    "Docs": {
      "GitHub": {
        "Owner": "HueByte",
        "Repo": "Gabriel",
        "Branch": "main",
        "DocsPath": "docs",
        "Token": null,                       // PAT raises rate limit 60 → 5000 req/h
        "ListCacheMinutes": 5
      }
    }
  }
}
```

**Fallback source** behind `LocalDocsLookup`. Note: working-tree folder is `PulsePixel`, but the GitHub remote is `Gabriel`.

### `Tools:Web` — search provider

```jsonc
{
  "Tools": {
    "Web": {
      "Active": "ddg",                       // or "brave"
      "Brave": {
        "ApiKey": "<via secret>",
        "BaseUrl": "https://api.search.brave.com/res/v1/web/search",
        "TimeoutSeconds": 15
      }
    }
  }
}
```

Unknown values log a warning and fall back to DDG.

### `Projects:Files` — `ProjectFilesOptions`

```jsonc
{
  "Projects": {
    "Files": {
      "Root": "./projects-data"              // per-project subdir uses {ProjectId:N}
    }
  }
}
```

### `Auth` — `AuthOptions`

```jsonc
{
  "Auth": {
    "RegistrationEnabled": true,
    "Seed": {
      "Enabled": false,
      "UserName": "",
      "Email": "",
      "Password": ""
    }
  }
}
```

### `Jwt` — `JwtOptions`

```jsonc
{
  "Jwt": {
    "Issuer": "gabriel",
    "Audience": "gabriel",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 30,
    "Key": "<via secret>"
  }
}
```

### `Infisical` — `InfisicalOptions`

```jsonc
{
  "Infisical": {
    "Host": "https://eu.infisical.com",
    "ProjectId": "<id>",
    "Environment": "dev",
    "SecretPath": "/gabriel",
    "ClientId": "<id>",
    "ClientSecret": "<via env>",
    "TimeoutSeconds": 15
  }
}
```

Secrets pulled from Infisical at host startup merge into the configuration provider chain, accessible by their dotted key (e.g., `Providers:Grok:ApiKey`).

## Env-var binding convention

`Microsoft.Extensions.Configuration.EnvironmentVariables` maps `__` → `:`. Examples:

| Env var | Sets |
| --- | --- |
| `PROVIDERS__GROK__APIKEY` | `Providers:Grok:ApiKey` |
| `TOOLS__DOCS__GITHUB__TOKEN` | `Tools:Docs:GitHub:Token` |
| `TOOLS__DOCS__LOCAL__PATH` | `Tools:Docs:Local:Path` |
| `TOOLS__WEB__BRAVE__APIKEY` | `Tools:Web:Brave:ApiKey` |
| `JWT__KEY` | `Jwt:Key` |
| `CONNECTIONSTRINGS__DEFAULT` | `ConnectionStrings:Default` |

## INVARIANTS

- Every options POCO implements `IConfigSection<TSelf>` exposing a static `SectionName`. One source of truth per section.
- `Grok` provider registers ONLY if `Providers:Grok` exists AND has ≥ 1 model.
- At most one Grok model can be `IsActive=true`.
- `Tools:Docs:Local` is the primary docs source; GitHub is the fallback.
- Path-traversal-protected: local and GitHub doc reads reject `.` / `..` / absolute prefixes.

## PITFALLS

- Changing `Tools:Docs:Local:Path` without ensuring the folder is shipped to the deployment will silently fall back to the GitHub source.
- The 5-minute `ListCacheMinutes` means a newly-added doc in GitHub won't show up in `docs_list` immediately.
- Don't put secrets in `appsettings.json` — they belong in env vars or Infisical.

## SEE ALSO

- `architecture.md` — which project owns which options POCO.
- `tools.md` — how docs/web/search configurations are consumed.
