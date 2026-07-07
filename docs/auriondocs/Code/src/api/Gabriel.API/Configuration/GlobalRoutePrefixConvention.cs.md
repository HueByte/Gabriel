# GlobalRoutePrefixConvention

> **File:** `src/api/Gabriel.API/Configuration/GlobalRoutePrefixConvention.cs`  
> **Kind:** class

```csharp
public class GlobalRoutePrefixConvention : IApplicationModelConvention
```


Applies a single, fixed prefix to every controller route by implementing IApplicationModelConvention, ensuring all endpoint routes share a centralized prefix without duplicating it in each [Route] attribute. Use it when you want a consistent API root (for example 'api') across all controllers and to avoid repeating the prefix in every Route attribute.

## Remarks
GlobalRoutePrefixConvention centralizes routing concerns by turning a per-controller prefix into a shared policy. If a controller already defines its own route, the convention merges the new prefix with the existing route via CombineAttributeRouteModel, preserving existing routing semantics. Because it operates at the ApplicationModel level, the convention applies uniformly across all controllers without modifying individual action attributes.

## Notes
- If a controller has no Route attribute, the prefix becomes the controller's route.
- If a controller defines an existing Route attribute, the prefix is merged with it via CombineAttributeRouteModel, preserving the original routing semantics.
- Changing the prefix requires restarting the application to rebind routes.