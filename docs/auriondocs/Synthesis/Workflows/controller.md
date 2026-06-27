# Adding a new controller

> *Workflow template auto-derived from 8 existing exemplar(s).*

Reach for this pattern when you need to add a new HTTP endpoint surface to the API: create a controller class that handles requests, takes its collaborators via constructor injection, and returns ActionResult-wrapped DTOs. This pattern is used for small, focused REST controllers that delegate business logic to services and rely on ASP.NET Core MVC for routing and model binding.

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

Controllers live in the API project's Controllers folder: src/api/Gabriel.API/Controllers. Files follow the "NameController.cs" convention (for example, AuthController.cs, ConversationsController.cs, DiagnosticsController.cs), and each file contains a class named {Name}Controller (e.g., AuthController) decorated with [ApiController] and routed under v1/[controller].

## DI wiring

ASP.NET Core will discover controller classes automatically, so you do not need to register controllers by hand. What you do need to register are the service dependencies injected into your controller (e.g., IFooService). Add a single service registration in the API project's composition root where other services are registered — for example, add a line like services.AddScoped<IFooService, FooService>(); to the API project's service registration area so the container can resolve the controller's constructor parameter.

## Existing examples

- [AuthController.cs](Code/src/api/Gabriel.API/Controllers/AuthController.cs.md)
- [ConversationsController.cs](Code/src/api/Gabriel.API/Controllers/ConversationsController.cs.md)
- [DiagnosticsController.cs](Code/src/api/Gabriel.API/Controllers/DiagnosticsController.cs.md)
- [MemoriesController.cs](Code/src/api/Gabriel.API/Controllers/MemoriesController.cs.md)
- [ModelsController.cs](Code/src/api/Gabriel.API/Controllers/ModelsController.cs.md)
- [ProjectFilesController.cs](Code/src/api/Gabriel.API/Controllers/ProjectFilesController.cs.md)
- [ProjectsController.cs](Code/src/api/Gabriel.API/Controllers/ProjectsController.cs.md)
- [SequenceController.cs](Code/src/api/Gabriel.API/Controllers/SequenceController.cs.md)

---
*Synthesised by Aurion on 2026-06-08 22:36:22 UTC*
