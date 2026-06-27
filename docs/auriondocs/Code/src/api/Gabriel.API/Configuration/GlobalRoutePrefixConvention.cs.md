# GlobalRoutePrefixConvention

> **File:** `src/api/Gabriel.API/Configuration/GlobalRoutePrefixConvention.cs`  
> **Kind:** class

Prepends a fixed route prefix (for example "api" or "api/v1") to every controller's attribute route so controllers can be authored without repeating the common leading segment. Reach for this when you want a single, centralized place to enforce a shared route prefix (versioning, API root, etc.) across all attribute-routed controllers instead of adding the segment to every [Route] attribute.

## Remarks
Centralizes the routing concern of a shared prefix so controllers remain focused on their own resource paths. Implementing IApplicationModelConvention lets this run during application model construction and either combine the prefix with an existing attribute route or assign the prefix as the controller's route when none exists. This prevents accidental drift (some controllers using the prefix, others not) and keeps route-versioning or root segments consistent.

## Example
```csharp
// In Startup.cs / Program.cs (ConfigureServices):
services.AddControllers(options =>
{
    options.Conventions.Add(new GlobalRoutePrefixConvention("api"));
});
```

## Notes
- If a controller selector has no attribute route, the convention will set the controller's AttributeRouteModel to the prefix alone — ensure your actions have appropriate attribute routes or templates so endpoints are reachable.
- Provide the prefix as you would to a [Route] attribute (e.g., "api" or "api/v1"); it is treated as a route template.
- This convention runs at startup when the application model is built; changing routes at runtime after startup is not supported by this mechanism.