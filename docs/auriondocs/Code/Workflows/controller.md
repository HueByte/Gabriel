# Adding a new controller

> *Workflow template auto-derived from 8 existing exemplar(s).*

Adding a new controller

When you need to expose a new HTTP surface for a resource or feature in the API, add a controller that follows the project's existing conventions. A controller in this codebase is a thin HTTP adapter: it declares routes and HTTP verbs, depends on service abstractions (injected via constructor), and returns ActionResult-wrapped DTOs so callers get proper status codes.

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

Controllers live in the API project's Controllers folder: src/api/Gabriel.API/Controllers. Follow the established naming convention: the class and filename end with "Controller" (for example, ProjectsController), the class is decorated with [ApiController] and [Route("v1/[controller]")], and it inherits from ControllerBase. Controller methods use HTTP verb attributes (e.g., [HttpGet], [HttpPost]) and model parameters (route, query, body) as in the exemplars.

## DI wiring

You do not register controllers individually; the MVC pipeline discovers controllers in the Gabriel.API assembly. What you must register is the controller's constructor dependencies (the IFooService in the scaffold). Add a single registration line in the Gabriel.API project's DI composition root where services are configured—for example, a one-line registration such as:

services.AddScoped<IFooService, FooService>();

This ensures the concrete service is provided when the controller is constructed.

## Existing examples

- [`AuthController`](../../Code/src/api/Gabriel.API/Controllers/AuthController.cs.md)
- [`ConversationsController`](../../Code/src/api/Gabriel.API/Controllers/ConversationsController.cs.md)
- [`DiagnosticsController`](../../Code/src/api/Gabriel.API/Controllers/DiagnosticsController.cs.md)
- [`MemoriesController`](../../Code/src/api/Gabriel.API/Controllers/MemoriesController.cs.md)
- [`ModelsController`](../../Code/src/api/Gabriel.API/Controllers/ModelsController.cs.md)
- [`ProjectFilesController`](../../Code/src/api/Gabriel.API/Controllers/ProjectFilesController.cs.md)
- [`ProjectsController`](../../Code/src/api/Gabriel.API/Controllers/ProjectsController.cs.md)
- [`SequenceController`](../../Code/src/api/Gabriel.API/Controllers/SequenceController.cs.md)

---
*Synthesised by Aurion on 2026-07-07 18:13:24 UTC*
