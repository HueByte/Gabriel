# GlobalRoutePrefixConvention

> **File:** `src/api/Gabriel.API/Configuration/GlobalRoutePrefixConvention.cs`  
> **Kind:** class

```csharp
public class GlobalRoutePrefixConvention : IApplicationModelConvention
```


GlobalRoutePrefixConvention is an ASP.NET Core MVC application model convention that prepends a fixed route prefix to every controller route. It centralizes routing concerns by defining the base path in one place (for example "api") and applying it to all controllers, so endpoints share a consistent base URL without repeating the prefix on each Route attribute. During application model construction, it iterates all controllers and their selectors and either assigns the prefix when a controller has no explicit route, or combines the prefix with the existing route using AttributeRouteModel.CombineAttributeRouteModel.

## Remarks
By moving the base path into this convention, teams avoid drift between controllers and simplify versioning or scoping of APIs. It collaborates with other MVC conventions and the Route attribute system by layering the prefix under the existing route definitions, rather than replacing them.

## Example
```csharp
// Common usage
services.AddControllers(options =>
{
    options.Conventions.Add(new GlobalRoutePrefixConvention("api"));
});
```

## Notes
- All routes defined on controllers and actions will be prefixed with the base path (e.g. "api/values").
- There is no per-action opt-out in this convention; to bypass the prefix you would need a different approach or custom convention.