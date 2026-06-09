Registers the infrastructure services required by the application into an IServiceCollection: database context, repositories, unit-of-work, project file storage options and implementation, and several feature-specific providers (chat, web search, web fetch, docs lookup). Use this extension during application startup (for example in Program.cs) to centralize all infrastructure-related DI registrations instead of adding them individually.

## Remarks
This method centralizes wiring of persistence and cross-cutting infrastructure so calling code does not need to know the concrete implementations. It configures an EF Core AppDbContext using SQLite (connection string read from configuration key "Default", falling back to a local file), registers repository and unit-of-work types with Scoped lifetimes, binds ProjectFilesOptions from configuration and registers a disk-backed project file service. It also delegates additional feature registrations to helper methods (AddChatProvider, AddWebSearch, AddWebFetch, AddDocsLookup), so those features must be available on the DI call chain.

## Example
```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);
// ensure configuration contains connection strings and Projects:Files section as needed
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
// ...
```

## Notes
- If no connection string named "Default" is present, the code falls back to SQLite file "gabriel.db" in the current working directory — suitable for local development but likely not for production.
- The AppDbContext is configured for SQLite; provider-specific behavior (migrations, SQL features) will follow SQLite semantics.
- Repositories and the IUnitOfWork are registered with Scoped lifetime; resolve them from scoped contexts (e.g., per-request in web apps).
- ProjectFilesOptions are bound from the configuration section named by ProjectFilesOptions.SectionName (Projects:Files per the inline comment), and the DiskProjectFileService persists files under {Root}/{ProjectId:N} as the implementation expects.
- This method assumes the helper registration methods (AddChatProvider, AddWebSearch, AddWebFetch, AddDocsLookup) are present and properly register their dependencies; missing implementations will cause runtime registration errors.
