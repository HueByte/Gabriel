# GlobalRoutePrefixConvention

> **File:** `src/api/Gabriel.API/Configuration/GlobalRoutePrefixConvention.cs`  
> **Kind:** class

Prepends a fixed route segment to every controller's attribute route so you can declare a common prefix (for example "api" or "api/v1") in one place instead of repeating it on every controller.

## Remarks
Centralizes routing concerns by applying a single prefix to all controllers that use attribute routing. Implementing IApplicationModelConvention allows this to run during MVC's application model construction so controllers keep their individual routes while gaining a consistent global prefix. Registering this convention on MVC options prevents route drift and reduces duplication across controllers.

## Example
```csharp
// Register the convention when configuring MVC services (Program.cs or Startup.cs)
services.AddControllers(options =>
{
    options.Conventions.Add(new GlobalRoutePrefixConvention("api"));
});

// Controller routes remain simple — the global prefix is applied automatically
[Route("products")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok();
}

// Resulting route for GetAll: GET /api/products
```

## Notes
- Avoid leading or trailing slashes in the prefix (use "api" or "api/v1"), since combining model routes expects simple segments and inconsistent slashes can produce unexpected routes.
- This convention only affects attribute-routed controllers (selector.AttributeRouteModel). Conventional routes defined via MapControllerRoute / endpoint routing are not modified.
- The prefix is applied at application model build time; changing it requires application restart or reconfiguration during startup.