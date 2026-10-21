namespace Gabriel.Core.Identity;

// Lets code that runs outside an HTTP request (the autonomous-task worker) act
// as a specific user for the rest of a DI scope. ICurrentUser implementations
// consult this first, so every user-scoped service downstream - preferences,
// memory, project authorization - works unchanged in the background.
public sealed class CurrentUserOverride
{
    public Guid? UserId { get; private set; }

    public void Set(Guid userId)
    {
        if (UserId is { } existing && existing != userId)
            throw new InvalidOperationException("The current user of a scope cannot be changed once set.");
        UserId = userId;
    }
}
