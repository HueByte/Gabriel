# Data persistence and repositories

> Repositories, entities, and UoW coordinating persistence for conversations and memory.

> This topic covers the repository and unit-of-work layer that persist conversation and memory domain objects. Readers will learn which concrete repositories exist, what domain types they operate on, how query semantics (user- and project-scoped) are expressed, and where commits actually occur. The patterns here keep EF Core details inside thin adapters ([AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md)) while services depend on small interfaces for testability.

## ConversationRepository.cs
Implements data access for conversations.

The [ConversationRepository](../Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs.md) is a thin EF Core adapter that implements the [IConversationRepository](../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md) contract. It centralizes the common query shapes and mutation helpers for the [Conversation](../Code/src/api/Gabriel.Core/Entities/Conversation.cs.md) aggregate and its Message collection: callers use it for user-scoped reads (by userId), to include messages (the repository offers a GetByIdWithMessagesAsync that applies an Include with an OrderBy for messages), and to add/update/remove conversations and messages. The class mutates the injected [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md) but intentionally does not call SaveChanges/SaveChangesAsync; callers (for example higher-level services or a unit-of-work) are responsible for persisting the changes. The repository depends on the domain Conversation type and its interface [IConversationRepository] for shape and usage expectations.

## MemoryRepository.cs
Implements in-memory persistence for memory entries.

[MemoryRepository](../Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs.md) is a lightweight EF Core-backed repository implementing [IMemoryRepository](../Code/src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md). It exposes common query patterns for MemoryEntry entities scoped to a user and optionally to a project: ListAsync for explicit project or global lists, FindByNameAsync which matches Name and ProjectId exactly within a UserId, and ListForAgentAsync which encodes the agent-facing view by returning the user's global entries plus a project’s entries (when a projectId is provided) in a single query with ordering that places global entries first then project-scoped entries. Like the conversation repository, its AddAsync/Update/Remove methods only change the DbContext state and do not commit; callers must persist with an ambient unit-of-work or direct AppDbContext save.

## UnitOfWork.cs
Coordinates transactional changes across repositories.

[UnitOfWork](../Code/src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs.md) is the concrete implementation of [IUnitOfWork](../Code/src/api/Gabriel.Core/Repositories/IUnitOfWork.cs.md) in this codebase. It encapsulates an [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md) and exposes a single asynchronous SaveChangesAsync operation that forwards the provided CancellationToken to the context’s SaveChangesAsync, returning the number of state entries written. It is intended to be the coordination point so multiple repositories that share the same context can persist their accumulated changes together; it does not create its own cross-context transaction scope, so transactional semantics are those provided by the underlying context/provider.

## Conversation.cs
Represents a conversation domain entity.

[Conversation](../Code/src/api/Gabriel.Core/Entities/Conversation.cs.md) is the aggregate root for a chat thread owned by a user. It holds metadata (Title, timestamps, AvatarSeed, optional PatternOverride/PaletteOverride), a collection of Message entities (exposed as an IReadOnlyList), optional ProjectId, a rolling summary with summarized-through pointer, and a serialized StateJson for evolving state that is not modeled as discrete columns. The class is designed for EF Core materialization (private parameterless constructor) and exposes a factory method Create that enforces non-empty userId and projectId for new conversations and initializes stable defaults such as Id and AvatarSeed; repository code is expected to filter by Conversation.UserId to enforce per-user isolation.

## IConversationRepository.cs
Defines repository interface for conversations.

[IConversationRepository](../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md) defines the tenant-aware contract for working with Conversation aggregates and their Message entities. The interface requires reads to include a userId to guarantee user-scoped access, exposes methods to fetch a conversation (with or without its messages), list a user’s conversations (optionally filtered by project), and perform explicit create/update/delete operations on conversations and messages. The design intentionally separates AddMessage and RemoveMessages to make message lifecycle explicit and to ensure EF Core’s change tracker records deletions reliably rather than relying on implicit orphan removal.

## IMemoryRepository.cs
`IMemoryRepository` is the interface/implementation counterpart of `MemoryRepository`, which this topic covers.

[IMemoryRepository](../Code/src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md) is the abstraction for storing and querying MemoryEntry objects with user- and project-scoped visibility. Its surface includes listing the entries visible to an agent (ListForAgentAsync), listing or filtering by project scope, finding a memory by name within a specific (userId, projectId) scope (FindByNameAsync), and creating/updating/removing entries. Implementations are expected to rely on an ambient unit-of-work or change-tracking context for persisting Update/Remove operations; AddAsync is asynchronous while Update/Remove are synchronous in the interface.

## IUnitOfWork.cs
`IUnitOfWork` is the interface/implementation counterpart of `UnitOfWork`, which this topic covers.

[IUnitOfWork](../Code/src/api/Gabriel.Core/Repositories/IUnitOfWork.cs.md) defines the contract that coordinates persistence across repositories by exposing SaveChangesAsync(CancellationToken) which returns the number of state entries written. Services perform repository updates and then invoke this single commit point to persist the combined changes. The abstraction hides EF Core specifics from callers and improves testability by enabling mock or fake implementations in unit tests.

## AppDbContext.cs
`AppDbContext` collaborates directly with `Conversation` and other members of this topic (4 dependency links).

[AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md) is the EF Core DbContext used across the persistence layer. It inherits from IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid> so Identity tables are wired into the model and it exposes DbSet properties for Conversations, Messages, MemoryEntries, RefreshTokens, Projects, ProjectFiles, and MetricEntries. In OnModelCreating it applies identity configuration via the base and then loads entity configurations from the assembly with ApplyConfigurationsFromAssembly; ConfigureConventions applies a DateTimeOffsetToBinaryConverter to persist DateTimeOffset values safely on providers (like SQLite) that lack native support. Repositories and the UnitOfWork operate against this context; it is intended to be registered in DI with an appropriate scope so repositories share the same context instance for a coordinated commit.

How the pieces fit

Repositories ([ConversationRepository](../Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs.md) and [MemoryRepository](../Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/MemoryRepository.cs.md)) are thin adapters over [AppDbContext](../Code/src/api/Gabriel.Infrastructure/Persistence/AppDbContext.cs.md) that encapsulate common query shapes and ownership filtering (userId and optional projectId). They mutate the shared DbContext state but purposely do not persist changes; the single commit point is [UnitOfWork](../Code/src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs.md) which implements [IUnitOfWork](../Code/src/api/Gabriel.Core/Repositories/IUnitOfWork.cs.md) and forwards SaveChangesAsync to AppDbContext. The [IConversationRepository](../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md) and [IMemoryRepository](../Code/src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md) interfaces decouple services from EF Core and encourage reuse and testability while the [Conversation](../Code/src/api/Gabriel.Core/Entities/Conversation.cs.md) entity models the aggregate root and message ownership that the repositories enforce.

---
*Covers 8 of 8 source files identified for this topic.*

*Synthesised by Aurion on 2026-07-08 05:45:56 UTC*
