# IProjectService

> **File:** `src/api/Gabriel.Core/Services/IProjectService.cs`  
> **Kind:** interface

*Figure: How IProjectService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
IService["IProjectService (interface)"]
List["IProjectService.ListAsync\nLazily ensures 'Default' project exists; absorbs legacy conversations; returns user's projects (repo-scoped)"]
Ensure["IProjectService.EnsureDefaultProjectIdAsync\nCreate 'Default' if missing; assign project-less conversations; return Default project id"]
Get["IProjectService.GetAsync(Guid id)\nReturn Project"]
GetFiles["IProjectService.GetWithFilesAsync(Guid id)\nLoad files; sort newest-first; return Project"]
Create["IProjectService.CreateAsync(name, description, systemPrompt)\nCreate and return Project"]
Update["IProjectService.RenameAsync / UpdateDescriptionAsync / UpdateSystemPromptAsync\nMutate Project fields"]
Skin["IProjectService.SetSkinAsync(Guid id, pattern?, palette?)\nPin overrides; null or empty clears that dimension"]
Avatar["IProjectService.RerollAvatarAsync(Guid id)\nRe-roll AvatarSeed -> changes seed-derived pattern/palette"]
Delete["IProjectService.DeleteAsync(Guid id)\nDelete Project"]

IService --> List
List --|"lazily calls EnsureDefaultProjectIdAsync on first call"| Ensure
IService --> Get
IService --> GetFiles
IService --> Create
IService --> Update
IService --> Skin
IService --> Avatar
IService --> Delete
```

```csharp
public interface IProjectService
```


Manages projects for the current user within the repository-scoped, multi-tenant domain. Use this service when you need to list, read, create, mutate, or delete projects (including project-level visual settings like avatar skin), or when you need to ensure or obtain the user's default project and migrate legacy, project-less conversations into it.

## Remarks
This interface centralizes project lifecycle and presentation concerns so higher-level code doesn't need to manipulate persistence directly or reimplement migration behaviour. Several members perform lazy or side-effecting work: ListAsync and EnsureDefaultProjectIdAsync will create a per-user "Default" project on first access and reassign legacy conversations into it; RerollAvatarAsync and SetSkinAsync control seed-derived and pinned avatar dimensions respectively. The API layer validates catalog identifiers before calling SetSkinAsync, so callers can expect invalid pattern/palette ids to be rejected earlier in the stack.

## Example
```csharp
// Ensure a default project exists, then list projects and update the system prompt
public async Task EnsureAndConfigureDefault(IProjectService projects, CancellationToken ct)
{
    var defaultId = await projects.EnsureDefaultProjectIdAsync(ct);
    var all = await projects.ListAsync(ct);

    var defaultProject = all.FirstOrDefault(p => p.Id == defaultId);
    if (defaultProject != null)
    {
        await projects.UpdateSystemPromptAsync(defaultId, "You are an assistant that explains code clearly.", ct);
    }
}
```

## Notes
- ListAsync and EnsureDefaultProjectIdAsync are lazy: they may create the "Default" project and migrate legacy, project-less conversations on first call.
- SetSkinAsync treats null or empty strings as "clear this override" for pattern or palette; passing null/empty falls back to seed-derived behaviour.
- GetWithFilesAsync returns the project with its files loaded and sorted newest-first; callers who care about file order can rely on that ordering.