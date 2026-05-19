using Gabriel.Core.Entities;
using Gabriel.Core.Exceptions;
using Gabriel.Core.Identity;
using Gabriel.Core.Repositories;

namespace Gabriel.Core.Services;

public class ProjectService : IProjectService
{
    public const string DefaultProjectName = "Default";

    private readonly IProjectRepository _projects;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public ProjectService(IProjectRepository projects, IUnitOfWork uow, ICurrentUser currentUser)
    {
        _projects = projects;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct = default)
    {
        var userId = RequireUserId();
        // Ensure Default exists before listing so a brand-new user always sees
        // a non-empty list (and any legacy project-less conversations attach
        // automatically).
        await EnsureDefaultInternalAsync(userId, ct);
        return await _projects.ListAsync(userId, ct);
    }

    public async Task<Project> GetAsync(Guid id, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        return await _projects.GetByIdAsync(id, userId, ct)
            ?? throw new NotFoundException(nameof(Project), id);
    }

    public async Task<Project> GetWithFilesAsync(Guid id, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        return await _projects.GetByIdWithFilesAsync(id, userId, ct)
            ?? throw new NotFoundException(nameof(Project), id);
    }

    public async Task<Project> CreateAsync(string name, string? description, string? systemPrompt, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var project = Project.Create(userId, name, description, systemPrompt);
        await _projects.AddAsync(project, ct);
        await _uow.SaveChangesAsync(ct);
        return project;
    }

    public async Task<Project> RenameAsync(Guid id, string name, CancellationToken ct = default)
    {
        var project = await GetAsync(id, ct);
        project.Rename(name);
        _projects.Update(project);
        await _uow.SaveChangesAsync(ct);
        return project;
    }

    public async Task<Project> UpdateDescriptionAsync(Guid id, string? description, CancellationToken ct = default)
    {
        var project = await GetAsync(id, ct);
        project.UpdateDescription(description);
        _projects.Update(project);
        await _uow.SaveChangesAsync(ct);
        return project;
    }

    public async Task<Project> UpdateSystemPromptAsync(Guid id, string? systemPrompt, CancellationToken ct = default)
    {
        var project = await GetAsync(id, ct);
        project.UpdateSystemPrompt(systemPrompt);
        _projects.Update(project);
        await _uow.SaveChangesAsync(ct);
        return project;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await GetAsync(id, ct);
        _projects.Remove(project);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<Guid> EnsureDefaultProjectIdAsync(CancellationToken ct = default)
    {
        var userId = RequireUserId();
        return await EnsureDefaultInternalAsync(userId, ct);
    }

    // Idempotent: returns the existing Default project's id, or creates one
    // (and back-fills the user's legacy project-less conversations) on first
    // call. Cheap on every subsequent call (one indexed FirstOrDefault).
    private async Task<Guid> EnsureDefaultInternalAsync(Guid userId, CancellationToken ct)
    {
        var existing = await _projects.GetFirstByNameAsync(userId, DefaultProjectName, ct);
        if (existing is not null) return existing.Id;

        var project = Project.Create(
            ownerUserId: userId,
            name: DefaultProjectName,
            description: "Default project for everything not categorized elsewhere.",
            systemPrompt: null);

        await _projects.AddAsync(project, ct);
        // Save first so the new Project.Id is committed before the bulk-update
        // tries to reference it.
        await _uow.SaveChangesAsync(ct);

        // One-shot legacy backfill: any project-less conversations the user
        // already has get attached to Default.
        await _projects.AssignOrphanConversationsAsync(userId, project.Id, ct);

        return project.Id;
    }

    private Guid RequireUserId()
        => _currentUser.UserId ?? throw new UnauthorizedAccessException("Authenticated user required.");
}
