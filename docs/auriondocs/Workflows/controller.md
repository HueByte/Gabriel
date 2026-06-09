# Adding a new controller

> *Workflow template auto-derived from 8 existing exemplar(s).*

Add a new controller when you need a new HTTP endpoint surface — typically to expose a resource or coordinate operations that belong to a single domain concept. Controllers in this codebase are lightweight ASP.NET Core API controllers that accept services via constructor injection and return ActionResult-wrapped DTOs; follow the existing folder, naming, and routing conventions so the framework and teammates can find and wire your new controller consistently.

## Scaffold

```csharp
using Microsoft.AspNetCore.Mvc;

namespace YourProject.Controllers;

/// <summary>
/// Replace the summary with what this controller is for.
/// </summary>
[ApiController]
[Route("v1/[controller]")]
public class FooController : ControllerBase
{
    private readonly IFooService _fooService;

    public FooController(IFooService fooService)
    {
        _fooService = fooService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FooDto>> Get(Guid id, CancellationToken ct)
    {
        var result = await _fooService.GetAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
```

## Where it lives

Controllers live under src/api/Gabriel.API/Controllers and follow the naming convention {Resource}Controller.cs with a public class named {Resource}Controller. They are decorated with [ApiController] and typically use [Route("v1/[controller]")] so the class name determines the route segment. Keep action methods small and return ActionResult<T> DTOs (as shown in the scaffold) so responses and status codes are explicit and consistent with existing controllers.

## DI wiring

ASP.NET Core will discover controllers automatically, but any constructor-injected dependency (for example IFooService in the scaffold) must be registered with the API project's dependency-injection container. Add a single service registration in the API project's composition root (e.g., the Program.cs composition section) such as: builder.Services.AddScoped<IFooService, FooService>(); — choose AddScoped/AddTransient/AddSingleton to match the service's intended lifecycle. Also add the IFooService interface and its implementation to the appropriate project if they don't already exist, then register the implementation once so the controller can be constructed.

## Existing examples

- [AuthController.cs](src/api/Gabriel.API/Controllers/AuthController.cs.md) — primary symbol AuthController. AuthController is a representative controller in src/api/Gabriel.API/Controllers.
- [ConversationsController.cs](src/api/Gabriel.API/Controllers/ConversationsController.cs.md) — primary symbol ConversationsController. ConversationsController is a representative controller in src/api/Gabriel.API/Controllers.
- [DiagnosticsController.cs](src/api/Gabriel.API/Controllers/DiagnosticsController.cs.md) — primary symbol DiagnosticsController. DiagnosticsController is a representative controller in src/api/Gabriel.API/Controllers.
- [MemoriesController.cs](src/api/Gabriel.API/Controllers/MemoriesController.cs.md) — primary symbol MemoriesController. MemoriesController is a representative controller in src/api/Gabriel.API/Controllers.
- [ModelsController.cs](src/api/Gabriel.API/Controllers/ModelsController.cs.md) — primary symbol ModelsController. ModelsController is a representative controller in src/api/Gabriel.API/Controllers.
- [ProjectFilesController.cs](src/api/Gabriel.API/Controllers/ProjectFilesController.cs.md) — primary symbol ProjectFilesController. ProjectFilesController is a representative controller in src/api/Gabriel.API/Controllers.
- [ProjectsController.cs](src/api/Gabriel.API/Controllers/ProjectsController.cs.md) — primary symbol ProjectsController. ProjectsController is a representative controller in src/api/Gabriel.API/Controllers.
- [SequenceController.cs](src/api/Gabriel.API/Controllers/SequenceController.cs.md) — primary symbol SequenceController. SequenceController is a representative controller in src/api/Gabriel.API/Controllers.

---
*Synthesised by Aurion on 2026-06-09 03:24:49 UTC*
