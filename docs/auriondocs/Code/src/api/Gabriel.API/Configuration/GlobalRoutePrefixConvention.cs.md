# GlobalRoutePrefixConvention

> **File:** `src/api/Gabriel.API/Configuration/GlobalRoutePrefixConvention.cs`  
> **Kind:** class

```csharp
public class GlobalRoutePrefixConvention : IApplicationModelConvention
```


GlobalRoutePrefixConvention centralizes the API base path by prepending a fixed prefix to every controller route. It allows you to define action routes without repeating the base path in each Route attribute and keeps routing concerns in one place to prevent drift between controllers.

## Remarks
It lives in the MVC configuration as an IApplicationModelConvention, modifying selector routes so every endpoint inherits the same base prefix. By consolidating the routing prefix logic here, changes to the API base path require only this one location, minimizing drift and mistakes across controllers.

## Notes
- If an action or controller already defines a full route template, the final route is the prefix combined with that template; verify resulting URLs to avoid surprises.
- Use a relative prefix (e.g., "api") rather than an absolute "/api" to align with how AttributeRouteModel.CombineAttributeRouteModel behaves.