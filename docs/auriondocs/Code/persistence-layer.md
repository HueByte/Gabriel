# Persistence layer and data access

> EF Core DbContext and repositories handling persistence across domain entities.

A concise orientation to the persistence layer: this guide explains the EF Core DbContext and repository implementations that encapsulate domain data access and transaction boundaries. It shows which concrete types expose query and mutation operations, how they treat change-tracking versus committing, and where transaction/save semantics live so application services can coordinate multi-step updates safely.

## ConversationRepository.cs
Implements data access for conversations.
The [ConversationRepository](../Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs.md) is a concrete EF Core repository that implements IConversationRepository and exposes CRUD-style operations for Conversation and Message entities against the application's [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md). It defines query methods such as GetByIdWithMessagesAsync (which can include the Messages navigation and applies an OrderBy on CreatedAt) and list operations for a user's conversations, plus mutation helpers like AddAsync, AddMessage, Update, Remove, and RemoveMessages that only touch the DbContext change tracker. The repository intentionally does not call SaveChanges/SaveChangesAsync; callers (typically application services) must persist via the DbContext or an IUnitOfWork implementation. This class depends on and delegates EF Core operations to the [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md).

## MemoryRepository.cs
Implements in-memory storage for memory-related data.
The [MemoryRepository](../Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs.md) implements IMemoryRepository and encapsulates query and basic mutation operations for MemoryEntry entities scoped by user and optionally by project. It provides read methods such as ListForAgentAsync (which constructs a single IQueryable to return both user-level (null project) and project-scoped entries in one query, ordering user-scope entries first) and FindByNameAsync, and mutation methods like AddAsync, Update, and Remove that only affect the AppDbContext change tracker. Like the conversation repository, it does not persist changes itself; callers must call SaveChanges/SaveChangesAsync on the [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md). The class depends on the [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md) for querying and tracking.

## UnitOfWork.cs
Coordinates repositories and transactional boundaries.
The [UnitOfWork](../Code/src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs.md) is a thin IUnitOfWork implementation that wraps the [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md) and exposes a single SaveChangesAsync method to commit all tracked changes in one call. It purposefully avoids implementing explicit transaction APIs itself and relies on AppDbContext.SaveChangesAsync to perform the commit (which will be a single database transaction per call as provided by EF Core). By depending on IUnitOfWork rather than DbContext directly, higher-level services can be tested or swapped without touching EF Core; however, UnitOfWork does not provide query methods—repositories remain responsible for composing queries and mutating the context.

## AppDbContext.cs
Defines EF Core DbContext for identity and domain entities.
The [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md) is the central EF Core DbContext that inherits from IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid> and exposes DbSets for Conversations, Messages, RefreshTokens, Projects, ProjectFiles, MemoryEntries, and MetricEntries. It applies entity configurations from the assembly to keep mappings centralized and uses a DateTimeOffsetToBinaryConverter to persist DateTimeOffset as a binary/long for compatibility with providers like SQLite. This DbContext is the persistence boundary used by the repositories and by the [UnitOfWork](../Code/src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs.md); it is registered with DI and its lifetime must be compatible with the UnitOfWork and repository usages.

How the pieces fit
Repositories ([ConversationRepository](../Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs.md) and [MemoryRepository](../Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs.md)) encapsulate query composition and mutations against the [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md) but intentionally stop at the change tracker. The [UnitOfWork](../Code/src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs.md) provides the single SaveChangesAsync commit point so application services can group multiple repository operations into one persistence boundary. Dependency direction is from repositories and the UnitOfWork toward the AppDbContext; application services call repositories to read/mutate and call the UnitOfWork (or DbContext) to persist.

---
*Synthesised by Aurion on 2026-07-07 18:12:20 UTC*
