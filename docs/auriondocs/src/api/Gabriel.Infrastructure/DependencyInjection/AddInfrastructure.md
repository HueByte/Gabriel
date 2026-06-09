Registers the application's infrastructure services into an IServiceCollection. Use this extension from your application's startup (Program.cs or Startup.cs) to wire up persistence (AppDbContext using SQLite), repository and unit-of-work implementations, project file storage options and service, and to add the chat provider, web search, web fetch and documentation lookup integrations.

## Remarks
This method centralizes DI registrations for the infrastructure layer so the host-level composition code remains concise. It binds ProjectFilesOptions from configuration and registers a disk-backed project file implementation that persists files under {Root}/{ProjectId:N}. Repositories and the unit-of-work are registered with scoped lifetimes (appropriate for per-request usage in web apps). The helper calls (AddChatProvider, AddWebSearch, AddWebFetch, AddDocsLookup) encapsulate additional provider-specific registrations so those concerns stay decoupled from the core registrations here.

## Example
```csharp
var builder = WebApplication.CreateBuilder(args);
// registers DbContext, repositories, project file service, and external integrations
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
// continue application setup
```

## Notes
- If no "Default" connection string is configured, the method falls back to a local SQLite file: "Data Source=gabriel.db" — verify your configuration to avoid unintentionally using the fallback.
- This method only registers the DbContext; it does not apply migrations or initialize the database. Run migrations or call EnsureCreated/EnsureMigrated elsewhere as appropriate.
- Services are registered as scoped. If you consume them from a background thread or singleton, create an IServiceScope (via IServiceScopeFactory) to avoid captive dependency issues.
