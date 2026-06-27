Registers the infrastructure services required by the application into an IServiceCollection during startup. This method configures the EF Core DbContext (SQLite), registers the Unit of Work and repository implementations with scoped lifetimes, binds ProjectFilesOptions from configuration, registers a disk-backed project file service, and delegates provider-specific registrations (chat, web search, web fetch, docs lookup) to helper methods.

## Remarks
This method acts as the composition root for the infrastructure layer: it centralizes wiring of persistence, file storage, and provider integrations so that startup code stays concise and consistent. ProjectFilesOptions are bound from the configuration section named by ProjectFilesOptions.SectionName and the registered DiskProjectFileService uses those options to persist project files under the configured root (the implementation stores files under {Root}/{ProjectId:N} as noted in the source).

## Example
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();
```

## Notes
- If the configuration does not contain a "Default" connection string, the method falls back to "Data Source=gabriel.db" — a local SQLite file created in the application's working directory.
- The UnitOfWork, repositories, and the DbContext are registered with scoped lifetimes; do not resolve them from singletons.
- The project requires the EF Core SQLite provider (e.g. Microsoft.EntityFrameworkCore.Sqlite) and appropriate migrations if you intend to use a persistent database rather than the default file.