namespace Gabriel.Core.Repositories;

// Coordinates persistence across repositories. Services call this once per
// use-case to commit the in-flight changes as a single transaction.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
