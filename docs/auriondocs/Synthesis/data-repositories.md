# Persistence layer and repositories

> EF Core entity mappings and repository patterns for persisting conversations, memories, and related data; includes a unit of work for coordinated saves.

This guide describes the persistence layer and repository patterns used to store conversations, memory entries, and related data with Entity Framework Core. It highlights the EF Core entity mapping for conversations, the concrete repository implementations for conversations and memory entries, and the Unit of Work that provides a single commit point for coordinated saves. Use this guide to understand where data access logic lives and how repositories and configuration classes interact with an AppDbContext to persist domain aggregates.

## ConversationRepository.cs

Provides data access for Conversation aggregates using EF Core.

ConversationRepository is the concrete EF Core repository that performs queries and updates against Conversation aggregates (including their messages) using the shared AppDbContext. When you need to load conversation graphs, apply filters, add or remove messages, or update conversation metadata, this class centralizes those operations and translates domain intent into EF Core queries. It is the primary entry point for higher-level services that operate on conversations and relies on the entity mapping defined in [ConversationConfiguration.cs](Code/src/api/Gabriel.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs.md) and the scoped DbContext that the UnitOfWork coordinates.

## MemoryRepository.cs

Provides an EF Core-backed repository for MemoryEntry records.

MemoryRepository offers a thin data-access abstraction for persisting and querying MemoryEntry records, typically scoped by user and optionally by project. It encapsulates common query patterns and persistence concerns for memory entries so service code does not use the DbContext directly. Use this repository when your application code needs to store, update, or enumerate memory entries; like the conversation repository it depends on the same AppDbContext and participates in coordinated commits handled by the UnitOfWork.

## UnitOfWork.cs

Coordinates persistence across repositories and defines a single commit point.

UnitOfWork is a small adapter that implements the application IUnitOfWork abstraction by delegating SaveChangesAsync to an injected AppDbContext. It provides a single commit boundary for multiple repository operations so callers can perform several changes across repositories and then call one commit method to persist them atomically (as supported by EF Core and the underlying transaction behavior). Prefer depending on this abstraction in services and handlers to decouple business logic from the concrete DbContext and to make coordinated saves explicit.

## ConversationConfiguration.cs

Configures EF Core mappings for the Conversation entity.

ConversationConfiguration declares the EF Core mapping rules for the Conversation entity: table name, primary key, property constraints (required and max-length settings), and indexes used by common queries, as well as mapping for optional fields like rolling summaries. This configuration ensures the database schema and EF Core model align with how the repositories query and update conversations; the settings here (indexes, nullability, lengths) directly influence query performance and correctness when [ConversationRepository.cs](Code/src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs.md) and other data-access classes materialize conversation data.

These pieces collaborate in a straightforward request-flow: repositories (ConversationRepository and MemoryRepository) encapsulate EF Core queries and updates and depend on the AppDbContext mapping established by ConversationConfiguration, while UnitOfWork provides a single commit point that application code calls after performing repository operations. Together they enforce a clear separation between domain persistence logic (repositories), the EF Core model (configurations), and transaction/commit concerns (unit of work), enabling coordinated, testable data access across the application.

---
*Synthesised by Aurion on 2026-06-08 22:35:19 UTC*
